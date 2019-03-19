using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;

namespace Steganography
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool one = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string arr = Stegoanalyzer.Instance.GetData();
                Raw.Document = new FlowDocument(new Paragraph(new Run(arr)));
            }
            catch (Exception ex)
            {
                Raw.Document = new FlowDocument(new Paragraph(new Run("Ошибка: " + ex.Message)));
            }
        }

        private void LoadFirstButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bi3 = GetImage();
            InputImage.Source = bi3;
            Analyze.IsEnabled = one;
            one = true;
            Stegoanalyzer.Instance.BitmapImage2Bitmap(bi3, true);

        }

        private BitmapImage GetImage()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != true)
            {
                return null;
            }

            BitmapImage bi3 = new BitmapImage();
            try
            {
                bi3.BeginInit();
                bi3.UriSource = new Uri(ofd.FileName);
                bi3.EndInit();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex);
            }

            return bi3;
        }

        private void LoadSecondButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bi3 = GetImage();
            InputSecondImage.Source = bi3;
            Analyze.IsEnabled = one;
            Stegoanalyzer.Instance.BitmapImage2Bitmap(bi3, false);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (First.IsChecked == null || Second.IsChecked == null)
                return;
            if (!int.TryParse(BitBox.Text, out int bits))
            {
                MessageBox.Show("Неверный формат битов");
                return;
            }
            int flag = (bool)First.IsChecked ? 1 : (bool)Second.IsChecked ? 2 : 3;
            Stegoanalyzer.Instance.Bits = bits;
            Stegoanalyzer.Instance.Flag = flag;

            string sir = "";
            try
            {
                sir = Stegoanalyzer.Instance.Reset();
            }
            catch (Exception exception)
            {
                sir = exception.Message;
            }

            Raw.Document = new FlowDocument(new Paragraph(new Run(sir)));
        }
    }
}
