using CycWpfLibrary;
using CycWpfLibrary.Emgu;
using CycWpfLibrary.FluentDesign;
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
using ColorWF = System.Drawing.Color;
using ColorWpf = System.Windows.Media.Color;
using PointWF = System.Drawing.Point;
using PointWpf = System.Windows.Point;

namespace WpfColorPicker
{
  public enum AppState
  {
    Dropped, // color fixed
    Dropping, //update colors
  }
  public class Data
  {
    public Data(string name, object value)
    {
      Name = name;
      Value = value;
    }
    public string Name { get; set; }
    public object Value { get; set; }
  }

  /// <summary>
  /// MainWindow.xaml 的互動邏輯
  /// </summary>
  public partial class MainWindow : RevealWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      gridMain.DataContext = this;
      this.Loaded += MainWindow_Loaded;
      this.MouseDown += MainWindow_MouseDown;
      this.Deactivated += MainWindow_Deactivated;
      this.Activated += MainWindow_Activated;
      
      Task.Run(() =>
      {
        while (true)
        {
          if (IsActivated)
          {
            PointWF posWF = new PointWF();
            ScreenMethods.GetCursorPos(ref posWF);
            CursorPos = posWF.ToWpf();
            Dispatcher.BeginInvoke(new Action(() => Update()));
          }

          Thread.Sleep(50);
        }
      });
    }

    bool IsActivated;
    private void MainWindow_Activated(object sender, EventArgs e)
    {
      IsActivated = true;
    }
    private void MainWindow_Deactivated(object sender, EventArgs e)
    {
      IsActivated = false;
    }

    private const int length = 30;
    private const int halfLength = length / 2;

    public PointWpf CursorPos { get; set; }
    public Data[] CursorPosSource => new Data[]
    {
      new Data("X:", CursorPos.X),
      new Data("Y:", CursorPos.Y),
    };

    public Image<Bgra, byte> Image { get; set; }
    public BitmapSource ImageSource => Image?.ToBitmapSource();

    public byte B { get; set; }
    public byte G { get; set; }
    public byte R { get; set; }
    public byte A { get; set; }
    public Data[] RgbaSource => new Data[]
    {
      new Data("R:", R),
      new Data("G:", G),
      new Data("B:", B),
    };
    public string HexString { get; set; }
    public Data[] HexSource => new Data[]
    {
      new Data("Hex:", HexString),
    };

    public SolidColorBrush ColorBrush { get; set; }

    private void Update()
    {
      Image = ScreenMethods.CopyScreen(new Rect(CursorPos.Minus(halfLength), CursorPos.Add(halfLength))).ToImage<Bgra, byte>();
      if (State == AppState.Dropped)
        return;
      var data = Image.Data;
      B = data[halfLength, halfLength, 0];
      G = data[halfLength, halfLength, 1];
      R = data[halfLength, halfLength, 2];
      A = data[halfLength, halfLength, 3];
      ColorBrush = new SolidColorBrush(ColorWpf.FromArgb(A, R, G, B));
      HexString = new byte[] { A, R, G, B }.ToHexString();
    }

    private AppState state = default(AppState);
    public AppState State
    {
      get => state;
      set
      {
        state = value;
        switch (state)
        {
          case AppState.Dropped:
            dropperButton.IsEnabled = true;
            ReleaseMouseCapture();
            Cursor = Cursors.AppStarting;
            break;
          case AppState.Dropping:
            dropperButton.IsEnabled = false;
            CaptureMouse();
            Cursor = cursorDropper;
            break;
          default:
            break;
        }
      }
    }
    private static readonly Cursor cursorDropper = new Bitmap(Application.GetResourceStream(new Uri("resources/dropper.png", UriKind.RelativeOrAbsolute)).Stream).ToCursor(new PointWpf(0.125, 0.75));
    private void dropperButton_Click(object sender, RoutedEventArgs e)
    {
      State = AppState.Dropping; // Dropped -> Dropping      
    }

    private void MainWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (State == AppState.Dropping)
        State = AppState.Dropped; // Dropping -> Dropped
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      State = AppState.Dropping;
    }
  }
}
