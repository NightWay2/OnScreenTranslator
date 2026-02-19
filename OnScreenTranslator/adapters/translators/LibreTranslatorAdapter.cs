using Newtonsoft.Json;
using OnScreenTranslator.models;
using System.Net.Http;

namespace OnScreenTranslator.adapters.translators
{
    internal class LibreTranslatorAdapter : ITranslator
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private readonly string _baseUrl;

        public LibreTranslatorAdapter(string url = "https://libretranslate.com")
        {
            _baseUrl = url.TrimEnd('/');
        }

        public async Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey)
        {
            var formUrlEncodedContent = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "q", textToTranslate },
                { "source", source },
                { "target", target },
                { "api_key", apiKey }
            });

            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/translate")
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
    }
}
