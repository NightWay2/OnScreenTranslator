using System.Windows.Controls;

namespace OnScreenTranslator.settings
{
    // todo del or do smth
    internal class Logger
    {
        private Logger() { }

        public static void Log(TextBox tb, string message)
        {
            tb.Text += "\n" + message;
        }

        public static void Log(TextBlock tb, string message)
        {
            tb.Text += "\n" + message;
        }
    }
}
