using System.Drawing;
using System.Drawing.Drawing2D;
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
            //engine = new TesseractEngine(@"./tessdata_fast", "eng+uk");
            engine = new TesseractEngine(@"./tessdata_fast", "eng", EngineMode.Default);

            // engine.SetVariable("tessedit_char_whitelist", "1234"); // unique for each lang
        }

        public String GetTextFromImage(Bitmap bmp)
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
