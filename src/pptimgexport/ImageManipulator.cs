using OpenCvSharp;

namespace libppexport
{
    public class ImageManipulator
    {
        public static void SaveDifferenceImage(string inputImageFilePath1, string outputImageFilePath1, string inputImageFilePath2, string outputImageFilePath2)
        {
            using (var inputImage1 = new Mat(inputImageFilePath1, ImreadModes.Color))
            using (var inputImage2 = new Mat(inputImageFilePath2, ImreadModes.Color))
            {
                DrawDiffContours(inputImage1, inputImage2);
                inputImage1.SaveImage(outputImageFilePath1);
                inputImage2.SaveImage(outputImageFilePath2);
            }
        }

        private static void DrawDiffContours(Mat image1, Mat image2)
        {
            Point[][] contours;
            using (var absDiffImage = CreateAbsDiffImage(image1, image2))                                          // Create an abs diff image.
            using (var grayscaleImage = absDiffImage.CvtColor(ColorConversionCodes.RGB2GRAY))                      // Convert to grayscale.
            using (var blurImage = grayscaleImage.Blur(new Size(15, 15), new Point(-1, -1), BorderTypes.Default))  // Expand the diff areas by blur.
            using (var binImage = blurImage.Threshold(1, 255, ThresholdTypes.Binary))                              // Create a black and white image.
            {
                binImage.FindContours(out contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            }

            const int DrawAllContour = -1;
            var contourColor = new Scalar(0, 255, 0);
            Cv2.DrawContours(image1, contours, DrawAllContour, contourColor, Cv2.FILLED, LineTypes.AntiAlias);
            Cv2.DrawContours(image2, contours, DrawAllContour, contourColor, Cv2.FILLED, LineTypes.AntiAlias);
        }

        private static Mat CreateAbsDiffImage(Mat image1, Mat image2)
        {
            var diffImage = new Mat(image1.Size(), MatType.CV_8UC3);
            Cv2.Absdiff(image1, image2, diffImage);
            return diffImage;
        }
    }
}
