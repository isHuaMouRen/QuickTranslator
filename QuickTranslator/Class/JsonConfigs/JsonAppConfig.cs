using Newtonsoft.Json;

namespace QuickTranslator.Class.JsonConfigs
{
    public class JsonAppConfig
    {
        public class Index
        {
            [JsonProperty("source_language")]
            public string SourceLanguage { get; set; }

            [JsonProperty("target_language")]
            public string TargetLanguage { get; set; }
        }
    }
}
