using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;
using ScottPlot;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;


namespace Aiphw.WPF.Views {
    /// <summary>
    /// HistoEqualizeView.xaml 的互動邏輯
    /// </summary>
    public partial class HistoEqualizeView : UserControl {
        public HistoEqualizeView() {
            InitializeComponent();
            c_SaveFileBtn.IsEnabled = false;
        }
        RawImage m_inputRaw;
        RawImage m_outputRaw;
        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new() {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm",
                Title = "Open Image"
            };
            if (dialog.ShowDialog() == true) {


                RawImage loadRaw = new(dialog.FileName);

                m_inputRaw = ImageProcessing.GrayScale(new RawImage(loadRaw));
                m_outputRaw = ImageProcessing.HistoEqualizeGrayscale(m_inputRaw);

                // equalization

                Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
                Utility.UpdateImageBox(c_InputImgBox, m_inputRaw.ToBitmap());

                DrawHistogram(c_InputHisto, m_inputRaw);
                DrawHistogram(c_OutputHisto, m_outputRaw);


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
        private void DrawHistogram(WpfPlot plotControl, RawImage image) {

            Utility.SetHistogramFromChannel(plotControl.Plot, image, channel: 0, Color.FromArgb(128, 128, 128), "gray");

            plotControl.Render();
        }
    }


}
