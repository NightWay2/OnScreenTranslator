using OnScreenTranslator.adapters.ocrs;
using OnScreenTranslator.services;
using System.Drawing;
using System.Windows;

namespace OnScreenTranslator.ui
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlayWindow;
        private Rect? selectedScreenArea;
        private IOcr? ocrService;

        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Create overlay window if it not exist, add listener to overlay window
        /// </summary>
        private void CreateOverlayWindow(object sender, RoutedEventArgs e)
        {
            if (overlayWindow != null)
                return;

            overlayWindow = new OverlayWindow();
            overlayWindow.Closed += OverlayWindow_Closed;
            overlayWindow.Show();

            tlgBtnOverlayLock.IsEnabled = true;
        }

        /// <summary>
        /// Destroy overlay window if toggle button is disable
        /// </summary>
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

        private void LockOverlayWindow(object sender, RoutedEventArgs e)
        {
            overlayWindow?.EnableLockMode();
        }

        private void UnlockOverlayWindow(object sender, RoutedEventArgs e)
        {
            overlayWindow?.DisableLockMode();
        }

        /// <summary>
        /// Disable buttons on MainWindow if overlay window is closed
        /// </summary>
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

        private void SelectAreaOnScreen(object sender, RoutedEventArgs e)
        {
            AreaSelectionWindow selector = new AreaSelectionWindow();
            if (selector.ShowDialog() == true)
            {
                selectedScreenArea = selector.SelectedArea;

                // Debug code :: ToDo clear
                MessageBox.Show($"X={selectedScreenArea.Value.X}, " +
                    $"Y={selectedScreenArea.Value.Y}, w={selectedScreenArea.Value.Width}, " +
                    $"h={selectedScreenArea.Value.Height}");

                // todo: save to settings
            }
        }

        private void StartTranslation(object sender, RoutedEventArgs e)
        {
            // change as need
            // hide overlay while doing screenshot
            if (selectedScreenArea.HasValue)
            {
                // maybe something different, not hiding and showing
                overlayWindow?.Hide();
                Bitmap bitmap = ScreenCaptureService.GetImage(selectedScreenArea.Value);
                overlayWindow?.Show();
                
                // Create new Ocr with settings and not here
                ocrService = new TesseractOcrAdapter();
                MessageBox.Show(ocrService.GetTextFromImage(bitmap));
            }
            else
            {
                // think about something better
                MessageBox.Show("Area on screen isn`t selected.");
            }
        }
    }

    // think about multithreading

    // mb hide overlay window while doing screenshot & delete screenshot?
    // add setting of selecting monitor ?

    // Add settings logic with all necessary checks
    // Fix OverlayWindow transparency, add textblock or text field to provide text on, add clicking through window
    // Think about binding and how it can be used in this project
    // Think about different hot keys
    // mb animations
    // download tesseract
    // mb rename all methods with more appropriate
    // mb add button restore to defaults 
}