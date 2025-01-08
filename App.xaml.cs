using System.Configuration;
using System.Data;
using System.Windows;
using static Bitirme.MainWindow;

namespace Bitirme
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // Uygulama başladığında geçmişi yükle
            HistoryData.Load();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            // Uygulama kapandığında geçmişi kaydet
            HistoryData.Save();
        }


    }

}
