using OnScreenTranslator.adapters.translators;
using OnScreenTranslator.settings;
using System.Windows;

namespace OnScreenTranslator.ui
{
    public partial class TranslatorSettingsWindow : Window
    {
        public TranslatorSettingsWindow()
        {
            InitializeComponent();

            ComBoxTranslator.SelectedValue = SettingsManager.GetInstance().GetTranslator();
            TranslatorChanged(this, null);
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

        private void TranslatorChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Enum.TryParse(ComBoxTranslator.SelectedValue.ToString(), out Translators tag);

            if (tag == null)
                tag = Translators.GoogleFreeTranslator;

            switch (tag)
            {
                case Translators.GoogleFreeTranslator:
                    TxtEndpoint.IsEnabled = false;
                    TxtEndpoint.Text = string.Empty;
                    TxtEndpoint.Opacity = 0.7;
                    TxtApiKey.IsEnabled = false;
                    TxtApiKey.Text = string.Empty;
                    TxtApiKey.Opacity = 0.7;
                    break;

                case Translators.LibreTranslator:
                    SettingsManager sm = SettingsManager.GetInstance();
                    TxtEndpoint.IsEnabled = true;
                    TxtEndpoint.Opacity = 1;
                    TxtEndpoint.Text = sm.GetLibreTranslatorEndpoint();
                    TxtApiKey.IsEnabled = true;
                    TxtApiKey.Opacity = 1;
                    TxtApiKey.Text = sm.GetLibreTranslatorApikey();
                    break;

                default:
                    ComBoxTranslator.SelectedValue = Translators.GoogleFreeTranslator;
                    TranslatorChanged(sender, e);
                    break;
            }
        }
    }
}
