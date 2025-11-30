using HuaZi.Library.Logger;
using System.IO;
using System.Text;

namespace QuickTranslator.Class
{
    public static class AppLogger
    {
        public static Logger logger = new Logger
        {
            LogDirectory = Path.Combine(AppInfo.ExecuteDirectory, "Logs"),
            ShowCallerInfo = false,
            ShowDate = false,
            Encoding = Encoding.UTF8,
            ShowTime = true
        };
    }
}
