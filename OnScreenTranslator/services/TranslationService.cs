using OnScreenTranslator.adapters.translators;

namespace OnScreenTranslator.services
{
    internal class TranslationService
    {
        private ITranslator _translator;

        public TranslationService(ITranslator translator)
        {
            _translator = translator;
        }

        public async Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey = "")
        {
            return await _translator.TranslateAsync(textToTranslate, source, target, apiKey);
        }
    }
}
