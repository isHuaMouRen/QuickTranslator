using HuaZi.Library.Json;
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
            return Json.ReadJson<JsonApi.Index>(await Client.GetStringAsync(url));
        }

        public static async Task<JsonLanguageList.Index> GetLanguageList()
        {
            string url = $"{AppInfo.ApiUrl}/langs";
            return Json.ReadJson<JsonLanguageList.Index>(await Client.GetStringAsync(url));
        }
    }
}
