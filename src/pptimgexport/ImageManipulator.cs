using OpenCvSharp;
using D = System.Drawing;

namespace libppexport
{
    public class ImageManipulator
    {
        public D.Size BlurSize { get; set; }

        public D.Color FillColor { get; set; }

        public ImageManipulator()
        {
            BlurSize = new D.Size(15, 15);
            FillColor = D.Color.FromArgb(0, 255, 0);
        }

        public void SaveDiffDrawnImage(string inputImageFilePath1, string outputImageFilePath1, string inputImageFilePath2, string outputImageFilePath2)
        {
            using (var inputImage1 = new Mat(inputImageFilePath1, ImreadModes.Color))
            using (var inputImage2 = new Mat(inputImageFilePath2, ImreadModes.Color))
            {
                DrawDiffArea(inputImage1, inputImage2);
                inputImage1.SaveImage(outputImageFilePath1);
                inputImage2.SaveImage(outputImageFilePath2);
            }
        }

        private void DrawDiffArea(Mat image1, Mat image2)
        {
            Point[][] contours;

            // Create an abs diff image.
            using (var absDiffImage = CreateAbsDiffImage(image1, image2))
            // Convert to grayscale image.
            using (var grayscaleImage = absDiffImage.CvtColor(ColorConversionCodes.RGB2GRAY))
            // Expand the diff contours by blur.
            using (var blurImage = grayscaleImage.Blur(new Size(BlurSize.Width, BlurSize.Height), new Point(-1, -1), BorderTypes.Default))
            // Convert to black and white image.
            using (var binImage = blurImage.Threshold(1, 255, ThresholdTypes.Binary))
            {
                binImage.FindContours(out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            }

            const int DrawAllContour = -1;
            var contourFillColor = new Scalar(FillColor.R, FillColor.G, FillColor.B);
            Cv2.DrawContours(image1, contours, DrawAllContour, contourFillColor, Cv2.FILLED, LineTypes.AntiAlias);
            Cv2.DrawContours(image2, contours, DrawAllContour, contourFillColor, Cv2.FILLED, LineTypes.AntiAlias);
        }

        private static Mat CreateAbsDiffImage(Mat image1, Mat image2)
        {
            var diffImage = new Mat(image1.Size(), MatType.CV_8UC3);
            Cv2.Absdiff(image1, image2, diffImage);
            return diffImage;
        }
    }
}
