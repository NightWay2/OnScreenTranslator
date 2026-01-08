using System.Drawing;

namespace OnScreenTranslator.adapters.ocrs
{
    internal interface IOcr
    {
        string GetTextFromImage(Bitmap bitmap);
        void DisposeEngine();
    }
}
