using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using static Bitirme.MainWindow;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using static Bitirme.SignInSignUpWindow;

namespace Bitirme
{
    public partial class ProfilePage : Window
    {
        private readonly string _profileImageDirectory = "PatientPhotos"; // Directory to store photos
        private readonly string _currentHastaTC;
        
        public ProfilePage(string hastaTC, string image, string zaman, string sonuc, string eye)
        {
            InitializeComponent();
           
            // Fetch patient data based on HastaTC
            Directory.CreateDirectory(_profileImageDirectory);

            _currentHastaTC = hastaTC;

            var patient = HistoryData.Records.FirstOrDefault(r => r.HastaTC == hastaTC);

            if (patient != null)
            {
                // Display patient details in the UI
                PatientNameTextBlock.Text = patient.HastaName;
                PatientTCTextBlock.Text = patient.HastaTC;
                EyeTextBlock.Text = eye;
                SelectedImage.Source = new BitmapImage(new Uri(image));
                
                PredictionTextBlock.Text = sonuc;
                TimestampTextBlock.Text = zaman;
                LoadPatientPhoto();
            }

            if (pp.Source == null)
            {
                pp.Source = new BitmapImage(new Uri("images/profile.png", UriKind.RelativeOrAbsolute));
            }
        }

        private void HastaHistory_Click(object sender, RoutedEventArgs e)
        {
            // Assuming you have a method to get the current patient's HastaTC
            string currentHastaTC = PatientTCTextBlock.Text;

            // Open the HistoryPage with the current HastaTC
            HistoryPage historyPage = new HistoryPage(currentHastaTC);
            historyPage.Show();
            this.Close();
        }

        private void LoadPatientPhoto()
        {
            string photoPath = Path.Combine(_profileImageDirectory, $"{_currentHastaTC}.png");

            if (File.Exists(photoPath))
            {
                // Load the photo with caching disabled
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(photoPath, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();

                pp.Source = bitmap; // Set the loaded image as the source
            }
            else
            {
                // Load default profile photo
                pp.Source = new BitmapImage(new Uri("images/profile.png", UriKind.RelativeOrAbsolute));
            }
        }
        private void fotoyukle_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.png;*.bmp",
                Title = "Fotoğraf Seç"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedPhotoPath = openFileDialog.FileName;

                // Save the uploaded photo to the designated directory
                string savedPhotoPath = Path.Combine(_profileImageDirectory, $"{_currentHastaTC}.png");

                // Copy the selected photo to the desired location
                File.Copy(selectedPhotoPath, savedPhotoPath, overwrite: true);

                // Load the new photo and refresh the UI
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Disable caching
                bitmap.UriSource = new Uri(savedPhotoPath, UriKind.RelativeOrAbsolute); // Ensure absolute path
                bitmap.EndInit();

                pp.Source = bitmap; // Update the UI with the new image
            }
        }



        private void printRapor_Click(object sender, RoutedEventArgs e)
        {
            // Open a SaveFileDialog to let the user choose the save location
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF Files|*.pdf",
                Title = "Raporu Kaydet",
                FileName = $"{_currentHastaTC}_Rapor.pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string pdfPath = saveFileDialog.FileName;

                // Fetch patient details
                string patientName = PatientNameTextBlock.Text;
                patientName = patientName.Replace('ğ', 'g').Replace('Ğ', 'G')
                    .Replace('ş','s').Replace('Ş', 'S');
                string patientTC = PatientTCTextBlock.Text;
                string eye = EyeTextBlock.Text;
                string prediction = PredictionTextBlock.Text;
                string timestamp = TimestampTextBlock.Text;
                
                

                // Fetch patient history
                var patientHistory = HistoryData.Records
                    .Where(r => r.HastaTC == _currentHastaTC)
                    .Select(r => new { r.Timestamp, r.Prediction, r.HastaName, r.ImageName, r.eye})
                    .ToList();

                // Define the photo file path
                string photoPath = Path.Combine(_profileImageDirectory, $"{_currentHastaTC}.png");
                

                // Create a new PDF document
                using (var writer = new PdfWriter(pdfPath))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf);
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                        
                        document.Add(new Paragraph("Hasta Raporu")
                                .SetFontSize(22).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        document.Add(new Paragraph("\n"));
                        document.Add(new Paragraph("\n"));

                        // Get current date and time
                        string currentTime = DateTime.Now.ToString("HH:mm:ss");
                        string currentDate = DateTime.Now.ToString("dd MMM yyyy");

                        // Get the user name
                        string name = UserData.Name;

                        document.Add(new Paragraph($"Dr. {name}                         " +
                            $"                            Tarih: {currentDate} - {currentTime}")
                            .SetFontSize(14)
                            .SetMargin(0)
                            .SetTextAlignment(iText.Layout.Properties.TextAlignment.LEFT));

                        

                        // Create a div container with a border
                        Div borderDiv = new Div()
                            .SetBorder(new SolidBorder(ColorConstants.BLACK, 2))
                            .SetPadding(10); // Add padding inside the border


                        borderDiv.Add(new Paragraph($"Hasta Bilgileri")
                                .SetFontSize(18).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));
                        if (File.Exists(photoPath))
                        {
                            var imageData = ImageDataFactory.Create(photoPath);
                            var image = new Image(imageData);
                            image.SetWidth(100);  
                            image.SetHeight(100); 
                            image.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                            borderDiv.Add(image);
                        }
                        
                       borderDiv.Add(new Paragraph($"Hasta TC: {patientTC}"));
                        borderDiv.Add(new Paragraph($"Hasta Adı: {patientName}"));
                        document.Add(borderDiv);
                        document.Add(new LineSeparator(new SolidLine()));

                        // Add history section
                        document.Add(new Paragraph("Hasta Geçmisi")
                            .SetFontSize(22).SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                        if (patientHistory.Count > 0)
                        {
                            foreach (var history in patientHistory)
                            {
                                var imageData = ImageDataFactory.Create(history.ImageName);
                                var image = new Image(imageData);

                                image.SetWidth(100);  
                                image.SetHeight(100); 
                                document.Add(image);

                                document.Add(new Paragraph($"Tarih: {history.Timestamp}"));
                                document.Add(new Paragraph($"Göz: {history.eye.Replace('ğ', 'g')}"));
                                document.Add(new Paragraph($"Sonuç: {history.Prediction}"));
                               
                                document.Add(new LineSeparator(new SolidLine()));
                                document.Add(new Paragraph("\n"));
                            }
                        }
                        else
                        {
                            document.Add(new Paragraph("Geçmiş verisi bulunamadı."));
                        }

                        

                        // Close the document
                        document.Close();
                        }
                    }

                   
                
            }
        }



    }
}