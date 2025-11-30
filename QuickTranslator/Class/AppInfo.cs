using QuickTranslator.Class.JsonConfigs;
using System.IO;
using System.Reflection;

namespace QuickTranslator.Class
{
    public static class AppInfo
    {
        //版本
        public static readonly string Version = "1.0.0-alpha.1";
        //执行目录
        public static readonly string ExecuteDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)!;
        //API URL
        public static readonly string ApiUrl = "https://60api.09cdn.xyz/v2/fanyi";
        //配置
        public static JsonAppConfig.Index Config;
        //配置文件路径
        public static readonly string ConfigPath = Path.Combine(ExecuteDirectory, "config.json");
    }
}
