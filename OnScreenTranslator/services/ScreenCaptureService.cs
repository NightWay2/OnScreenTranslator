using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OnScreenTranslator.services
{
    class ScreenCaptureService
    {
        private Rectangle _area;

        public ScreenCaptureService() { }

        public void SetArea(Rectangle area)
        {
            _area = area;
        }

        /*public Bitmap GetImage()
        {
            return new Bitmap();
        }*/
    }
}
