namespace OnScreenTranslator.adapters.translators
{
    internal class TranslatorFactory
    {
        private TranslatorFactory() { }

        public static ITranslator GetTranslator(Translators translator)
        {
            return GetTranslator(translator, "https://libretranslate.com");
        }

        public static ITranslator GetTranslator(Translators translator, string url)
        {
            return translator switch
            {
                Translators.GoogleTranslator => new GoogleTranslatorAdapter(),
                Translators.LibreTranslator => new LibreTranslatorAdapter(url),
                _ => throw new NotImplementedException()
            };
        }
    }
}
