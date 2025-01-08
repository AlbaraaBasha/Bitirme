using Microsoft.ML;

using Microsoft.ML.Transforms.Image;
using System.Drawing;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.IO;

namespace Bitirme
{
    public class PredictionEngine
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;

        public PredictionEngine(string modelPath)
        {
            _mlContext = new MLContext();
            _model = LoadModel(modelPath);
        }

        private ITransformer LoadModel(string modelPath)
        {
            var pipeline = _mlContext.Transforms.LoadImages(
                    outputColumnName: "input",
                    imageFolder: "",
                    inputColumnName: nameof(RetinopathyInput.ImagePath))
                .Append(_mlContext.Transforms.ResizeImages(
                    outputColumnName: "input",
                    imageWidth: 256,
                    imageHeight: 256))
                .Append(_mlContext.Transforms.ExtractPixels(
                    outputColumnName: "input",
                    scaleImage: 1f / 255f))
                .Append(_mlContext.Transforms.ApplyOnnxModel(
                    modelFile: modelPath,
                    outputColumnNames: new[] { "dense_11" },
                    inputColumnNames: new[] { "input" }));

            var emptyData = _mlContext.Data.LoadFromEnumerable<RetinopathyInput>(new List<RetinopathyInput>());
            return pipeline.Fit(emptyData);
        }





        public int Predict(string imagePath)
        {
            var input = new RetinopathyInput { ImagePath = imagePath };

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<RetinopathyInput, RetinopathyOutput>(_model);
            var prediction = predictionEngine.Predict(input);

            
            return Array.IndexOf(prediction.Predictions, prediction.Predictions.Max());
        }

        public BitmapImage PredictAndVisualize(string imagePath)
        {
            // Predict the class
            var input = new RetinopathyInput { ImagePath = imagePath };
            var predictionEngine = _mlContext.Model.CreatePredictionEngine<RetinopathyInput, RetinopathyOutput>(_model);
            var prediction = predictionEngine.Predict(input);
            int predictedClass = Array.IndexOf(prediction.Predictions, prediction.Predictions.Max());

            // Load original image
            Bitmap originalImage = new Bitmap(imagePath);

            // Overlay prediction text
            using (Graphics graphics = Graphics.FromImage(originalImage))
            {
                string[] labels = { "Healthy Retina", "Mild NPDR", "Moderate NPDR", "Severe NPDR", "PDR" };
                Brush textBrush = Brushes.Green;
                if (predictedClass != 0)
                    textBrush = Brushes.Red;

                graphics.DrawString(
                    $"Prediction: {labels[predictedClass]}",
                    new Font("Arial", 20),
                    textBrush,
                    new PointF(10, 10)
                );
            }

            // Convert Bitmap to BitmapImage for WPF display
            return ConvertBitmapToBitmapImage(originalImage);
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
    }
}
