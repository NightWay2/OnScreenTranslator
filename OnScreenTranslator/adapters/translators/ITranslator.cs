namespace OnScreenTranslator.adapters.translators
{
    internal interface ITranslator
    {
        public Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey);
    }
}
