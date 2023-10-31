// See https://aka.ms/new-console-template for more information
using Aiphw.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

//var image = new RawImage("./Assets/wolf.png");

var summary1 = BenchmarkRunner.Run<BitwiseVSarrayGray>();


public class BitwiseVSarrayGray {

    [Benchmark]
    public void Rawimage() {
        //RawImage image = new("./Assets/wolf.png");
        //ImageProcessing.GaussianNoise(image, out RawImage noise, 5);
    }
    [Benchmark]
    public void Rawimage_MT() {
        //RawImage image = new("./Assets/wolf.png");
        
    }

}