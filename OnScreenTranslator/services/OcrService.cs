using OnScreenTranslator.adapters.ocrs;
using System.Drawing;

namespace OnScreenTranslator.services
{
    internal class OcrService
    {
        private IOcr ocr;

        public OcrService(IOcr ocr) 
        {
            this.ocr = ocr;
        }

        public string GetTextFromImage(Bitmap bitmap)
        {
            return ocr.GetTextFromImage(bitmap);
        }

        public void DisposeEngine()
        {
            ocr.DisposeEngine();
        }
    }
}
