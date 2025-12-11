using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OnScreenTranslator.ui
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlayWindow;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
        }

        // Create overlay window if it not exist, add listener
        private void CreateOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
                return;

            overlayWindow = new OverlayWindow();
            overlayWindow.Closed += OverlayWindow_Closed;
            overlayWindow.Show();

            tlgBtnOverlayLock.IsEnabled = true;
        }

        // Destroy overlay window
        private void DestroyOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
            {
                overlayWindow.Closed -= OverlayWindow_Closed;
                overlayWindow.Close();
                overlayWindow = null;
            }

            tlgBtnOverlayLock.IsEnabled = false;
            tlgBtnOverlayLock.IsChecked = false;
        }

        private void tlgBtnOverlayLock_Checked(object sender, RoutedEventArgs e)
        {
            overlayWindow?.EnableLockMode();
        }

        private void tlgBtnOverlayLock_Unchecked(object sender, RoutedEventArgs e)
        {
            overlayWindow?.DisableLockMode();
        }


        private void OverlayWindow_Closed(object? sender, EventArgs e)
        {
            overlayWindow = null;

            tlgBtnOverlaySwitch.IsChecked = false;

            tlgBtnOverlayLock.IsEnabled = false;
            tlgBtnOverlayLock.IsChecked = false;
        }

         /// <summary>
         /// Close overlay window if Main window is closed
         /// </summary>
        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            overlayWindow?.Close();
        }
    }

    // Add settings logic with all necessary checks
    // Fix OverlayWindow transparency, add textblock or text field to provide text on, add clicking through window
    // Think about binding and how it can be used in this project
    // Think about different hot keys
    // mb animations
    // download tesseract
    // mb rename all methods with more appropriate
    // mb add button restore to defaults 
}