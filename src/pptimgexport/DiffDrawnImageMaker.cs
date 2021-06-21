using OpenCvSharp;
using D = System.Drawing;

namespace libppexport
{
    public class DiffDrawnImageOptions
    {
        public D.Size BlurSize { get; set; }

        public D.Color FillColor { get; set; }

        public DiffDrawnImageOptions()
        {
            BlurSize = new D.Size(15, 15);
            FillColor = D.Color.FromArgb(0, 255, 0);
        }
    }

    public static class DiffDrawnImageMaker
    {
        public static void SaveDiffDrawnImage(string inputImageFilePath1, string outputImageFilePath1, string inputImageFilePath2, string outputImageFilePath2, DiffDrawnImageOptions options = null)
        {
            if (options == null) options = new DiffDrawnImageOptions();

            using (var inputImage1 = new Mat(inputImageFilePath1, ImreadModes.Color))
            using (var inputImage2 = new Mat(inputImageFilePath2, ImreadModes.Color))
            {
                DrawDiffArea(inputImage1, inputImage2, options);
                inputImage1.SaveImage(outputImageFilePath1);
                inputImage2.SaveImage(outputImageFilePath2);
            }
        }

        private static void DrawDiffArea(Mat image1, Mat image2, DiffDrawnImageOptions options)
        {
            Point[][] contours;

            // Create an abs diff image.
            using (var absDiffImage = CreateAbsDiffImage(image1, image2))
            // Convert to a grayscale image.
            using (var grayscaleImage = absDiffImage.CvtColor(ColorConversionCodes.RGB2GRAY))
            // Expand the diff contours by blur.
            using (var blurImage = grayscaleImage.Blur(new Size(options.BlurSize.Width, options.BlurSize.Height), new Point(-1, -1), BorderTypes.Default))
            // Convert to a black and white image.
            using (var binImage = blurImage.Threshold(1, 255, ThresholdTypes.Binary))
            {
                binImage.FindContours(out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            }

            const int DrawAllContours = -1;
            var contourFillColor = new Scalar(options.FillColor.B, options.FillColor.G, options.FillColor.R);
            Cv2.DrawContours(image1, contours, DrawAllContours, contourFillColor, Cv2.FILLED, LineTypes.AntiAlias);
            Cv2.DrawContours(image2, contours, DrawAllContours, contourFillColor, Cv2.FILLED, LineTypes.AntiAlias);
        }

        private static Mat CreateAbsDiffImage(Mat image1, Mat image2)
        {
            var diffImage = new Mat(image1.Size(), image1.Type());
            Cv2.Absdiff(image1, image2, diffImage);
            return diffImage;
        }
    }
}
