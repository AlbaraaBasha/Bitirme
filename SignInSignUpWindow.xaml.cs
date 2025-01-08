using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using static Bitirme.MainWindow;

namespace Bitirme
{
    public partial class SignInSignUpWindow : Window
    {
        private readonly FireBaseServices firebaseService;
        
        public SignInSignUpWindow()
        {
            InitializeComponent();

            firebaseService = new FireBaseServices();
        }
        
      
        // Handle Sign In button click
        private async void SignIn_Click(object sender, RoutedEventArgs e)
        {
            
            var username = SignInUsername.Text;
            var password = SignInPassword.Password;
            var name = await firebaseService.GetUserName(username);
            UserData.UserName = username;
            UserData.Name = name;

            var user = await firebaseService.GetUser(username, password);

            if (user != null)
            {
                // Initialize HistoryData with the current user

                ShowAutoClosingMessageBox($"Hoşgeldiniz {name}!", "Oturum açıldı", 2000, false);

                MainWindow mainWindow = new MainWindow();
                await HistoryData.Initialize(firebaseService);
                HistoryData.Load();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ShowTemporaryMessage("Kullanıcı adı veya şifre hatalı", gmesaj, 2000);

            }
        }

        // Handle Sign Up button click
        private async void SignUp_Click(object sender, RoutedEventArgs e)
        {
            var username = SignUpUsername.Text;
            var password = SignUpPassword.Password;
            var name = Username_Copy.Text;

            if (SignUpPassword.Password != SignUpConfirmPassword.Password  )
            {
                ShowTemporaryMessage("Şifreler eşleşmiyor!", cmesaj, 2000);              
                return;
            }
            else if ( SignUpPassword.Password == "" || SignUpConfirmPassword.Password == "") 
            {
                ShowTemporaryMessage("Lütfen şifre girin.", cmesaj, 2000);            
                return;
            }
            else if ( SignUpUsername.Text == "")
            {
                ShowTemporaryMessage("Lütfen kullanıcı adı girin.", cmesaj, 2000);
                return;
            }
            if (SignUpPassword.Password.Length < 8 ||
                !SignUpPassword.Password.Any(char.IsUpper) ||
                !SignUpPassword.Password.Any(char.IsLower) ||
                !SignUpPassword.Password.Any(char.IsDigit) ||
                !SignUpPassword.Password.Any(ch => !char.IsLetterOrDigit(ch)))
            {
                ShowAutoClosingMessageBox("Şifre en az 8 haneli olmalı.\n" +
                    "En az 1 büyük harf, 1 küçük harf,\n" +
                   "1 rakam ve 1 özel karakter içermelidir.", "Hata", 5000, true);
                
                return;
            }
            // Check if the username already exists in the database
            var existingUser = await firebaseService.GetUserByUsername(username);
            if (existingUser != null)
            {
                ShowTemporaryMessage("Bu kullanıcı adı zaten mevcut.", cmesaj, 4000);
                return;
            }

            else
            {
                string key = GenerateEncryptionKey();
                var user = new Bitirme.User { Username = username, Password = password, Key=key, Name=name};
                var result = firebaseService.AddUser(user);
                if (result != null)
                {
                    ShowAutoClosingMessageBox($"Kayıt başarılı. {SignInUsername.Text}!", "Başarılı", 1500, true);
                    SignUpPanel.Visibility = Visibility.Collapsed;
                    SignInPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show("Kayıt işlemi sırasında bir hata oluştu.", "Hata");
                }
            }

            
        }
        private static string GenerateEncryptionKey()
        {
            var random = new System.Security.Cryptography.RNGCryptoServiceProvider();
            byte[] keyBytes = new byte[16]; // 16 bytes = 128 bits for AES encryption
            random.GetBytes(keyBytes);

            return Convert.ToBase64String(keyBytes);
        }

        private void SignInPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (SignInPassword.Password.Length > 0)
            {
                SignInPasswordPlaceholder.Visibility = Visibility.Collapsed;  // Hide placeholder when user starts typing
            }
            else
            {
                SignInPasswordPlaceholder.Visibility = Visibility.Visible;    // Show placeholder when the password box is empty
            }
        }
        private void SignUpPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (SignUpPassword.Password.Length > 0)
            {
                SignUpPasswordPlaceholder.Visibility = Visibility.Collapsed;  // Hide placeholder when user starts typing
            }
            else
            {
                SignUpPasswordPlaceholder.Visibility = Visibility.Visible;    // Show placeholder when the password box is empty
            }
        }

        private void SignUpConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (SignUpConfirmPassword.Password.Length > 0)
            {
                SignUpConfirmPasswordPlaceholder.Visibility = Visibility.Collapsed;  // Hide placeholder when user starts typing
            }
            else
            {
                SignUpConfirmPasswordPlaceholder.Visibility = Visibility.Visible;    // Show placeholder when the confirm password box is empty
            }
        }


        // Show Sign In panel and hide Sign Up panel
        private void SignUpLink_Click(object sender, MouseButtonEventArgs e)
        {
            SignInPanel.Visibility = Visibility.Collapsed;
            SignUpPanel.Visibility = Visibility.Visible;
        }

        // Show Sign Up panel and hide Sign In panel
        private void SignInLink_Click(object sender, MouseButtonEventArgs e)
        {
            SignUpPanel.Visibility = Visibility.Collapsed;
            SignInPanel.Visibility = Visibility.Visible;
        }



        public void ShowAutoClosingMessageBox(string message, string title, int timeoutMilliseconds, bool showCloseButton = true)
        {
            var customMessageBox = new Window
            {
                Title = title,
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ResizeMode = ResizeMode.NoResize,
                Background = new LinearGradientBrush(Colors.SkyBlue, Colors.Purple, 93),
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.BlueViolet
            };

            var grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            if (showCloseButton)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            var textBlock = new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 14,
                Margin = new Thickness(10)
            };

            Grid.SetRow(textBlock, 0);
            grid.Children.Add(textBlock);

            if (showCloseButton)
            {
                var closeButton = new Button
                {
                    Content = "Tamam",
                    Width = 80,
                    Height = 30,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(5)
                };

                closeButton.Click += (s, e) => customMessageBox.Close();

                Grid.SetRow(closeButton, 1);
                grid.Children.Add(closeButton);
            }

            customMessageBox.Content = grid;

            customMessageBox.Loaded += (s, e) =>
            {
                Task.Delay(timeoutMilliseconds).ContinueWith(_ =>
                {
                    customMessageBox.Dispatcher.Invoke(customMessageBox.Close);
                });
            };

            customMessageBox.ShowDialog();
        }

        private void ShowTemporaryMessage(string message, TextBlock textBlock, int durationMilliseconds)
        {
            // Set the message text
            textBlock.Text = message;

            // Create a DispatcherTimer
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(durationMilliseconds)
            };

            // Set the timer's Tick event
            timer.Tick += (s, e) =>
            {
                // Clear the TextBlock's text and stop the timer
                textBlock.Text = string.Empty;
                timer.Stop();
            };

            // Start the timer
            timer.Start();
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && SignInPanel.Visibility == Visibility.Visible)
            {
                SignIn_Click(SignInB, new RoutedEventArgs());
            }

            if (e.Key == Key.Enter && SignUpPanel.Visibility == Visibility.Visible)
            {
                SignUp_Click(SignUpB, new RoutedEventArgs());
            }
        }

        public static class UserData
        {
            public static string UserName { get; set; }
            public static string Name { get; set; }
        }


    }
}
