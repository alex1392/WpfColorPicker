using CycWpfLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WpfColorPicker
{
  /// <summary>
  /// App.xaml 的互動邏輯
  /// </summary>
  public partial class App : Application
  {
    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);
      LoadLanguage();
    }

    private Uri defaultUri = new Uri(@"pack://application:,,,/WpfColorPicker;component/lang/default.xaml", UriKind.Absolute);
    private void LoadLanguage()
    {
      CultureInfo currentCultureInfo = CultureInfo.CurrentCulture;

      ResourceDictionary rd = null;

      try
      {
        rd = LoadComponent(new Uri( @"lang/" + currentCultureInfo.Name + ".xaml", UriKind.Relative)) as ResourceDictionary;
      }
      catch
      {
      }

      if (rd != null)
      {
        if (this.Resources.MergedDictionaries.Count > 0)
        {
          foreach (var d in Resources.MergedDictionaries)
          {
            if (d.Source == defaultUri)
            {
              d.Clear();
              break;
            }
          }
          
        }
        this.Resources.MergedDictionaries.Add(rd);
      }
    }
  }
}
