using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Drawing;

namespace OnScreenTranslator.services
{
    internal class ImagePreprocessor
    {
        // private static readonly bool HasCuda = CudaInvoke.HasCuda;

        public static Bitmap Preprocess(Bitmap input)
        {
            /*return HasCuda
                ? PreprocessCuda(input)
                : PreprocessCpu(input);*/

            return LightPreprocess(input);
            //return PreprocessCpu(input);
        }

        private static Bitmap LightPreprocess(Bitmap input)
        {
            using var src = BitmapConverter.ToMat(input);

            Mat gray = new();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            Mat thresh = new();
            Cv2.Threshold(gray, thresh, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

            Mat upscaled = new();
            Cv2.Resize(thresh, upscaled, new OpenCvSharp.Size(), 2, 2, InterpolationFlags.Cubic);

            Bitmap bmp = BitmapConverter.ToBitmap(upscaled);

            return bmp;
        }

        // =========================
        // ===== CPU PIPELINE ======
        // =========================
        private static Bitmap PreprocessCpu(Bitmap input)
        {
            using var src = BitmapConverter.ToMat(input);

            Mat gray = new();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            Mat blurred = new();
            Cv2.GaussianBlur(gray, blurred, new OpenCvSharp.Size(5, 5), 0);

            Mat contrast = new();
            blurred.ConvertTo(contrast, MatType.CV_8UC1, 1.8, 0);

            Mat thresh = new();
            Cv2.AdaptiveThreshold(
                contrast,
                thresh,
                255,
                AdaptiveThresholdTypes.MeanC,
                ThresholdTypes.Binary,
                31,
                10);

            Mat morph = new();
            using var kernel = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new OpenCvSharp.Size(2, 2));

            Cv2.MorphologyEx(thresh, morph, MorphTypes.Close, kernel);

            Mat upscaled = new();
            Cv2.Resize(morph, upscaled, new OpenCvSharp.Size(), 2, 2, InterpolationFlags.Cubic);

            Bitmap normal = BitmapConverter.ToBitmap(upscaled);
            Bitmap inverted = InvertBitmap(normal);

            return ChooseBetterForOcr(normal, inverted);
        }

        // =========================
        // ===== GPU PIPELINE ======
        // =========================
        /*private static Bitmap PreprocessCuda(Bitmap input)
        {
            using var srcCpu = BitmapConverter.ToMat(input);
            using var srcGpu = new CudaMat(srcCpu);

            using var gray = new CudaMat();
            Cuda.CvtColor(srcGpu, gray, ColorConversionCodes.BGR2GRAY);

            using var blurFilter = Cuda.CreateGaussianFilter(
                gray.Type(), gray.Type(),
                new Size(5, 5), 0);

            using var blurred = new CudaMat();
            blurFilter.Apply(gray, blurred);

            using var contrast = new CudaMat();
            blurred.ConvertTo(contrast, MatType.CV_8UC1, 1.8, 0);

            using var thresh = new CudaMat();
            Cuda.AdaptiveThreshold(
                contrast,
                thresh,
                255,
                AdaptiveThresholdTypes.MeanC,
                ThresholdTypes.Binary,
                31,
                10);

            using var kernel = Cv2.GetStructuringElement(
                MorphShapes.Rect,
                new Size(2, 2));

            using var morph = new CudaMat();
            Cuda.MorphologyEx(
                thresh,
                morph,
                MorphTypes.Close,
                kernel);

            using var resized = new CudaMat();
            Cuda.Resize(morph, resized, new Size(), 2, 2, InterpolationFlags.Cubic);

            using var resultCpu = resized.DownloadMat();
            Bitmap normal = BitmapConverter.ToBitmap(resultCpu);
            Bitmap inverted = InvertBitmap(normal);

            return ChooseBetterForOcr(normal, inverted);
        }*/

        // =========================
        // ===== UTILITIES =========
        // =========================
        private static Bitmap InvertBitmap(Bitmap bmp)
        {
            using var mat = BitmapConverter.ToMat(bmp);
            Mat inverted = new();
            Cv2.BitwiseNot(mat, inverted);
            return BitmapConverter.ToBitmap(inverted);
        }

        private static Bitmap ChooseBetterForOcr(Bitmap a, Bitmap b)
        {
            return EstimateTextClarity(a) >= EstimateTextClarity(b) ? a : b;
        }

        private static double EstimateTextClarity(Bitmap bmp)
        {
            int dark = 0;
            int total = 0;

            for (int y = 0; y < bmp.Height; y += 5)
            {
                for (int x = 0; x < bmp.Width; x += 5)
                {
                    total++;
                    var c = bmp.GetPixel(x, y);
                    if (c.R < 128) dark++;
                }
            }

            return (double)dark / total;
        }
    }
}
