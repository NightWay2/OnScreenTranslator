namespace OnScreenTranslator.adapters.translators
{
    internal class GTranslatorsAdapter : ITranslator
    {
        private GTranslate.Translators.ITranslator translator;

        public GTranslatorsAdapter(GTranslate.Translators.ITranslator translator)
        {
            this.translator = translator;
        }

        public async Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey)
        {
            source = GetLanguageCode(source);
            target = GetLanguageCode(target);

            var result = await translator.TranslateAsync(textToTranslate, target, source);
            return result.Translation;
        }

        private string GetLanguageCode(string language)
        {
            return language.ToLower() switch
            {
                "arabic" => "ar",
                "chinese (simplified)" or "zh" => "zh-CN",
                "chinese (traditional)" or "zt" => "zh-TW",
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
