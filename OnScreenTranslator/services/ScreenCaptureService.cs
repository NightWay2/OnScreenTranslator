using System.Windows;
using System.Drawing.Imaging;
using System.Drawing;

namespace OnScreenTranslator.services
{
    class ScreenCaptureService
    {
        private ScreenCaptureService() { }

        public static Bitmap GetImage(Rect area)
        {
            Bitmap bitmap = new Bitmap(
                (int) area.Width, 
                (int) area.Height,
                PixelFormat.Format32bppArgb
            );

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(
                    (int) area.X, 
                    (int) area.Y, 
                    0, 0,
                    new System.Drawing.Size((int) area.Width, (int) area.Height),
                    CopyPixelOperation.SourceCopy
                );
            }
            //bitmap.Save("screen_test_file.jpg"); // todo del
            return bitmap;
        }
    }
}
