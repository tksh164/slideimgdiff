using OpenCvSharp;

namespace libppexport
{
    public static class AddedImageMaker
    {
        public static void SaveAddedImage(string inputImageFilePath1, double weight1, string inputImageFilePath2, double weight2, string outputImageFilePath)
        {
            using (var inputImage1 = new Mat(inputImageFilePath1, ImreadModes.Color))
            using (var inputImage2 = new Mat(inputImageFilePath2, ImreadModes.Color))
            using (var addedImage = CreateAddedImage(inputImage1, weight1, inputImage2, weight2))
            {
                addedImage.SaveImage(outputImageFilePath);
            }
        }

        private static Mat CreateAddedImage(Mat image1, double weight1, Mat image2, double weight2)
        {
            var addedImage = new Mat(image1.Size(), MatType.CV_8UC3);
            Cv2.AddWeighted(image1, weight1, image2, weight2, 0, addedImage);
            return addedImage;
        }
    }
}
