using System.Drawing;
using Tesseract;

namespace OnScreenTranslator.adapters.ocrs
{
    internal class TesseractOcrAdapter : IOcr
    {
        // add dispose !!!
        // if (IOcr ocrService != null) {
        //    ocrService.dispose()
        //    create new()
        // }
        // else create new()
        private TesseractEngine engine;

        // mb params
        public TesseractOcrAdapter()
        {
            // mb different data for engine
            //engine = new TesseractEngine(@"./tessdata_fast", "eng");
            engine = new TesseractEngine(@"./tessdata_best", "eng");
        }

        public String GetTextFromImage(Bitmap bitmap)
        {
            using (var page = engine.Process(bitmap))
            {
                return page.GetText();
            }
        }

        public void DisposeEngine()
        {
            engine.Dispose();
        }
    }
}
