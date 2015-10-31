using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace MOLL_Controller {
  /// <summary>
  /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
  /// </summary>
  public sealed partial class MainPage : Page {

    private const string EXIST_SETTINGS = "EXIST_SETTINGS";
    private const string VELOCITY_SETTING = "VELOCITY";
    private const string VELOCITY_INDIVIDUAL_SETTING = "VELOCITY_INDIVIDUAL";
    private const string VELOCITY_LEFT_SETTING = "VELOCITY_LEFT";
    private const string VELOCITY_RIGHT_SETTING = "VELOCITY_RIGHT";
    private const string SENSOR_THRESHOLD_SETTING = "SENSOR_THRESHOLD";
    private const string BACK_PERIOD_SETTING = "BACK_PERIOD";
    private const string TURN_PERIOD_SETTING = "TURN_PERIOD";

    private static byte DEFAULT_VELOCITY = 120;
    private static int DEFAULT_SEOSOR_THRESHOLD = 500;
    private static int DEFAULT_BACK_PERIOD = 500;
    private static int DEFAULT_TURN_PERIOD = 500;

    private ResourceLoader loader = new ResourceLoader();

    private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    private bool existSettings;

    public MainPage () {
      this.InitializeComponent();

      Debug.WriteLine("Debug");

      if (existSettings = localSettings.Values[EXIST_SETTINGS] == null) {
        localSettings.Values[EXIST_SETTINGS] = true;
        localSettings.Values[VELOCITY_SETTING] = DEFAULT_VELOCITY;
        localSettings.Values[VELOCITY_INDIVIDUAL_SETTING] = false;
        localSettings.Values[VELOCITY_LEFT_SETTING] = DEFAULT_VELOCITY;
        localSettings.Values[VELOCITY_RIGHT_SETTING] = DEFAULT_VELOCITY;
        localSettings.Values[SENSOR_THRESHOLD_SETTING] = DEFAULT_SEOSOR_THRESHOLD;
        localSettings.Values[BACK_PERIOD_SETTING] = DEFAULT_BACK_PERIOD;
        localSettings.Values[TURN_PERIOD_SETTING] = DEFAULT_TURN_PERIOD;
      }
    }

    protected override void OnNavigatedTo (NavigationEventArgs e) {
      //とりま選択
      AutoRadioButton.IsChecked = true;
    }

    //自動探索(当分実装しない)
    private void AutoRadioButton_Checked (object sender, RoutedEventArgs e) {
      TitleTextBlock.Text = loader.GetString("AutoTitle");
      MainContentFrame.Navigate(typeof(AutoPage));
    }

    //コントローラ
    private void ManualRadioButton_Checked (object sender, RoutedEventArgs e) {
      TitleTextBlock.Text = loader.GetString("ManualTitle");
      MainContentFrame.Navigate(typeof(ManualPage));
    }

    //設定画面
    private void SettingsRadioButton_Checked (object sender, RoutedEventArgs e) {
      TitleTextBlock.Text = loader.GetString("SettingsTitle");
      MainContentFrame.Navigate(typeof(SettingsPage));
    }

    private void NavigationRadioButton_Click (object sender, RoutedEventArgs e) {
      SplitView.IsPaneOpen = false;
    }
  }
}
