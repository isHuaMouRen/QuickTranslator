using Newtonsoft.Json;

namespace QuickTranslator.Class.JsonConfigs
{
    public class JsonApi
    {
        public class Index
        {
            [JsonProperty("code")]
            public int Code { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("data")]
            public JsonApi.Data Data { get; set; }
        }

        public class Data
        {
            [JsonProperty("source")]
            public JsonApi.LanguageInfo Source { get; set; }

            [JsonProperty("target")]
            public JsonApi.LanguageInfo Target { get; set; }
        }

        public class LanguageInfo
        {
            [JsonProperty("text")]
            public string Text { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("type_desc")]
            public string TypeDescription { get; set; }

            [JsonProperty("pronounce")]
            public string Pronounce { get; set; }
        }
    }
}
