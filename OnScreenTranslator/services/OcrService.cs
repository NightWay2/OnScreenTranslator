using OnScreenTranslator.adapters.ocrs;
using System.Drawing;

namespace OnScreenTranslator.services
{
    internal class OcrService
    {
        private IOcr _ocr;

        public OcrService(IOcr ocr) 
        {
            _ocr = ocr;
        }

        public string GetTextFromImage(Bitmap bitmap)
        {
            return _ocr.GetTextFromImage(bitmap);
        }
    }
}
