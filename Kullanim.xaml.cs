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

namespace Bitirme
{
    /// <summary>
    /// Interaction logic for Kullanim.xaml
    /// </summary>
    public partial class Kullanim : Window
    {
        public Kullanim()
        {
            InitializeComponent();
        }

        private void VideoBackground_MediaEnded(object sender, RoutedEventArgs e)
        {

            VideoBackground.Position = TimeSpan.Zero; // Restart the video
            VideoBackground.Play();
        }
    }
}
