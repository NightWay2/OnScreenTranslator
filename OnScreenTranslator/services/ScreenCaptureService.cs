using OnScreenTranslator.ui;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows;
using System.Drawing.Imaging;

namespace OnScreenTranslator.services
{
    class ScreenCaptureService
    {
        private ScreenCaptureService() { }
        public static Bitmap GetImage(Rect area)
        {
            // todo: change with right values
            int X = (int) area.X;
            int Y = (int) area.Y;
            int width = (int) area.Width;
            int height = (int) area.Height;

            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(X, Y, width, height, new System.Drawing.Size(width, height));
                bitmap.Save("test.png");
            }
            return bitmap;
        }
    }
}
