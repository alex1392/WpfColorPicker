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
      this.Closed += MainWindow_Closed;

      Task.Run(() =>
      {
        while (true)
        {
          if (IsActivated)
          {
            PointWF posWF = new PointWF();
            ScreenMethods.GetCursorPos(ref posWF);
            var posWpf = posWF.ToWpf();
            var image = ScreenMethods.CopyScreen(new Rect(CursorPos.Minus(halfLength), CursorPos.Add(halfLength))).ToImage<Bgra, byte>();
            Dispatcher.BeginInvoke(new Action(() =>
            {
              CursorPos = posWpf;
              Image = image;
            }));

            if (state != AppState.Dropped)
            {
              var data = image.Data;
              var b = data[halfLength, halfLength, 0];
              var g = data[halfLength, halfLength, 1];
              var r = data[halfLength, halfLength, 2];
              var a = data[halfLength, halfLength, 3];
              var hex = new byte[] { a, r, g, b }.ToHexString();
              Dispatcher.BeginInvoke(new Action(() =>
              {
                B = b;
                G = g;
                R = r;
                A = a;
                ColorBrush = new SolidColorBrush(ColorWpf.FromArgb(a, r, g, b));
                HexString = hex;
              }));
            }
          }

          Thread.Sleep(50);
        }
      });
    }

    private bool IsActivated;
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
    public SolidColorBrush DroppedBrush { get; set; } = new SolidColorBrush(Colors.Black);
    public FontWeight DroppedWeight { get; set; } = FontWeight.FromOpenTypeWeight(400); //Normal = 400, Bold = 700

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
            Cursor = Cursors.AppStarting;
            DroppedBrush = new SolidColorBrush(ColorWpf.FromArgb(255, 249, 97, 49));
            DroppedWeight = FontWeight.FromOpenTypeWeight(700);
            ReleaseMouseCapture();
            backWindow.Hide();
            this.Activate();
            break;
          case AppState.Dropping:
            dropperButton.IsEnabled = false;
            Cursor = cursorDropper;
            DroppedBrush = new SolidColorBrush(Colors.LightGray);
            DroppedWeight = FontWeight.FromOpenTypeWeight(400);
            CaptureMouse();
            backWindow.Show();
            this.Activate();
            break;
          default:
            break;
        }
      }
    }

    private Window backWindow = new Window
    {
      AllowsTransparency = true,
      WindowStyle = WindowStyle.None,
      WindowState = WindowState.Maximized,
      Cursor = cursorDropper,
      Background = new SolidColorBrush(ColorWpf.FromArgb(1, 0, 0, 0)),
      ShowInTaskbar = false,
      IsHitTestVisible = false,
    };
    private void MainWindow_Closed(object sender, EventArgs e)
    {
      backWindow.Close();
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
