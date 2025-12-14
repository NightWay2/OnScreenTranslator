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
            int X = (int) area.X;
            int Y = (int) area.Y;
            int width = (int) area.Width;
            int height = (int) area.Height;

            Bitmap bitmap = new Bitmap(
                width, 
                height,
                PixelFormat.Format32bppArgb
            );

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(
                    X, Y, 
                    0, 0,
                    new System.Drawing.Size(width, height),
                    CopyPixelOperation.SourceCopy
                );
            }
            return bitmap;
        }
    }
}
