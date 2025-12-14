using System.Drawing;

namespace OnScreenTranslator.adapters.ocrs
{
    internal interface IOcr
    {
        String GetTextFromImage(Bitmap bitmap);
    }
}
