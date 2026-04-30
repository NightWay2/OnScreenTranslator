using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace OnScreenTranslator.adapters.ocrs
{
    internal sealed class OneOcrAdapter : IOcr
    {
        private long _ctx;
        private long _pipeline;
        private long _options;
        private bool _initialized;

        public OneOcrAdapter()
        {
            try
            {
                Init();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\nPlease ensure the OCR modules are present in the application folder.", 
                    "OCR initialization error.", MessageBoxButton.OK, MessageBoxImage.Error);
                throw new Exception("OCR initialization error");
            }
        }

        private void Init()
        {
            if (OneOcrNative.CreateOcrInitOptions(out _ctx) != 0)
                throw new Exception("OneOCR: ctx init failed");

            OneOcrNative.OcrInitOptionsSetUseModelDelayLoad(_ctx, 1);

            if (OneOcrNative.CreateOcrPipeline(
                "oneocr.onemodel",
                "kj)TGtrK>f]b[Piow.gU+nC@s\"\"\"\"\"\"4",
                _ctx,
                out _pipeline) != 0)
                throw new Exception("OneOCR: pipeline init failed");

            OneOcrNative.CreateOcrProcessOptions(out _options);
            OneOcrNative.OcrProcessOptionsSetMaxRecognitionLineCount(_options, 1000);

            _initialized = true;
        }

        public string GetTextFromImage(Bitmap bitmap)
        {
            if (!_initialized)
                return string.Empty;

            using var rgba = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(rgba))
                g.DrawImage(bitmap, 0, 0);

            var data = rgba.LockBits(
                new Rectangle(0, 0, rgba.Width, rgba.Height),
                ImageLockMode.ReadOnly,
                rgba.PixelFormat);

            try
            {
                Img img = new Img
                {
                    t = 3,
                    col = rgba.Width,
                    row = rgba.Height,
                    step = rgba.Width * 4,
                    data_ptr = data.Scan0
                };

                if (OneOcrNative.RunOcrPipeline(_pipeline, ref img, _options, out long instance) != 0)
                    return string.Empty;

                OneOcrNative.GetOcrLineCount(instance, out long count);

                StringBuilder sb = new();

                for (long i = 0; i < count; i++)
                {
                    OneOcrNative.GetOcrLine(instance, i, out long line);
                    OneOcrNative.GetOcrLineContent(line, out IntPtr ptr);
                    sb.AppendLine(PtrToStringUTF8(ptr));
                }

                return sb.ToString();
            }
            finally
            {
                rgba.UnlockBits(data);
            }
        }

        private static string PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return string.Empty;
            int len = 0;
            while (Marshal.ReadByte(ptr, len) != 0) len++;
            byte[] buf = new byte[len];
            Marshal.Copy(ptr, buf, 0, len);
            return Encoding.UTF8.GetString(buf);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Img
    {
        public int t;
        public int col;
        public int row;
        public int _unk;
        public long step;
        public IntPtr data_ptr;
    }

    internal static partial class OneOcrNative
    {
        [LibraryImport("oneocr.dll")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial long CreateOcrInitOptions(out long ctx);

        [LibraryImport("oneocr.dll")]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial long OcrInitOptionsSetUseModelDelayLoad(long ctx, byte flag);

        [LibraryImport("oneocr.dll", StringMarshalling = StringMarshalling.Utf8)]
        [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
        public static partial long CreateOcrPipeline(string modelPath, string key, long ctx, out long pipeline);

        [LibraryImport("oneocr.dll")]
        public static partial long CreateOcrProcessOptions(out long opt);

        [LibraryImport("oneocr.dll")]
        public static partial long OcrProcessOptionsSetMaxRecognitionLineCount(long opt, long count);

        [LibraryImport("oneocr.dll")]
        public static partial long RunOcrPipeline(long pipeline, ref Img img, long opt, out long instance);

        [LibraryImport("oneocr.dll")]
        public static partial long GetOcrLineCount(long instance, out long count);

        [LibraryImport("oneocr.dll")]
        public static partial long GetOcrLine(long instance, long index, out long line);

        [LibraryImport("oneocr.dll")]
        public static partial long GetOcrLineContent(long line, out IntPtr content);
    }
}
