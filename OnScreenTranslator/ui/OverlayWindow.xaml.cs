using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace OnScreenTranslator.ui
{
    public partial class OverlayWindow : Window
    {
        // WinAPI constants
        private const int GWL_EXSTYLE = -20;
        private const long WS_EX_TRANSPARENT = 0x00000020L;
        private const long WS_EX_LAYERED = 0x00080000L;
        private const long WS_EX_TOOLWINDOW = 0x00000080L;

        private IntPtr hwnd;
        private bool isLocked = false;

        public OverlayWindow()
        {
            InitializeComponent();

            Loaded += OverlayWindow_Loaded;
            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
        {
            hwnd = new WindowInteropHelper(this).Handle;

            SetUnlockedVisuals();
        }

        public void EnableLockMode()
        {
            if (isLocked) return;

            long ex = GetWindowLongPtr(hwnd).ToInt64();
            ex |= WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
            SetWindowLongPtr(hwnd, new IntPtr(ex));

            RootGrid.IsHitTestVisible = false;

            MainBorder.Background = new SolidColorBrush(Color.FromArgb(50, 0, 0, 0));
            ResizeMode = ResizeMode.NoResize;

            isLocked = true;
        }

        public void DisableLockMode()
        {
            if (!isLocked) return;

            long ex = GetWindowLongPtr(hwnd).ToInt64();
            ex &= ~WS_EX_TRANSPARENT;
            SetWindowLongPtr(hwnd, new IntPtr(ex));

            RootGrid.IsHitTestVisible = true;

            MainBorder.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));
            ResizeMode = ResizeMode.CanResizeWithGrip;

            isLocked = false;
        }

        private void SetUnlockedVisuals()
        {
            RootGrid.IsHitTestVisible = true;
            MainBorder.Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0));

            long ex = GetWindowLongPtr(hwnd).ToInt64();
            ex &= ~WS_EX_TRANSPARENT;
            SetWindowLongPtr(hwnd, new IntPtr(ex));
            isLocked = false;
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongW")]
        private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

        private static IntPtr GetWindowLongPtr(IntPtr hWnd)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, GWL_EXSTYLE);
            else
                return new IntPtr(GetWindowLong32(hWnd, GWL_EXSTYLE));
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
        private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongW")]
        private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        private static IntPtr SetWindowLongPtr(IntPtr hWnd, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, GWL_EXSTYLE, dwNewLong);
            else
                return new IntPtr(SetWindowLong32(hWnd, GWL_EXSTYLE, dwNewLong.ToInt32()));
        }
    }
}
