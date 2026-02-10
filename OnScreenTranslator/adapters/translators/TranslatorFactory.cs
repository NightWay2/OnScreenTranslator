using GTranslate.Translators;

namespace OnScreenTranslator.adapters.translators
{
    internal class TranslatorFactory
    {
        private TranslatorFactory() { }

        public static ITranslator GetTranslator(Translators translator) =>
            GetTranslator(translator, "https://libretranslate.com");
        

        public static ITranslator GetTranslator(Translators translator, string url)
        {
            return translator switch
            {
                Translators.LibreTranslator => new LibreTranslatorAdapter(url),
                Translators.GoogleFreeTranslator => new GTranslatorsAdapter(new GoogleTranslator()),
                _ => throw new NotImplementedException()
            };
        }
    }
}
