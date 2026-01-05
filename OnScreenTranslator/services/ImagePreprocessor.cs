using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text;

namespace OnScreenTranslator.services
{
    internal class ImagePreprocessor
    {
        public static Bitmap Upscale(Bitmap src, int scale = 2)
        {
            Bitmap bmp = new Bitmap(src.Width * scale, src.Height * scale);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(src, 0, 0, bmp.Width, bmp.Height);
            }
            return bmp;
        }
    }
}
