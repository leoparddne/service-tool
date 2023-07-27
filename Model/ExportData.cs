using ServiceTool.VM;

namespace ServiceTool.Model
{
    public class ExportData : VMConfig
    {
        public bool ISAPIService { get; set; } = false;
        public bool UseConsul { get; set; } = false;

    }
}
