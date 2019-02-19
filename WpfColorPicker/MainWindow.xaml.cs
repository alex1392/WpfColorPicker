using CycWpfLibrary;
using CycWpfLibrary.Emgu;
using CycWpfLibrary.Media;
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
  /// <summary>
  /// MainWindow.xaml 的互動邏輯
  /// </summary>
  public partial class MainWindow : ObservableWindow
  {
    public MainWindow()
    {
      InitializeComponent();
      gridMain.DataContext = this;

      Task.Run(() =>
      {
        while (true)
        {
          PointWF posWF = new PointWF();
          ScreenMethods.GetCursorPos(ref posWF);
          CursorPos = posWF.ToWpf();
          Dispatcher.BeginInvoke(new Action(() => Update()));

          Thread.Sleep(50);
        }
      });
    }

    private void mainWindow_Loaded(object sender, RoutedEventArgs e)
    {
      State = AppState.Dropping;
    }

    private const int length = 30;
    private const int halfLength = length / 2;

    public PointWpf CursorPos { get; set; }

    public Image<Bgra, byte> Image { get; set; }
    public BitmapSource ImageSource => Image?.ToBitmapSource();

    public byte B { get; set; }
    public byte G { get; set; }
    public byte R { get; set; }
    public byte A { get; set; }
    public SolidColorBrush ColorBrush { get; set; }
    public string HexString { get; set; }

    private void Update()
    {
      Image = ScreenMethods.CopyScreen(new Rect(CursorPos.Minus(halfLength), CursorPos.Add(halfLength)));
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
    private static readonly Cursor cursorDropper = new Bitmap(Application.GetResourceStream(new Uri("resources/dropper.png", UriKind.RelativeOrAbsolute)).Stream).ToCursor(new PointWpf(0.5, 0.5));
    private void dropperButton_Click(object sender, RoutedEventArgs e)
    {
      State = AppState.Dropping; // Dropped -> Dropping      
    }

    private void mainWindow_MouseUp(object sender, MouseButtonEventArgs e)
    {
    }

    private void mainWindow_MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (State == AppState.Dropping)
      {
        State = AppState.Dropped; // Dropping -> Dropped
      }

    }

    private void mainWindow_MouseLeave(object sender, MouseEventArgs e)
    {
    }
  }
}
