using OnScreenTranslator.adapters.translators;

namespace OnScreenTranslator.services
{
    internal class TranslationService
    {
        private ITranslator translator;

        public TranslationService(ITranslator translator)
        {
            this.translator = translator;
        }

        public async Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey = "")
        {
            if (string.IsNullOrWhiteSpace(textToTranslate))
                return string.Empty;

            return await translator.TranslateAsync(textToTranslate, source, target, apiKey);
        }
    }
}
