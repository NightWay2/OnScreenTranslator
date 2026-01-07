using Newtonsoft.Json;

namespace OnScreenTranslator.models
{
    internal class LTRequestModel // mb del
    {
        [JsonProperty("q")]
        public string TextToTranslate { get; set; }

        [JsonProperty("source")]
        public  string Source { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        //[JsonProperty("alternatives")]
        //public string Alternatives { get; set; }

        [JsonProperty("api_key")]
        public string ApiKey { get; set; }
    }
}
