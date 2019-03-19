using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Steganography
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _capacity;
        private int _current;
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void LoadFirstButton_Click(object sender, RoutedEventArgs e)
        {
            BitmapImage bi3 = GetImage();
            InputImage.Source = bi3;
            Decoder.Instance.BitmapImage2Bitmap(bi3);
            _capacity = Decoder.Instance.GetCapacity();
            AvalLabel.Text = $"{_capacity.ToString()} ед.";
            CheckCurrent();
        }

        private void CheckCurrent()
        {
            _current = Raw.Text.Length * 2;
            CurrLabel.Text = $"{_current} ед.";
            CurrLabel.Foreground = _current > _capacity ? Brushes.Red : Brushes.ForestGreen;
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

        private void SaveImage(string text)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != true)
            {
                return;
            }

            Decoder.Instance.Encode(text, sfd.FileName);
            MessageBox.Show("Сохранено!");
        }

        private void Decrypt_OnClick(object sender, RoutedEventArgs e)
        {
            Raw.Text = Decoder.Instance.Decode((BitmapImage) InputImage.Source);
        }

        private void Encrypt_OnClick(object sender, RoutedEventArgs e)
        {
            SaveImage(Raw.Text);
        }

        private void Raw_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            CheckCurrent();
        }
    }
}
