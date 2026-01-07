using Newtonsoft.Json;

namespace OnScreenTranslator.models
{
    internal class LTResponseModel
    {
        [JsonProperty("translatedText")]
        public string TranslatedText { get; set; }
    }
}
