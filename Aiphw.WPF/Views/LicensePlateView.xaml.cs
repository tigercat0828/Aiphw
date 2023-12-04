using Aiphw.Models;
using Aiphw.WPF.Extensions;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aiphw.WPF.Views {
    /// <summary>
    /// LicensePlateView.xaml 的互動邏輯
    /// </summary>
    public partial class LicensePlateView : UserControl {
     
        
        public LicensePlateView() {
            InitializeComponent();
        }
        RawImage m_InputRaw;
        RawImage m_RedChannel;
        RawImage m_GreenChannel;
        RawImage m_BlueChannel;
        RawImage m_GlobalBinarize;
        RawImage m_Mosaic;
        RawImage m_LocalBinarize;
        RawImage m_Outline;
        RawImage m_Empty;
        private void OpenFileBtn_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog dialog = new() {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp; *.ppm)|*.jpg; *.jpeg; *.png; *.bmp; *.ppm",
                Title = "Open Image"
            };
            if (dialog.ShowDialog() == true) {

                m_InputRaw = new(dialog.FileName);
                m_RedChannel = ImageProcessing.RedChannel(m_InputRaw);
                m_GreenChannel = ImageProcessing.GreenChannel(m_InputRaw);
                m_BlueChannel = ImageProcessing.BlueChannel(m_InputRaw);
                m_GlobalBinarize = ImageProcessing.BinarizeGlobal(m_InputRaw, 128);
                int cellsize = 30;
                m_Mosaic = ImageProcessing.Mosaic(m_InputRaw, cellsize);
                m_LocalBinarize = ImageProcessing.BinarizeLocal(m_InputRaw, cellsize);
                m_Outline = ImageProcessing.Outline(m_LocalBinarize);
                Utility.UpdateImageBox(c_InputImgBox, m_InputRaw.ToBitmap());
                Utility.UpdateImageBox(c_RedChannelImgBox, m_RedChannel.ToBitmap());
                Utility.UpdateImageBox(c_GreenChannelImgBox, m_GreenChannel.ToBitmap());
                Utility.UpdateImageBox(c_BlueChannelImgBox, m_BlueChannel.ToBitmap());
                Utility.UpdateImageBox(c_GlobalBinarizeImgBox, m_GlobalBinarize.ToBitmap());
                Utility.UpdateImageBox(c_MosaicImgBox, m_Mosaic.ToBitmap());
                Utility.UpdateImageBox(c_LocalBinarizeImgBox, m_LocalBinarize.ToBitmap());
                Utility.UpdateImageBox(c_OutlineImgBox, m_Outline.ToBitmap());
            }
        }
        
    }
}
