using OpenCvSharp;
using System.Threading.Tasks;

namespace libppexport
{
    public static class DiffDrawnAddedImageMaker
    {
        public static void SaveDiffDrawnAddedImage(string inputImageFilePath1, string outputImageFilePath1, string inputImageFilePath2, string outputImageFilePath2, double inputImageWeight, DiffDrawnImageOptions options = null)
        {
            if (options == null) options = new DiffDrawnImageOptions();

            using (var inputImage1 = new Mat(inputImageFilePath1, ImreadModes.Color))
            using (var inputImage2 = new Mat(inputImageFilePath2, ImreadModes.Color))
            using (var diffDrawnImage1 = new Mat(inputImage1.Size(), inputImage1.Type()))
            using (var diffDrawnImage2 = new Mat(inputImage2.Size(), inputImage2.Type()))
            {
                Task.WaitAll(new Task[] {
                    Task.Run(() => { inputImage1.CopyTo(diffDrawnImage1); }),
                    Task.Run(() => { inputImage2.CopyTo(diffDrawnImage2); })
                });

                DrawDiffArea(diffDrawnImage1, diffDrawnImage2, options);

                Task.WaitAll(new Task[] {
                    Task.Run(() => {
                        AddImageWithWeight(inputImage1, inputImageWeight, diffDrawnImage1);
                        inputImage1.SaveImage(outputImageFilePath1);
                    }),
                    Task.Run(() => {
                        AddImageWithWeight(inputImage2, inputImageWeight, diffDrawnImage2);
                        inputImage2.SaveImage(outputImageFilePath2);
                    })
                });
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
            Task.WaitAll(new Task[] {
                Task.Run(() => { Cv2.DrawContours(image1, contours, DrawAllContours, contourFillColor, Cv2.FILLED, LineTypes.AntiAlias); }),
                Task.Run(() => { Cv2.DrawContours(image2, contours, DrawAllContours, contourFillColor, Cv2.FILLED, LineTypes.AntiAlias); })
            });
        }

        private static Mat CreateAbsDiffImage(Mat image1, Mat image2)
        {
            var diffImage = new Mat(image1.Size(), image1.Type());
            Cv2.Absdiff(image1, image2, diffImage);
            return diffImage;
        }

        private static void AddImageWithWeight(Mat image, double imageWeight, Mat addingImage)
        {
            Cv2.AddWeighted(image, imageWeight, addingImage, 1.0 - imageWeight, 0, image);
        }
    }
}
