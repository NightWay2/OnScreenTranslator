using GTranslate.Translators;

namespace OnScreenTranslator.adapters.translators
{
    internal class GoogleFreeTranslatorAdapter : ITranslator
    {
        private GoogleTranslator googleTranslator;

        public GoogleFreeTranslatorAdapter()
        {
            googleTranslator = new GoogleTranslator();
        }

        public async Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey)
        {
            var result = await googleTranslator.TranslateAsync(textToTranslate, target, source);
            return result.Translation;
        }
    }
}
