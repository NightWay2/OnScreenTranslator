using System.Windows;

namespace OnScreenTranslator.ui
{
    public partial class TranslatorSettingsWindow : Window
    {
        public TranslatorSettingsWindow()
        {
            InitializeComponent();
            // todo add settings, change title
            ComBoxTranslator.SelectedIndex = 0;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string confirmMsg = resources.Strings.ApplyConfirmation;

            var confirmDlg = new ConfirmWindow(confirmMsg);
            confirmDlg.Owner = this;

            if (confirmDlg.ShowDialog() == true)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
