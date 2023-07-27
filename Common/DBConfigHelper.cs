using Newtonsoft.Json;
using ServiceTool.Model;
using ServiceTool.Model.Constant;
using System;

namespace ServiceTool.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class DBConfigHelper
    {
        public string ConnectionString { get; set; }
        public string ConnectionType { get; set; } = "Oracle";

        public string RedisConnectionString { get; set; }
        public string GateWay { get; set; }
        public string MongoDB { get; set; }


        public APIPublish APIPublish { get; set; }
        public string DBConfigKey { get; set; }

        /// <summary>
        /// 是否需要自动转小写
        /// </summary>
        public bool SqlAutoToLower { get; set; }

        public string ParseErrInfo { get; set; }


        public DBConfigHelper()
        {

        }

        /// <summary>
        /// 根据文件解析
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Exception"></exception>
        public DBConfigHelper(string path)
        {
            AppSettingsHelper settingsHelper = new AppSettingsHelper();
            settingsHelper.AddFile(path);
            settingsHelper.Build();


            //获取服务基础信息
            APIPublish = settingsHelper.GetObject<APIPublish>("Publish");



            // 预留配置中心逻辑
            DBConfigKey = settingsHelper.GetSetting(DBConstant.OutterDBConfig);
            if (!string.IsNullOrEmpty(DBConfigKey))
            {
                try
                {
                    var consul = new ConsulBuilderExtension(APIPublish.ConsulIP, APIPublish.ConsulPort);
                    var configValue = consul.GetKV(DBConfigKey);
                    if (string.IsNullOrWhiteSpace(configValue))
                    {
                        ParseErrInfo = $"{DBConfigKey} not in consul";
                    }
                    var value = JsonConvert.DeserializeObject<DBConfigHelper>(configValue);
                    if (value != null)
                    {

                        ConnectionString = value.ConnectionString;
                        ConnectionType = value.ConnectionType;
                        RedisConnectionString = value.RedisConnectionString;
                        GateWay = value.GateWay;
                        MongoDB = value.MongoDB;
                        SqlAutoToLower = value.SqlAutoToLower;
                    }
                }
                catch (Exception e)
                {
                    ParseErrInfo = $"解析异常:{e.Message}";
                }

                return;
            }



            //读取配置文件

            ConnectionString = settingsHelper.GetSetting("ConnectionString");
            ConnectionType = settingsHelper.GetSetting("ConnectionType") ?? "Oracle";
            RedisConnectionString = settingsHelper.GetSetting("RedisConnectionString");
            GateWay = settingsHelper.GetSetting("GateWay");
            MongoDB = settingsHelper.GetSetting("MongoDB");

            string sqlAutoToLowerStr = settingsHelper.GetSetting("SqlAutoToLower");
            bool sqlAutoToLower = true;

            if (!string.IsNullOrWhiteSpace(sqlAutoToLowerStr))
            {
                if (bool.TryParse(sqlAutoToLowerStr, out sqlAutoToLower))
                {
                    SqlAutoToLower = sqlAutoToLower;
                }
            }

        }
    }
}
