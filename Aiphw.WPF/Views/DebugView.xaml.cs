using Aiphw.Models;
using Aiphw.WPF.Extensions;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace Aiphw.WPF.Views {
    /// <summary>
    /// DebugView.xaml 的互動邏輯
    /// </summary>
    public partial class DebugView : UserControl {
        public DebugView() {
            InitializeComponent();
            AllocConsole();
            Console.WriteLine("Console Launch !! :)");
        }
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();
        private void DebugMain(object sender, RoutedEventArgs e) {
            uint pixel = 0x12345678;
            Console.WriteLine();
            Console.WriteLine("A" + (pixel >> 24 & 0xFF).ToString("X"));
            Console.WriteLine("R" + (pixel >> 16 & 0xFF).ToString("X"));
            Console.WriteLine("G" + (pixel >> 8 & 0xFF).ToString("X"));
            Console.WriteLine("B" + (pixel >> 0 & 0xFF).ToString("X"));
            Console.WriteLine();


            RawImage raw = new("./Assets/wolf.png");

            Utility.UpdateImageBox(c_ShowImgBox, raw.ToBitmap());

            raw.SaveFile("./test.ppm");
        }
    }
}
