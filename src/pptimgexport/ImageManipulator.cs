using OpenCvSharp;

namespace libppexport
{
    public class ImageManipulator
    {
        public static void SaveDifferenceImage(string inputImageFilePath1, string inputImageFilePath2, string diffImageFilePath)
        {
            using (var srcImage1 = new Mat(inputImageFilePath1, ImreadModes.Color))
            using (var srcImage2 = new Mat(inputImageFilePath2, ImreadModes.Color))
            {
                var diffRects = GetDiffRects(srcImage1, srcImage2);
                using (var diffRectImage = CreateDiffRectImage(srcImage1.Size(), diffRects))
                {
                    diffRectImage.SaveImage(diffImageFilePath);
                }
            }
        }

        private static Rect[] GetDiffRects(Mat image1, Mat image2)
        {
            Point[][] countours;
            using (var absDiffImage = CreateAbsDiffImage(image1, image2))                                          // Create an abs diff image.
            using (var grayscaleImage = absDiffImage.CvtColor(ColorConversionCodes.RGB2GRAY))                      // Convert to grayscale.
            using (var blurImage = grayscaleImage.Blur(new Size(15, 15), new Point(-1, -1), BorderTypes.Default))  // Expand the diff areas by blur.
            using (var binImage = blurImage.Threshold(1, 255, ThresholdTypes.Binary))                              // Create a black and white image.
            {
                binImage.FindContours(out countours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            }

            var countoursPoly = new Point[countours.Length][];
            var diffRects = new Rect[countours.Length];
            for (int i = 0; i < countours.Length; i++)
            {
                countoursPoly[i] = Cv2.ApproxPolyDP(countours[i], 3, true);
                diffRects[i] = Cv2.BoundingRect(countoursPoly[i]);
            }
            return diffRects;
        }

        private static Mat CreateAbsDiffImage(Mat image1, Mat image2)
        {
            var diffImage = new Mat(image1.Size(), MatType.CV_8UC3);
            Cv2.Absdiff(image1, image2, diffImage);
            return diffImage;
        }

        private static Mat CreateDiffRectImage(Size imageSize, Rect[] diffRects)
        {
            var diffRectImage = new Mat(imageSize, MatType.CV_8UC3);
            for (int i = 0; i < diffRects.Length; i++)
            {
                diffRectImage.Rectangle(diffRects[i], new Scalar(0, 255, 0), -1, LineTypes.AntiAlias, 0);
            }
            return diffRectImage;
        }
    }
}
