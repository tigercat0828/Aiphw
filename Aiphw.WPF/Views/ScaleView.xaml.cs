using System;
using System.Windows;
using System.Windows.Controls;
using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;

namespace Aiphw.WPF.Views {
    /// <summary>
    /// ScaleView.xaml 的互動邏輯
    /// </summary>


    public partial class ScaleView : UserControl {
        RawImage m_outputRaw;
        RawImage m_inputRaw;
        float m_ScaleFactor = 1.0f;
        string m_SceleStrategyStr;
        enum ScaleMode { NearestNeighbor, Bilinear }
        public ScaleView() {
            InitializeComponent();
            c_SaveFileBtn.IsEnabled = false;
            c_ScaleButton.IsEnabled = false;
            c_ScaleStrategyComboBox.ItemsSource = Enum.GetValues(typeof(ScaleMode));
        }
        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new() {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm",
                Title = "Open Image"
            };
            if (dialog.ShowDialog() == true) {

                RawImage loadRaw = new(dialog.FileName);

                m_inputRaw = new RawImage(loadRaw);
                m_outputRaw = new RawImage(loadRaw);

                Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
                Utility.UpdateImageBox(c_InputImgBox, m_inputRaw.ToBitmap());

                c_SaveFileBtn.IsEnabled = true;
                c_ScaleButton.IsEnabled = true;
            }
        }

        private void SaveFileBtn_Click(object sender, RoutedEventArgs e) {

            SaveFileDialog saveFileDialog = new() {
                Filter = "JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|PPM Image (*.ppm)|*.ppm",
                Title = "Save Image"
            };

            if (saveFileDialog.ShowDialog() == true) {
                string filename = saveFileDialog.FileName;
                m_outputRaw.SaveFile(filename);
            }
        }

        private void c_ScaleFactorTextBox_TextChanged(object sender, TextChangedEventArgs e) {
            bool isValid = float.TryParse(c_ScaleFactorTextBox.Text, out float value);
            if (!isValid || string.IsNullOrWhiteSpace(c_ScaleFactorTextBox.Text)) {
                value = 1.0f;
                c_ScaleFactorTextBox.Background = System.Windows.Media.Brushes.LightPink;
            }
            else {
                c_ScaleFactorTextBox.Background = System.Windows.Media.Brushes.White;
            }
            m_ScaleFactor = value;

        }

        private void ScaleFactorBtn_Click(object sender, RoutedEventArgs e) {
            if (m_SceleStrategyStr == "NearestNeighbor") {
                m_outputRaw = ImageProcessing.ScaleNearestNeighbor(m_inputRaw, m_ScaleFactor);
            }
            else {
                m_outputRaw = ImageProcessing.ScaleBilinearInterpolation(m_inputRaw, m_ScaleFactor);
            }

            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
        }

        private void c_ScaleStrategyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            m_SceleStrategyStr = (sender as ComboBox)?.SelectedItem.ToString();
        }
    }
}
