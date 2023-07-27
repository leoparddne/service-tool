using Consul;
using System;
using System.Text;

namespace ServiceTool.Common
{
    public class ConsulBuilderExtension
    {
        private ConsulClient client = null;

        public ConsulBuilderExtension(string ip, string port)
        {

            client = new ConsulClient(c =>
                 {
                     c.Address = new Uri($"http://{ip}:{port}");
                     c.Datacenter = "dc1";
                     c.WaitTime = new TimeSpan(0, 0, 0, 30);
                 });
        }


        public string GetKV(string key)
        {
            var configValue = GetKV(client, key);
            return configValue;
        }

        public string GetKV(ConsulClient client, string key)
        {
            System.Threading.Tasks.Task<QueryResult<KVPair>> config = client.KV.Get(key);
            config.Wait();
            QueryResult<KVPair> kvPair = config.Result;
            if (kvPair.Response != null && kvPair.Response.Value != null)
            {
                return Encoding.UTF8.GetString(kvPair.Response.Value);
                //return Encoding.UTF8.GetString(kvPair.Response.Value, 0, kvPair.Response.Value.Length);
            }

            return string.Empty;
        }
    }
}
