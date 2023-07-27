using Microsoft.Win32;
using PropertyChanged;
using ServiceTool.Common;
using ServiceTool.Helper;
using ServiceTool.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ServiceTool.VM
{
    [AddINotifyPropertyChangedInterface]
    public class VMMain
    {

        public ObservableCollection<VMConfig> ServiceList { get; set; } = new ObservableCollection<VMConfig>();

        public bool ISSelectAll { get; set; } = false;
        public string SearchServiceName { get; set; }


        public ICommand DetectCommand { get; set; }
        public ICommand ExportCommand { get; set; }
        public ICommand AutoRestartCommand { get; set; }
        public ICommand AutoStartCommand { get; set; }
        public ICommand AutoStopCommand { get; set; }
        public ICommand SelectALLCommand { get; set; }


        public VMMain()
        {
            DetectCommand = new BaseCommand((para) =>
            {
                string key = @"SYSTEM\CurrentControlSet\Services\";
                var services = Registry.LocalMachine.OpenSubKey(key);
                if (services == null)
                {
                    return;
                }
                var serviceNameList = services.GetSubKeyNames().ToList();
                if (serviceNameList != null && serviceNameList.Count > 0)
                {
                    //serviceNameList= serviceNameList.Where(f=>f.Contains("K3CloudClienter")).ToList();
                    foreach (var serviceName in serviceNameList)
                    {
                        //if (!serviceName.Contains("MES.Server.ManufactureData.WebAPI"))

                        bool fetch = false;
                        if (string.IsNullOrEmpty(SearchServiceName))
                        {
                            fetch = true;
                        }
                        else
                        {

                            if (serviceName.Contains(SearchServiceName))
                            {
                                fetch = true;
                            }
                        }

                        if (!fetch)
                        {
                            continue;
                        }

                        var serviceKey = Registry.LocalMachine.OpenSubKey($"{key}\\{serviceName}");
                        if (serviceKey != null)
                        {
                            var configValue = serviceKey.GetValue("ImagePath");
                            if (configValue == null)
                            {
                                continue;
                            }
                            System.Console.WriteLine(configValue.ToString());
                            var startupExePath = new FileInfo(configValue.ToString());


                            VMConfig config = new VMConfig
                            {
                                ServiceName = serviceName,
                                Path = startupExePath.DirectoryName,
                                ISSelect = false
                            };

                            if (startupExePath.Exists)
                            {
                                try
                                {
                                    string appsettingFilePath = $"{startupExePath.DirectoryName}/appsettings.json";
                                    if (File.Exists(appsettingFilePath))
                                    {
                                        var dbConfig = new DBConfigHelper(appsettingFilePath);
                                        if (dbConfig != null)
                                        {
                                            config = MapperHelper.AutoMap<DBConfigHelper, VMConfig>(dbConfig);
                                        }
                                    }
                                }
                                catch (System.Exception e)
                                {
                                    HandyControl.Controls.Growl.Error($"无法解析配置文件,{e.Message}");
                                }
                            }

                            if (config == null)
                            {
                                config = new();
                            }

                            config.ServiceName = serviceName;
                            config.Path = startupExePath.DirectoryName;

                            ServiceList.Add(config);
                        }
                    }
                }
            });

            ExportCommand = new BaseCommand((para) =>
            {
                var saveFileDialog = new SaveFileDialog()
                {
                    Title = "保存",
                    Filter = "文件(*.txt)|*.txt",
                    RestoreDirectory = true,
                };
                if (!(saveFileDialog.ShowDialog() ?? false))
                {
                    return;
                }


                //#if DEBUG
                //                var dataTxt = File.ReadAllText(@"C:\Users\ivesBao\Desktop\1.txt");
                //                ServiceList = JsonConvert.DeserializeObject<ObservableCollection<VMConfig>>(dataTxt);
                //#endif
                //File.WriteAllText(saveFileDialog.FileName, JsonConvert.SerializeObject(ServiceList));
                //return;

                if (ServiceList == null || ServiceList.Count == 0)
                {
                    return;
                }

                FileInfo resultFile = new FileInfo(saveFileDialog.FileName);

                //根据服务分组
                Dictionary<string, List<ExportData>> dicServiceGroup = new();
                foreach (var item in ServiceList)
                {
                    if (!item.ISSelect)
                    {
                        continue;
                    }
                    var exportItem = MapperHelper.AutoMap<VMConfig, ExportData>(item);
                    if (exportItem == null)
                    {
                        item.ParseErrInfo = "无法转换数据";
                        continue;
                    }

                    //尝试获取Consul配置
                    string key = string.Empty;
                    if (!string.IsNullOrEmpty(exportItem.DBConfigKey))
                    {
                        key = item.APIPublish.ConsulIP + "-" + item.DBConfigKey;
                        exportItem.UseConsul = true;
                        exportItem.ISAPIService = true;
                    }

                    //若无consul配置则尝试获取数据库配置
                    if (string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(exportItem.ConnectionString))
                    {
                        key = exportItem.ConnectionString;
                        exportItem.ISAPIService = true;
                    }

                    //非API服务
                    if (string.IsNullOrEmpty(key))
                    {
                        key = exportItem.ServiceName;
                    }

                    if (dicServiceGroup.ContainsKey(key))
                    {
                        dicServiceGroup[key].Add(exportItem);
                    }
                    else
                    {
                        dicServiceGroup.Add(key, new List<ExportData> { exportItem });
                    }
                }

                var strBuilder = new StringBuilder();

                //根据数据库最终结果分类
                foreach (var serviceList in dicServiceGroup)
                {
                    var first = serviceList.Value.First();
                    if (first.ISAPIService)
                    {
                        if (first.APIPublish != null)
                        {
                            if (first.UseConsul)
                            {
                                strBuilder.AppendLine($"{first.APIPublish.ConsulIP}:{first.APIPublish.ConsulPort},{first.DBConfigKey}");
                            }

                            strBuilder.AppendLine($"{first.ConnectionType},{first.ConnectionString}");
                        }
                    }

                    //输出表头
                    if (serviceList.Value != null && serviceList.Value.Count > 0)
                    {
                        strBuilder.AppendLine($"| 服务 | 部署路径 | 占用ip | 占用端口 |");
                        strBuilder.AppendLine($"|  -   | -  | - | - |");
                    }
                    foreach (var service in serviceList.Value)
                    {
                        if (first.APIPublish != null)
                        {
                            strBuilder.AppendLine($"|{service.ServiceName}|{service.Path}|{service.APIPublish.ApplicationIP}|{service.APIPublish.ApplicationPort}|");
                            if (first.APIPublish.UseHttps)
                            {
                                strBuilder.AppendLine($"|{service.ServiceName}|{service.Path}|{service.APIPublish.HttpsIP}|{service.APIPublish.HttpsPort}|");
                            }
                        }
                        else
                        {
                            strBuilder.AppendLine($"|{service.ServiceName}|{service.Path}|||");
                        }
                    }
                    strBuilder.Append("\n\n");
                }

                File.WriteAllText(resultFile.FullName, strBuilder.ToString());
            });

            AutoRestartCommand = new BaseCommand((para) =>
            {
                if (ServiceList == null || ServiceList.Count == 0)
                {
                    return;
                }

                foreach (var item in ServiceList)
                {
                    if (!item.ISSelect)
                    {
                        continue;
                    }
                    ProcessCommandBase process = new ProcessCommandBase("cmd.exe");
                    process.AddParameter($" /c sc failure \"{item.ServiceName}\"  reset= \"60\" actions= \"restart/30000/restart/30000/restart/30000\" ");

                    process.Exec();

                    process.ClearParameter();
                    process.AddParameter($" /c sc config \"{item.ServiceName}\"  start=\"auto\" ");

                    process.Exec();
                }
            });

            AutoStartCommand = new BaseCommand((para) =>
            {
                if (ServiceList == null || ServiceList.Count == 0)
                {
                    return;
                }

                foreach (var item in ServiceList)
                {
                    if (!item.ISSelect)
                    {
                        continue;
                    }
                    ProcessCommandBase process = new ProcessCommandBase("cmd.exe");
                    process.AddParameter($" /c sc start \"{item.ServiceName}\"");

                    process.Exec();
                }
            });

            AutoStopCommand = new BaseCommand((para) =>
            {
                if (ServiceList == null || ServiceList.Count == 0)
                {
                    return;
                }

                foreach (var item in ServiceList)
                {
                    if (!item.ISSelect)
                    {
                        continue;
                    }
                    ProcessCommandBase process = new ProcessCommandBase("cmd.exe");
                    process.AddParameter($" /c sc stop \"{item.ServiceName}\"  ");

                    process.Exec();
                }
            });

            SelectALLCommand = new BaseCommand((para) =>
            {
                if (ServiceList == null || ServiceList.Count == 0)
                {
                    return;
                }

                foreach (var item in ServiceList)
                {
                    item.ISSelect = ISSelectAll;
                }
            });
        }
    }
}
