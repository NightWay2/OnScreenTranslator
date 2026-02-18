namespace OnScreenTranslator.adapters.translators
{
    internal interface ITranslator : IDisposable
    {
        public Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey);
    }
}
