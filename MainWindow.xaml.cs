using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Windows.Controls;
using System.Windows.Resources;
using System.Reflection;
using Emgu.CV.CvEnum;
using Emgu.CV;

using Emgu.CV.Structure;
using Microsoft.ML;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using static Bitirme.SignInSignUpWindow;


namespace Bitirme
{
    public partial class MainWindow : Window
    {
        private readonly FireBaseServices firebaseService;
        private readonly PredictionEngine _predictionEngine;
        private readonly preprocess _preprocess;
        private string _uploadedImagePath;
        private string text;
       
        
        private static string? encryptionKey;
        public MainWindow()
        {
            string name = UserData.UserName;
            HistoryData.getName(name);
            InitializeComponent();
            string modelPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Models", "retinopathy_model.onnx");

            _predictionEngine = new PredictionEngine(modelPath);
            _preprocess = new preprocess();
            
        }

        private void UploadImage_Click(object sender, RoutedEventArgs e)
        {
            
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image Files|*.jpg;*.png;*.bmp",
                Title = "Görüntü Seç"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _uploadedImagePath = openFileDialog.FileName; 
                SelectedImage.Source = new BitmapImage(new Uri(_uploadedImagePath));
                SelectedImage_pre.Source = null;
                SelectedImage_out.Source = null;
                ResultText.Text = "Bir görüntü yüklendi. Tahmin yapmak için 'Tahmin' butonuna tıklayın.";
                ResultText.Foreground = new SolidColorBrush(Colors.Black);
            }
            else
            {
                ResultText.Text = "Görüntü seçimi iptal edildi.";
            }
            

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HakkindaWindow hakkindaWindow = new HakkindaWindow();

            // Show the new window
            hakkindaWindow.Show();
        }
        
        

        public static class HistoryData
        {

            private static string fileName;
            private static string FilePath;
            static string nname;
            public static void getName(string userName)
            {
                fileName = $"{userName}_history.json";
                FilePath = fileName;
                nname = userName;
                Console.WriteLine($"History file initialized: {FilePath}");
            }
            //  private static readonly string EncryptionKey = "3DFD3C431ACCE7F9";
            // Firebase service instance (passed from outside)
            private static FireBaseServices firebaseService;

            // Method to initialize the HistoryData class
            public static async Task Initialize(FireBaseServices service)
            {
                Console.WriteLine("Starting HistoryData initialization...");
                firebaseService = service;

                encryptionKey = await firebaseService.GetKey(nname);

                if (string.IsNullOrEmpty(encryptionKey))
                {
                    Console.WriteLine($"No encryption key found for user");
                    throw new Exception($"No encryption key found for user {nname}");
                }

                Console.WriteLine($"Encryption Key for: {encryptionKey}");
            }
           

            // Retrieve the encryption key
            public static string GetEncryptionKey()
            {
                if (string.IsNullOrEmpty(encryptionKey))
                {
                    return "3DFD3C431ACCE7F9";
                }

                return encryptionKey;
            }
            private static readonly string EncryptionKey = GetEncryptionKey();
            // Structure to hold history records
            public class Record
            {
                public string ImageName { get; set; }
                public string Prediction { get; set; }
                public string HastaName { get; set; }
                public string HastaTC { get; set; }
                public string eye { get; set; }
                public DateTime Timestamp { get; set; } 
            }

            // List of history records
            public static List<Record> Records { get; set; } = new List<Record>();

            // Load history from file with decryption
            public static void Load()
            {
                if (File.Exists(FilePath))
                {
                    var encryptedJson = File.ReadAllText(FilePath);
                    var json = DecryptString(encryptedJson);  // Decrypt the JSON string
                    Records = JsonConvert.DeserializeObject<List<Record>>(json) ?? new List<Record>();
                }
            }

            // Save history to file with encryption
            public static void Save()
            {
                var json = JsonConvert.SerializeObject(Records, Formatting.Indented);
                var encryptedJson = EncryptString(json);  // Encrypt the JSON string
                File.WriteAllText(FilePath, encryptedJson);
            }

            // Add a new record to the history
            public static void AddRecord(string imageName, string prediction)
            {
                Records.Add(new Record
                {
                    ImageName = imageName,
                    Prediction = prediction,
                    Timestamp = DateTime.Now 
                });
            }

            
            private static string EncryptString(string plainText)
            {
                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);  
                    aesAlg.IV = new byte[16];  // Initialization vector (set to zero for simplicity)

                    ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs))
                            {
                                sw.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }

            // Decrypt a string using AES decryption
            private static string DecryptString(string cipherText)
            {
                try
                {
                    using (Aes aesAlg = Aes.Create())
                    {
                        aesAlg.Key = Encoding.UTF8.GetBytes(EncryptionKey);  
                        aesAlg.IV = new byte[16];  // Initialization vector (set to zero for simplicity)

                        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
                        using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(cipherText)))  // Try decoding Base64
                        {
                            using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                            {
                                using (StreamReader sr = new StreamReader(cs))
                                {
                                    return sr.ReadToEnd();
                                }
                            }
                        }
                    }
                }
                catch (FormatException ex)
                {
                    MessageBox.Show($"Error decoding Base64: {ex.Message}");
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error during decryption: {ex.Message}");
                    return string.Empty;
                }
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
                Kullanim kullanimWindow = new Kullanim();

            // Show the new window
            kullanimWindow.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            HistoryPage historyPage = new HistoryPage();
            historyPage.Show();
        }

       

        private void btnT_Checked(object sender, RoutedEventArgs e)
        {
             this.Background = new SolidColorBrush(Color.FromRgb(63, 17, 78));
            
            
            UploadImage.Background = new SolidColorBrush(Color.FromRgb(93, 75, 99));
            gecmis.Background = new SolidColorBrush(Color.FromRgb(93, 75, 99));
            hakkinda.Background = new SolidColorBrush(Color.FromRgb(93, 75, 99));
            kullanim.Background = new SolidColorBrush(Color.FromRgb(93, 75, 99));
            tahmin_btn.Background = new SolidColorBrush(Color.FromRgb(93, 75, 99));

            LinearGradientBrush borderBrush = new LinearGradientBrush();
            borderBrush.StartPoint = new Point(0, 0);
            borderBrush.EndPoint = new Point(1, 1);
            borderBrush.GradientStops.Add(new GradientStop(Color.FromRgb(83, 28, 101), 0.0)); 
            borderBrush.GradientStops.Add(new GradientStop(Colors.White,  2.5)); // White at the end of the gradient
            border.Background = borderBrush;

            ResultText.Background = Brushes.Transparent;

            if (btnS != null)
            {
                DockPanel.SetDock(btnS, Dock.Right);
            }

            Uri resourceUri = new Uri("images/night-sky.jpg", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.UniformToFill;
            btnT.Background = brush;

            resourceUri = new Uri("images/icons8-moon-48.png", UriKind.Relative);
            streamInfo = Application.GetResourceStream(resourceUri);

            temp = BitmapFrame.Create(streamInfo.Stream);
            brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.UniformToFill;
            btnS.Background = brush;
        }

        private void btnT_Unchecked(object sender, RoutedEventArgs e)
        {
            this.Background = new SolidColorBrush(Colors.LightSkyBlue);
            border.Background = new SolidColorBrush(Colors.LightBlue);
            ResultText.Background = new SolidColorBrush(Colors.LightBlue);
            UploadImage.Background = new SolidColorBrush(Colors.LightBlue);
            gecmis.Background = new SolidColorBrush(Colors.LightBlue);
            hakkinda.Background = new SolidColorBrush(Colors.LightBlue);
            kullanim.Background = new SolidColorBrush(Colors.LightBlue);
            tahmin_btn.Background = new SolidColorBrush(Colors.LightBlue);

            if (btnS != null)
            {
                DockPanel.SetDock(btnS, Dock.Left);
            }

            Uri resourceUri = new Uri("Images/sunny.jpg", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(resourceUri);

            BitmapFrame temp = BitmapFrame.Create(streamInfo.Stream);
            var brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.UniformToFill;
            btnT.Background = brush;

            resourceUri = new Uri("Images/icons8-sun-48.png", UriKind.Relative);
            streamInfo = Application.GetResourceStream(resourceUri);

            temp = BitmapFrame.Create(streamInfo.Stream);
            brush = new ImageBrush();
            brush.ImageSource = temp;
            brush.Stretch = Stretch.UniformToFill;
            btnS.Background = brush;
        }

        private async void tahmin_Click(object sender, RoutedEventArgs e)
        {

            var eye = "";
            if (string.IsNullOrEmpty(_uploadedImagePath))
            {
                ResultText.Text = "Lütfen önce bir görüntü yükleyin!";
                ResultText.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (hasta.Text=="" || hasta_tc.Text=="")
            {
                ResultText.Text = "Lütfen Hasta bilgilerini giriniz!";
                ResultText.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (righteye.IsChecked==false && lefteye.IsChecked==false)
            {
                ResultText.Text = "Lütfen hangi göz olduğunu belirtininz!";
                ResultText.Foreground = new SolidColorBrush(Colors.Red);
                return;

            }
            if (!(hasta_tc.Text.Length == 11) || !(hasta_tc.Text.All(char.IsDigit)))
            {
                ResultText.Text = "Lütfen geçerli TC giriniz!";
                ResultText.Foreground = new SolidColorBrush(Colors.Red);
                return;
            }
            if (righteye.IsChecked==true) 
            {
                eye = "Sağ";
            }
            if (lefteye.IsChecked == true)
            {
                eye = "Sol";
            }
            await Task.Delay(2000);
            // Perform prediction
            int predictedClass = _predictionEngine.Predict(_uploadedImagePath);

            // Labels and disease information
            string[] labels = { "Healthy Retina", "Mild NPDR", "Moderate NPDR", "Severe NPDR", "Proliferative DR" };
            var diseaseInfo = new Dictionary<int, string>
    {
        { 0, "Sağlıklı Retina: Diyabetik retinopati belirtisi saptanmadı." },
        { 1, "Hafif NPDR: Kan damarlarında küçük şişlik alanları. Düzenli izleme önerilir." },
        { 2, "Orta NPDR: Kan damarı hasarı orta düzeydedir. Bulanık görme meydana gelebilir." },
        { 3, "Şiddetli NPDR: Retina kan damarlarında önemli hasar. Acil tıbbi müdahale gerekir." },
        { 4, "Proliferative DR: Anormal damar büyümesi olan ileri evre. Görme kaybı riski. Acil tedavi gerekir." }
    };
             
            // Display the result
            if (predictedClass >= 0 && predictedClass < labels.Length)
            {
                text = diseaseInfo[predictedClass];
                ResultText.Text = $"Sonuç: {labels[predictedClass]}\n\nDetaylar: {diseaseInfo[predictedClass]}";
                ResultText.Foreground = predictedClass == 0
                    ? new SolidColorBrush(Color.FromRgb(49, 199, 114)) // Green for healthy
                    : new SolidColorBrush(Colors.Red); // Red for unhealthy

                // Add record to history
                HistoryData.Records.Add(new HistoryData.Record
                {
                    HastaTC = hasta_tc.Text,
                    HastaName = hasta.Text,
                    eye= eye,
                    ImageName = _uploadedImagePath,
                    Prediction = labels[predictedClass],
                    Timestamp = DateTime.Now
                });
            }
            else
            {
                ResultText.Text = "Sonuç: Bilinmeyen\n\nDetaylar: Daha fazla bilgi sağlayamıyoruz.";
                ResultText.Foreground = new SolidColorBrush(Colors.Gray);
            }
            
            SelectedImage_pre.Source = _preprocess.PreprocessAndDisplayImage(_uploadedImagePath);
             SelectedImage_out.Source =  _preprocess.VisualizeOutput(_uploadedImagePath);

            HistoryData.Save();
        }

        private void share_Click(object sender, RoutedEventArgs e)
        {
            if (text == null)
            {
                MessageBox.Show("Lütfen önce bir resim yükleyin.");
                return;
            }

            string predictionMessage = $"İşte tahmin sonucu:\n\n{text}";

            string whatsappUrl = $"https://wa.me/?text={Uri.EscapeDataString(predictionMessage)}";

            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo(whatsappUrl) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void logout_Click(object sender, RoutedEventArgs e)
        {
            SignInSignUpWindow signInSignUpWindow = new SignInSignUpWindow();


            signInSignUpWindow.Show();
            this.Close();
        }

        private void clear_Click(object sender, RoutedEventArgs e)
        {
            SelectedImage_pre.Source = null;
            SelectedImage_out.Source = null;
            SelectedImage.Source = null;
            hasta.Text = "";
            hasta_tc.Text = "";
            lefteye.IsChecked = false;
            righteye.IsChecked = false;
            ResultText.Text = "";
        }
    }
}
