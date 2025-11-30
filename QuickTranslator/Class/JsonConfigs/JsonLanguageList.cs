using Newtonsoft.Json;

namespace QuickTranslator.Class.JsonConfigs
{
    public class JsonLanguageList
    {
        public class Index
        {
            [JsonProperty("code")]
            public int Code { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("data")]
            public LanguageInfo[] Data {  get; set; }
        }

        public class LanguageInfo
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("label")]
            public string Label { get; set; }

            [JsonProperty("alphabet")]
            public string Alphabet { get; set; }
        }
    }
}
