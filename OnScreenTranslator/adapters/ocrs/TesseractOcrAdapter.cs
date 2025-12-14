using System.Drawing;
using Tesseract;

namespace OnScreenTranslator.adapters.ocrs
{
    internal class TesseractOcrAdapter : IOcr
    {
        private TesseractEngine? engine;

        public TesseractOcrAdapter()
        {
            // mb different data for engine
            engine = new TesseractEngine(@"./tessdata", "eng");
        }

        public String GetTextFromImage(Bitmap bitmap)
        {
            using (var page = engine.Process(bitmap))
            {
                return page.GetText();
            }
        }
    }
}
