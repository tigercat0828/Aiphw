using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Aiphw.WPF.Views {

    public partial class ConvolutionView : UserControl {

        TextBox[,] c_MaskCellTextBox = new TextBox[5, 5];
        int m_MaskSize;
        RawImage m_inputRaw;
        RawImage m_outputRaw;
        public ConvolutionView() {
            InitializeComponent();
            BuildMaskControl();
            c_SaveFileBtn.IsEnabled = false;
            c_SmoothBtn.IsEnabled = false;
            c_EdgeDetectBtn.IsEnabled = false;
            c_ProcessBtn.IsEnabled = false;
        }

        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new() {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm",
                Title = "Open Image"
            };
            if (dialog.ShowDialog() == true) {
                c_SaveFileBtn.IsEnabled = true;
                c_SmoothBtn.IsEnabled = true;
                c_EdgeDetectBtn.IsEnabled = true;
                c_ProcessBtn.IsEnabled = true;
                RawImage loadRaw = new(dialog.FileName);
                m_inputRaw = new(loadRaw);
                m_outputRaw = new(loadRaw);
                Utility.UpdateImageBox(c_InputImgBox, m_inputRaw.ToBitmap());
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

        private void ProcessBtn_Click(object sender, RoutedEventArgs e) {
            float[] rawMask = GetCustomMaskCell();
            MaskKernel kernel = new MaskKernel(rawMask);
            m_outputRaw = ImageProcessing.ConvolutionFullColor(m_inputRaw, kernel);
            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
        }

        private void SetCustomMaskCell(string text) {
            int start = (m_MaskSize == 5) ? 0 : 1;
            int end = (m_MaskSize == 5) ? 5 : 4;
            for (int i = start; i < end; i++) {
                for (int j = start; j < end; j++) {
                    c_MaskCellTextBox[i, j].Text = text;
                }
            }
        }
        private float[] GetCustomMaskCell() {
            List<float> rawMask = new(25);
            int start = (m_MaskSize == 5) ? 0 : 1;
            int end = (m_MaskSize == 5) ? 5 : 4;
            for (int i = start; i < end; i++) {
                for (int j = start; j < end; j++) {
                    rawMask.Add(float.Parse(c_MaskCellTextBox[i, j].Text));
                }
            }
            return rawMask.ToArray();
        }
        private void SetMaskSize(int size) {
            m_MaskSize = size;
            bool isEnabled = (size == 3) ? false : true;
            string text = (size == 3) ? string.Empty : "1";
            for (int i = 0; i < 5; i++) {
                c_MaskCellTextBox[0, i].IsEnabled = isEnabled;
                c_MaskCellTextBox[0, i].Text = text;
                c_MaskCellTextBox[4, i].IsEnabled = isEnabled;
                c_MaskCellTextBox[4, i].Text = text;
            }
            for (int i = 1; i < 4; i++) {
                c_MaskCellTextBox[i, 0].IsEnabled = isEnabled;
                c_MaskCellTextBox[i, 0].Text = text;
                c_MaskCellTextBox[i, 4].IsEnabled = isEnabled;
                c_MaskCellTextBox[i, 4].Text = text;
            }
        }

        private void c_SmoothBtn_Click(object sender, RoutedEventArgs e) {
            //RawImage grayScale = ImageProcessing.GrayScale(m_inputRaw);
            //m_outputRaw = ImageProcessing.Smooth(grayScale);
            m_outputRaw = ImageProcessing.Smooth(m_inputRaw);
            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
        }

        private void c_EdgeDetectBtn_Click(object sender, RoutedEventArgs e) {
            m_outputRaw = ImageProcessing.EdgeDetection(m_inputRaw);
            Utility.UpdateImageBox(c_OutputImgBox, m_outputRaw.ToBitmap());
        }
        private void Size3x3Btn_Click(object sender, RoutedEventArgs e) {
            SetMaskSize(3);

        }
        private void Size5x5Btn_Click(object sender, RoutedEventArgs e) {
            SetMaskSize(5);
        }
        private void ResetAllZeroBtn_Click(object sender, RoutedEventArgs e) {
            SetCustomMaskCell("0");
        }
        private void ResetAllOneBtn_Click(object sender, RoutedEventArgs e) {
            SetCustomMaskCell("1");
        }

        private void BuildMaskControl() {
            // Loop to create and add 25 TextBox controls to the MaskGrid
            for (int i = 0; i < 5; i++) {
                for (int j = 0; j < 5; j++) {
                    TextBox textBox = new TextBox();
                    textBox.Name = $"m{i}{j}";
                    textBox.Text = "0";
                    textBox.Height = 40;
                    textBox.FontSize = 20;
                    textBox.TextAlignment = TextAlignment.Center;
                    textBox.VerticalContentAlignment = VerticalAlignment.Center;
                    textBox.GotFocus += (sender, e) => {
                        TextBox textBox = (TextBox)sender;
                        textBox.SelectAll();
                    };

                    Grid.SetRow(textBox, i);
                    Grid.SetColumn(textBox, j);
                    c_MaskCellTextBox[i, j] = textBox;
                    MaskGrid.Children.Add(textBox);
                }
            }
        }

    }
}
