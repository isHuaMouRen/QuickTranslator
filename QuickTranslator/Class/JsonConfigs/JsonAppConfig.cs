using Newtonsoft.Json;
using System.Windows.Input;

namespace QuickTranslator.Class.JsonConfigs
{
    public class JsonAppConfig
    {
        public class Index
        {
            [JsonProperty("source_language")]
            public string SourceLanguage { get; set; } = "auto";

            [JsonProperty("target_language")]
            public string TargetLanguage { get; set; } = "auto";

            [JsonProperty("key")]
            public int Key { get; set; } = KeyInterop.VirtualKeyFromKey(System.Windows.Input.Key.Space);

            [JsonProperty("press_delta")]
            public int PressDelta { get; set; } = 800;

            [JsonProperty("press_count")]
            public int PressCount { get; set; } = 3;
        }
    }
}
