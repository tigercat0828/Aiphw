using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace Aiphw.WPF.Views {

    public partial class RotationView : UserControl {

        RawImage m_outputRaw;
        RawImage m_inputRaw;
        public RotationView() {

            InitializeComponent();
            c_SaveFileBtn.IsEnabled = false;
            c_LeftRotateBtn.IsEnabled = false;
            c_RightRotateBtn.IsEnabled = false;
        }

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new();
            dialog.Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm";
            dialog.Title = "Open Image";
            if (dialog.ShowDialog() == true) {

                RawImage loadRaw = new(dialog.FileName);

                m_inputRaw = new RawImage(loadRaw);
                m_outputRaw = new RawImage(loadRaw);

                Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
                Utility.UpdateImageBox(c_InputImgBox, m_inputRaw.ToBitmap());

                c_SaveFileBtn.IsEnabled = true;
                c_LeftRotateBtn.IsEnabled = true;
                c_RightRotateBtn.IsEnabled = true;
            }
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

        private void RightRotateBtn_Click(object sender, RoutedEventArgs e) {
            m_outputRaw = ImageProcessing.RightRotate(m_outputRaw);
            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
        }

        private void LeftRotateBtn_Click(object sender, RoutedEventArgs e) {
            m_outputRaw = ImageProcessing.LeftRotate(m_outputRaw);
            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
        }

    }
}
