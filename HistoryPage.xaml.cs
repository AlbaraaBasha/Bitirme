using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Bitirme.MainWindow;

namespace Bitirme
{
    /// <summary>
    /// Interaction logic for HistoryPage.xaml
    /// </summary>
    public partial class HistoryPage : Window
    {
        private string _hastaTC;

        // Constructor to accept HastaTC and filter history
        public HistoryPage()
        {
            InitializeComponent();
            HistoryGrid.ItemsSource = HistoryData.Records
                .Select(r => new { r.HastaTC, r.HastaName, r.eye, r.ImageName, r.Prediction, r.Timestamp }) // ImageName >> image path
                .ToList();
        }
        public HistoryPage(string hastaTC)
        {
            InitializeComponent();
            _hastaTC = hastaTC;

            // Filter records by HastaTC
            LoadHistory();
        }

        private void LoadHistory()
        {
            var filteredRecords = HistoryData.Records
                .Where(r => r.HastaTC == _hastaTC)  // Filter records for the selected patient
                .Select(r => new { r.HastaTC, r.HastaName, r.eye, r.ImageName, r.Prediction, r.Timestamp })
                .ToList();

            HistoryGrid.ItemsSource = filteredRecords;
        }

        private void HistoryGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Get the selected item
            var selectedRecord = HistoryGrid.SelectedItem;

            if (selectedRecord != null)
            {
                // Retrieve HastaTC from the selected record
                string? hastaTC = (string?)selectedRecord.GetType().GetProperty("HastaTC")?.GetValue(selectedRecord, null);
                string? image = (string?)selectedRecord.GetType().GetProperty("ImageName")?.GetValue(selectedRecord, null);
                string? zaman = (string?)selectedRecord.GetType().GetProperty("Timestamp")?.GetValue(selectedRecord, null).ToString();
                string? sonuc = (string?)selectedRecord.GetType().GetProperty("Prediction")?.GetValue(selectedRecord, null);
                string? eye = (string?)selectedRecord.GetType().GetProperty("eye")?.GetValue(selectedRecord, null);
               

                // Open the profile page and pass the HastaTC
                if (!string.IsNullOrEmpty(hastaTC))
                {
                    ProfilePage profilePage = new ProfilePage(hastaTC, image ,zaman, sonuc, eye);
                    profilePage.Show();
                    this.Close();
                }
            }
        }

        private void filter_Click(object sender, RoutedEventArgs e)
        {
            if (!(hastatc.Text=="")) {
                _hastaTC = hastatc.Text;
                LoadHistory();
            }
            else {
                HistoryGrid.ItemsSource = HistoryData.Records
               .Select(r => new { r.HastaTC, r.HastaName, r.eye, r.ImageName, r.Prediction, r.Timestamp }) // Include Timestamp
               .ToList();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                filter_Click(share, new RoutedEventArgs());
            }

     
        }


    }
}
