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
            var result = await translator.TranslateAsync(textToTranslate, target, source);
            return result.Translation;
        }
    }
}
