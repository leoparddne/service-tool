using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace ServiceTool.Common
{
    public class AppSettingsHelper
    {
        private static readonly object _lock = new object();
        private IConfiguration configuration;
        private List<string> files { get; set; } = new List<string>();


        public void Build()
        {
            var builder = new ConfigurationBuilder();

            foreach (var item in files)
            {
                builder.AddJsonFile(item, optional: true, reloadOnChange: true);
            }

            configuration = builder.AddEnvironmentVariables().Build();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        public void AddFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("can not found " + AppContext.BaseDirectory + (filePath ?? ""));
                return;
            }

            files.Add(filePath);

            Console.WriteLine($"add file {filePath}");
        }

        public string GetSetting(params string[] sections)
        {
            if (sections != null && sections.Length > 0)
            {
                if (sections.Length == 1)
                {
                    return configuration?.GetSection(sections[0]).Value;
                }
                else
                {
                    return configuration?[string.Join(":", sections)];
                }
            }

            return "";
        }

        /// <summary>
        /// 获取指定对象字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="section"></param>
        /// <returns></returns>
        public T GetObject<T>(string section) where T : class, new()
        {
            T result = new T();
            IConfigurationSection configSection = configuration.GetSection(section);
            if (configSection == null)
            {
                return null;
            }

            configSection.Bind(result);

            return result;
        }
    }
}
