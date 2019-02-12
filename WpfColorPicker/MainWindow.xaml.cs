using CycWpfLibrary;
using CycWpfLibrary.Emgu;
using CycWpfLibrary.Media;
using CycWpfLibrary.MVVM;
using CycWpfLibrary.WinForm;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PointWF = System.Drawing.Point;

namespace WpfColorPicker
{
  /// <summary>
  /// MainWindow.xaml 的互動邏輯
  /// </summary>
  public partial class MainWindow : ObservableWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      gridMain.DataContext = this;

      //Task.Run(async () =>
      //{
      //  while (true)
      //  {
      //    PointWF posWF = new PointWF();
      //    ScreenMethods.GetCursorPos(ref posWF);
      //    var posWpf = posWF.ToWpf();
      //    await Dispatcher.BeginInvoke(new Action(() =>
      //    {
      //      ImageSource = ScreenMethods.CopyScreen(new Rect(posWpf.Minus(length / 2), posWpf.Add(length / 2)));
      //    }));

      //    await Task.Delay(100);
      //  }
      //});

      Task.Run(() =>
      {
        while (true)
        {
          PointWF posWF = new PointWF();
          ScreenMethods.GetCursorPos(ref posWF);
          var posWpf = posWF.ToWpf();
          Dispatcher.BeginInvoke(new Action(() =>
          {
            Image = ScreenMethods.CopyScreen(new Rect(posWpf.Minus(length / 2), posWpf.Add(length / 2)));
          }));

          Thread.Sleep(50);
        }
      });
    }

    private int length = 30;

    public BitmapSource ImageSource => Image?.ToBitmapSource();

    public Image<Bgra, byte> Image { get; set; }

    public byte B => Image?.Data[Image.Width / 2, Image.Height / 2, 0] ?? 0;
    public byte G => Image?.Data[Image.Width / 2, Image.Height / 2, 1] ?? 0;
    public byte R => Image?.Data[Image.Width / 2, Image.Height / 2, 2] ?? 0;
    public byte A => Image?.Data[Image.Width / 2, Image.Height / 2, 3] ?? 0;

    public string Bstring => "B = " + B.ToString();
    public string Gstring => "G = " + G.ToString();
    public string Rstring => "R = " + R.ToString();
    public string Astring => "A = " + A.ToString();
    public string HexString => new byte[] { A, R, G, B }.ToHexString();
  }
}
