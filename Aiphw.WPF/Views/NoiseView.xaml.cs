using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace Aiphw.WPF.Views {
    /// <summary>
    /// NoiseView.xaml 的互動邏輯
    /// </summary>
    public partial class NoiseView : UserControl {
        RawImage m_inputRaw;
        RawImage m_noiseRaw;
        RawImage m_outputRaw;
        public int SliderValue;
        public NoiseView() {
            InitializeComponent();
            c_GaussianBtn.IsEnabled = false;
            c_PepperBtn.IsEnabled = false;
            c_SaveFileBtn.IsEnabled = false;
            c_NoiseSlider.IsEnabled = false;
        }
        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new() {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm",
                Title = "Open Image"
            };
            if (dialog.ShowDialog() == true) {
                m_inputRaw = new(dialog.FileName);

                c_GaussianBtn.IsEnabled = true;
                c_PepperBtn.IsEnabled = true;
                c_SaveFileBtn.IsEnabled = true;
                c_NoiseSlider.IsEnabled = true;
                ResetControls();
                UpdateInputControl();
            }
        }
        private void ResetControls() {

            c_OutputImgBox.Source = null;
            c_NoiseImgBox.Source = null;
            c_OutputHistogram.Plot.Clear();
            c_NoiseHistogram.Plot.Clear();
            c_OutputHistogram.Render();
            c_NoiseHistogram.Render();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {

        }
        private void GaussianBtn_Click(object sender, RoutedEventArgs e) {

            //m_outputRaw = ImageProcessing.GaussianNoise(m_inputRaw, out m_noiseRaw, (int)c_NoiseSlider.Value);
            m_outputRaw = ImageProcessing.GaussianNoise(m_inputRaw, out m_noiseRaw, (int)c_NoiseSlider.Value);
            UpdateAllControls();
        }
        private void PepperBtn_Click(object sender, RoutedEventArgs e) {
            m_outputRaw = ImageProcessing.SaltPepperNoise(m_inputRaw, out m_noiseRaw, (int)c_NoiseSlider.Value);
            UpdateAllControls();
        }
        private void UpdateInputControl() {
            Utility.UpdateImageBox(c_InputImgBox, m_inputRaw.ToBitmap());
            var gray = ImageProcessing.GrayScale(m_inputRaw);
            Utility.SetHistogramFromChannel(c_InputHistogram.Plot, gray, 0, Color.FromArgb(128, 128, 128), "Gray");
            c_InputHistogram.Render();
        }
        private void UpdateAllControls() {
            Utility.UpdateImageBox(c_InputImgBox, m_inputRaw.ToBitmap());
            Utility.SetHistogramFromChannel(c_InputHistogram.Plot, ImageProcessing.GrayScale(m_inputRaw), 0, Color.FromArgb(128, 128, 128), "Gray");
            c_InputHistogram.Render();
            Utility.UpdateImageBox(c_NoiseImgBox, m_noiseRaw.ToBitmap());
            Utility.SetHistogramFromChannel(c_NoiseHistogram.Plot, ImageProcessing.GrayScale(m_noiseRaw), 0, Color.FromArgb(128, 128, 128), "Gray");
            c_NoiseHistogram.Render();
            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
            Utility.SetHistogramFromChannel(c_OutputHistogram.Plot, ImageProcessing.GrayScale(m_outputRaw), 0, Color.FromArgb(128, 128, 128), "Gray");
            c_OutputHistogram.Render();
        }
        private void SaveFileBtn_Click(object sender, RoutedEventArgs e) {
            SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "JPEG Image (*.jpg)|*.jpg|PNG Image (*.png)|*.png|Bitmap Image (*.bmp)|*.bmp|PPM Image (*.ppm)|*.ppm";
            saveFileDialog.Title = "Save Image";

            if (saveFileDialog.ShowDialog() == true) {
                string filename = saveFileDialog.FileName;
                m_outputRaw.SaveFile(filename);
            }
        }
    }
}
