using Newtonsoft.Json;
using OnScreenTranslator.models;
using System.Net.Http;

namespace OnScreenTranslator.adapters.translators
{
    internal class LibreTranslatorAdapter : ITranslator
    {
        private HttpClient HttpClient;

        public LibreTranslatorAdapter(string url = "https://libretranslate.com")
        {
            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(url),
                Timeout = TimeSpan.FromSeconds(5)
            };
        }

        public async Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey)
        {
            source = GetLanguageCode(source);
            target = GetLanguageCode(target);

            var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "q", textToTranslate },
                { "source", source },
                { "target", target },
                { "api_key", apiKey }
            });

            var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/translate")
            {
                Content = formUrlEncodedContent
            }).ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                var translatedText = JsonConvert.DeserializeObject<LTResponseModel>(
                    await response.Content.ReadAsStringAsync().ConfigureAwait(false));
                return translatedText.TranslatedText;
            }
            return string.Empty;
        }

        private string GetLanguageCode(string language)
        {
            return language.ToLower() switch
            {
                "arabic" => "ar",
                "chinese (simplified)" => "zh",
                "chinese (traditional)" => "zt",
                "czech" => "cs",
                "danish" => "da",
                "dutch" => "nl",
                "english" => "en",
                "finnish" => "fi",
                "french" => "fr",
                "german" => "de",
                "hindi" => "hi",
                "indonesian" => "id",
                "italian" => "it",
                "japanese" => "ja",
                "korean" => "ko",
                "persian" => "fa",
                "polish" => "pl",
                "portuguese" => "pt",
                "romanian" => "ro",
                "russian" => "ru",
                "spanish" => "es",
                "thai" => "th",
                "turkish" => "tr",
                "ukrainian" => "uk",
                "vietnamese" => "vi",
                _ => language
            };
        }
    }
}
