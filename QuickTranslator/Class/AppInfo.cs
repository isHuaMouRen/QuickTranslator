using System.IO;
using System.Reflection;

namespace QuickTranslator.Class
{
    public static class AppInfo
    {
        //字符串
        public static readonly string Version = "1.0.0-alpha.1";

        //执行目录
        public static readonly string ExecuteDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)!;

        //URL
        public static readonly string ApiUrl = "https://60api.09cdn.xyz/v2/fanyi";
    }
}
