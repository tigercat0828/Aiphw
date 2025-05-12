using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;

namespace Aiphw.WPF.Views {
    /// <summary>
    /// HistogramView.xaml 的互動邏輯
    /// </summary>
    public partial class HistogramView : UserControl {
        RawImage m_outputRaw;

        public HistogramView() {
            InitializeComponent();
            c_SaveFileBtn.IsEnabled = false;
        }
        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new() {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm",
                Title = "Open Image"
            };

            if (dialog.ShowDialog() == true) {

                RawImage loadRaw = new(dialog.FileName);

                m_outputRaw = ImageProcessing.GrayScale(loadRaw);
                Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());

                SetImageInfoTextBlock();
                DrawHistogram();

                c_SaveFileBtn.IsEnabled = true;
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
        public void SetImageInfoTextBlock() {
            c_ImageInfoText.Text = $"{m_outputRaw.Width} x {m_outputRaw.Height} = {m_outputRaw.Width * m_outputRaw.Height}";
        }
        private void DrawHistogram() {

            Utility.SetHistogramFromChannel(c_HistoGraph.Plot, m_outputRaw, channel: 0, Color.FromArgb(128, 128, 128), "gray");

            // c_HistoGraph.Render();
            c_HistoGraph.Refresh();
        }


    }
}
