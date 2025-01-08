using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Bitirme
{
    internal class preprocess
    {
        private MainWindow MainWindow;
        public BitmapImage PreprocessAndDisplayImage(string imagePath)
        {
            // Load and preprocess the image
            Mat preprocessedImage = PreprocessImage(imagePath);

            // Convert Mat to Bitmap using ToImage
            var image = preprocessedImage.ToImage<Bgr, byte>();
            Bitmap bitmap = image.ToBitmap();

            // Display the preprocessed image in SelectedImage_pre
            return ConvertBitmapToBitmapImage(bitmap);
        }





        private Mat PreprocessImage(string imagePath)
        {
            // Load the image in BGR format
            Mat image = CvInvoke.Imread(imagePath, ImreadModes.Color);

            if (image == null || image.IsEmpty)
            {
                throw new Exception("Image could not be loaded.");
            }

            // Convert BGR to RGB
            CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Rgb);

            // Resize to 256x256
            CvInvoke.Resize(image, image, new System.Drawing.Size(256, 256));

            // Do not normalize pixel values here for visualization
            // Use normalized values only when preparing the input for the model

            return image;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // Make it cross-thread accessible

                return bitmapImage;
            }
        }


        public BitmapImage VisualizeOutput(string imagePath)
        {
            // Load the image from the file path
            var image = new Bitmap(imagePath);

            // Visualize the processed grayscale output
            var visualizedImage = VisualizeGrayscaleOutput(image);

            // Convert the processed Bitmap to BitmapImage for display
            return ConvertBitmapToBitmapImage(visualizedImage);
        }

        private Bitmap VisualizeGrayscaleOutput(Bitmap image)
        {
            // Resize the image to 256x256
            var resizedImage = new Bitmap(image, new Size(256, 256));

            // Convert to grayscale and apply enhancements
            var grayscaleImage = new Bitmap(resizedImage.Width, resizedImage.Height);
            for (int i = 0; i < resizedImage.Width; i++)
            {
                for (int j = 0; j < resizedImage.Height; j++)
                {
                    Color pixelColor = resizedImage.GetPixel(i, j);

                    // Convert the pixel to grayscale
                    int grayValue = (int)(pixelColor.R * 0.299 + pixelColor.G * 0.587 + pixelColor.B * 0.114);

                    // Apply a basic contrast enhancement by stretching the intensity range
                    int enhancedGrayValue = Math.Min(255, Math.Max(0, (int)(grayValue * 1.2)));

                    // Create a grayscale color
                    Color grayColor = Color.FromArgb(enhancedGrayValue, enhancedGrayValue, enhancedGrayValue);
                    grayscaleImage.SetPixel(i, j, grayColor);
                }
            }
            
            // Optionally, apply a color map (e.g., Jet or Hot)
            return ApplyColorMap(grayscaleImage);
        }

        private Bitmap ApplyColorMap(Bitmap grayscaleImage)
        {
            var colorMappedImage = new Bitmap(grayscaleImage.Width, grayscaleImage.Height);

            for (int x = 0; x < grayscaleImage.Width; x++)
            {
                for (int y = 0; y < grayscaleImage.Height; y++)
                {
                    // Get the grayscale intensity
                    int grayValue = grayscaleImage.GetPixel(x, y).R;

                    // Map the intensity to a color (e.g., Jet colormap)
                    Color mappedColor = GetJetColor(grayValue);

                    colorMappedImage.SetPixel(x, y, mappedColor);
                }
            }

            return colorMappedImage;
        }

        private Color GetJetColor(int intensity)
        {
            // Jet colormap approximation
            double r = Math.Max(0.0, Math.Min(1.0, 1.5 - Math.Abs(intensity / 128.0 - 1.0)));
            double g = Math.Max(0.0, Math.Min(1.0, 1.5 - Math.Abs(intensity / 128.0 - 0.5)));
            double b = Math.Max(0.0, Math.Min(1.0, 1.5 - Math.Abs(intensity / 128.0)));

            return Color.FromArgb((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }

      


    }
}
