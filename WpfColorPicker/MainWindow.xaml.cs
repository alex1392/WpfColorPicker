using CycWpfLibrary;
using CycWpfLibrary.MVVM;
using CycWpfLibrary.WinForm;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointWF = System.Drawing.Point;

namespace WpfColorPicker
{

  /// <summary>
  /// MainWindow.xaml 的互動邏輯
  /// </summary>
  public partial class MainWindow : ObservableWindow
  {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool GetCursorPos(ref PointWF lpPoint);

    public MainWindow()
    {
      InitializeComponent();
      gridMain.DataContext = this;
      
      new Thread(() =>
      {
        while (true)
        {
          //Logic
          PointWF p = new PointWF();
          GetCursorPos(ref p);
          var pos = p.ToWpf();
          //Update UI
          Dispatcher.BeginInvoke(new Action(() =>
          {
            ImageSource = ScreenExtensions.CopyScreen(new Rect(pos.Minus(length / 2), pos.Add(length / 2)));
          }));

          Thread.Sleep(100);
        }
      }).Start();
    }

    private int length = 30;

    public ImageSource ImageSource { get; set; }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
      //{
      //  var pos = this.GetMouseScreenPos();
      //  ImageSource = ScreenExtensions.CopyScreen(new Rect(pos.Minus(length / 2), pos.Add(length / 2)));
      //}
    }

    private void ObservableWindow_Activated(object sender, EventArgs e)
    {
      //Mouse.Capture(this);
    }
  }
}
