using System.Windows;

namespace OnScreenTranslator.ui
{
    public partial class ConfirmWindow : Window
    {
        public ConfirmWindow(string message)
        {
            InitializeComponent();
            
            MessageTextBlock.Text = message;

            YesButton.Content = resources.Strings.Yes;
            CancelButton.Content = resources.Strings.Cancel;
            Title = resources.Strings.ConfirmationWindow;
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
