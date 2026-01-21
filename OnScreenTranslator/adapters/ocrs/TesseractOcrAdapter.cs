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
            //engine = new TesseractEngine(@"./tessdata_fast", "eng+ukr");
            engine = new TesseractEngine(@"./tessdata_fast", "eng", EngineMode.LstmOnly);
            engine.DefaultPageSegMode = PageSegMode.SingleBlock;
            engine.SetVariable("user_defined_dpi", "300");
        }

        public string GetTextFromImage(Bitmap bmp)
        {
            using (var page = engine.Process(bmp))
            {
                return page.GetText();
            }
        }

        public void DisposeEngine()
        {
            engine?.Dispose();
        }
    }
}
