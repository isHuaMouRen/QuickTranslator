using HuaZi.Library.Json;
using static QuickTranslator.Class.AppLogger;
using QuickTranslator.Class;
using QuickTranslator.Class.JsonConfigs;
using System.Net.Http;
using System.Threading.Tasks;

namespace QuickTranslator.Utils
{
    public static class TranslateManager
    {
        private static HttpClient Client = new HttpClient();

        public static async Task<JsonApi.Index> GetTranslate(string originText,string sourceLanguage="auto",string targetLanguage="auto")
        {
            string url = $"{AppInfo.ApiUrl}?text={originText}&from={sourceLanguage}&to={targetLanguage}";
            string result = await Client.GetStringAsync(url);
            logger.Info($"[TranslateManager] 获得翻译: {result}");
            return Json.ReadJson<JsonApi.Index>(result);
        }

        public static async Task<JsonLanguageList.Index> GetLanguageList()
        {
            string url = $"{AppInfo.ApiUrl}/langs";
            string result = await Client.GetStringAsync(url);
            logger.Info($"[TranslateManager] 获得翻译列表: {result}");
            return Json.ReadJson<JsonLanguageList.Index>(result);
        }
    }
}
