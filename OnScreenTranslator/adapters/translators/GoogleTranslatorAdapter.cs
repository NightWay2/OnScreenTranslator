using System;
using System.Collections.Generic;
using System.Text;

namespace OnScreenTranslator.adapters.translators
{
    internal class GoogleTranslatorAdapter : ITranslator
    {
        public Task<string> TranslateAsync(string textToTranslate, string source, string target, string apiKey)
        {
            throw new NotImplementedException();
        }
    }
}
