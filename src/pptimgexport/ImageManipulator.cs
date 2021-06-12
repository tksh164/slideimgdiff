using OpenCvSharp;

namespace libppexport
{
    public class ImageManipulator
    {
        public static void SaveDifferenceImage(string inputImageFilePath1, string outputImageFilePath1, string inputImageFilePath2, string outputImageFilePath2)
        {
            Mat inputImage1 = null, inputImage2 = null;
            Mat diffRectDrewImage1 = null, diffRectDrewImage2 = null;
            try
            {
                inputImage1 = new Mat(inputImageFilePath1, ImreadModes.Color);
                inputImage2 = new Mat(inputImageFilePath2, ImreadModes.Color);

                var diffRects = GetDiffRects(inputImage1, inputImage2);

                diffRectDrewImage1 = CreateDiffRectDrewImage(inputImage1, diffRects);
                diffRectDrewImage2 = CreateDiffRectDrewImage(inputImage2, diffRects);

                diffRectDrewImage1.SaveImage(outputImageFilePath1);
                diffRectDrewImage2.SaveImage(outputImageFilePath2);
            }
            finally
            {
                inputImage1?.Dispose();
                inputImage2?.Dispose();
                diffRectDrewImage1?.Dispose();
                diffRectDrewImage2?.Dispose();
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

        private static Mat CreateDiffRectDrewImage(Mat image, Rect[] diffRects)
        {
            var diffRectDrewImage = new Mat(image.Size(), MatType.CV_8UC3);
            image.CopyTo(diffRectDrewImage);
            for (int i = 0; i < diffRects.Length; i++)
            {
                diffRectDrewImage.Rectangle(diffRects[i], new Scalar(0, 255, 0), -1, LineTypes.AntiAlias, 0);
            }
            return diffRectDrewImage;
        }
    }
}
