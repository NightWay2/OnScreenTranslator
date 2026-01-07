using OnScreenTranslator.adapters.ocrs;
using OnScreenTranslator.services;
using OnScreenTranslator.win32;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace OnScreenTranslator.ui
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlayWindow;
        private Rect? selectedScreenArea;
        private IOcr? ocrService; // change from adapter to service

        private const int HOTKEY_ID = 1;
        private bool isSelectingArea = false;

        public MainWindow()
        {
            InitializeComponent();

            Closed += MainWindow_Closed;
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

        // think about hotkey for area selection
        private void SelectAreaOnScreen(object sender, RoutedEventArgs e)
        {
            isSelectingArea = true;

            try
            {
                AreaSelectionWindow selector = new AreaSelectionWindow();
                if (selector.ShowDialog() == true)
                {
                    selectedScreenArea = selector.SelectedArea;

                    // Debug code :: ToDo clear
                    /*var source = PresentationSource.FromVisual(this);
                    double dpiX = source.CompositionTarget.TransformToDevice.M11;
                    double dpiY = source.CompositionTarget.TransformToDevice.M22;

                    MessageBox.Show($"X={selectedScreenArea.Value.X}, " +
                        $"Y={selectedScreenArea.Value.Y}, w={selectedScreenArea.Value.Width}, " +
                        $"h={selectedScreenArea.Value.Height}, " +
                        $"x={dpiX}, y={dpiY}");*/
                    // Debug code :: end
                }
            }
            finally
            {
                isSelectingArea = false;
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
                Bitmap bmp = ScreenCaptureService.GetImage(selectedScreenArea.Value);
                overlayWindow?.Show();
                
                // Create new Ocr with settings and not here
                ocrService = new TesseractOcrAdapter(); // use service, not adapter

                // bmp = ImagePreprocessor.Upscale(bmp); // mb useless

                overlayWindow?.TxtOverlay.Text = ocrService.GetTextFromImage(bmp);
                //MessageBox.Show(ocrService.GetTextFromImage(bmp));
            }
            else
            {
                // think about something better
                MessageBox.Show("Area on screen isn`t selected.");
            }
        }

        //
        // methods that provide non-main logic, but are necessary
        //
        private void OverlayWindow_Closed(object? sender, EventArgs e)
        {
            overlayWindow = null;

            tlgBtnOverlaySwitch.IsChecked = false;

            tlgBtnOverlayLock.IsEnabled = false;
            tlgBtnOverlayLock.IsChecked = false;
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            overlayWindow?.Close();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var hwnd = new WindowInteropHelper(this).Handle;

            NativeMethods.RegisterHotKey(hwnd, HOTKEY_ID, NativeMethods.MOD_CONTROL,
                (uint)KeyInterop.VirtualKeyFromKey(Key.Oem5));

            HwndSource.FromHwnd(hwnd).AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                if (!isSelectingArea)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        SelectAreaOnScreen(this, null);
                    }));
                }
                
                handled = true;
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.UnregisterHotKey(hwnd, HOTKEY_ID);
            base.OnClosed(e);
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
    // download tesseract +
    // mb rename all methods with more appropriate 
    // mb add button restore to defaults 
    // mb tray icon

    // add user entered seconds for delay before each translation
    // add custom size of text in overlay, custom color of text in overlay, custom alpha of overlay window

    // docker compose: add only supported languages
}