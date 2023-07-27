namespace ServiceTool.Model
{
    public class APIPublish
    {
        public string ApplicationIP { get; set; }
        public string ApplicationPort { get; set; }
        public bool UseHttps { get; set; }
        public string HttpsIP { get; set; }
        public string HttpsPort { get; set; }
        public string ConsulIP { get; set; }
        public string ConsulPort { get; set; }
        public string ConsulName { get; set; }
    }
}
