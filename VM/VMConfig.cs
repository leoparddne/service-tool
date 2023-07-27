using PropertyChanged;
using ServiceTool.Common;

namespace ServiceTool.VM
{
    [AddINotifyPropertyChangedInterface]
    public class VMConfig : DBConfigHelper
    {
        /// <summary>
        /// 服务名称
        /// </summary>
        public string ServiceName { get; set; } = string.Empty;

        /// <summary>
        /// 服务部署位置
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool ISSelect { get; set; }
    }
}
