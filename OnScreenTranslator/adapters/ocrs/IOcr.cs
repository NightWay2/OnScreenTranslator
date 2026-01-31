using System.Drawing;

namespace OnScreenTranslator.adapters.ocrs
{
    internal interface IOcr : IDisposable
    {
        string GetTextFromImage(Bitmap bitmap);
    }
}
