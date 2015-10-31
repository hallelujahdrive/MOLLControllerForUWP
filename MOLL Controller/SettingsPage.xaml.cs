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

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace MOLL_Controller {
  /// <summary>
  /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
  /// </summary>
  public sealed partial class SettingsPage : Page {

    private const string EXIST_SETTINGS = "EXIST_SETTINGS";
    private const string VELOCITY_SETTING = "VELOCITY";
    private const string VELOCITY_INDIVIDUAL_SETTING = "VELOCITY_INDIVIDUAL";
    private const string VELOCITY_LEFT_SETTING = "VELOCITY_LEFT";
    private const string VELOCITY_RIGHT_SETTING = "VELOCITY_RIGHT";
    private const string SENSOR_THRESHOLD_SETTING = "SENSOR_THRESHOLD";
    private const string BACK_PERIOD_SETTING = "BACK_PERIOD";
    private const string TURN_PERIOD_SETTING = "TURN_PERIOD";

    private bool existSettings;
    private byte velocity;
    private bool velocityIndividual;
    private byte velocityL;
    private byte velocityR;
    private int sensorThreshold;
    private int backPeriod;
    private int turnPeriod;

    private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;


    public SettingsPage () {
      this.InitializeComponent();

      if (existSettings = localSettings.Values[EXIST_SETTINGS] != null) {

        if (existSettings) {

          velocity = (byte)localSettings.Values[VELOCITY_SETTING];

          if (!(velocityIndividual = (bool)localSettings.Values[VELOCITY_INDIVIDUAL_SETTING])) {
            velocityL = velocity;
            velocityR = velocity;
          } else {
            velocityL = (byte)localSettings.Values[VELOCITY_LEFT_SETTING];
            velocityR = (byte)localSettings.Values[VELOCITY_RIGHT_SETTING];
          }
          sensorThreshold = (int)localSettings.Values[SENSOR_THRESHOLD_SETTING];
          backPeriod = (int)localSettings.Values[BACK_PERIOD_SETTING];
          turnPeriod = (int)localSettings.Values[TURN_PERIOD_SETTING];
        }
      }
    }

    protected override void OnNavigatedTo (NavigationEventArgs e) {
      //とりま選択
      VelocitySlider.Value = velocity;
      VelocityIndividualCheckBox.IsChecked = velocityIndividual;
      VelocityLeftSlider.Value = velocityL;
      VelocityRightSlider.Value = velocityR;
      SensorThresholdtSlider.Value = sensorThreshold;
      BackPeripdSlider.Value = backPeriod;
      TurnPeriodSlider.Value = turnPeriod;
    }

    private void VelocitySlider_ValueChanged (object sender, RangeBaseValueChangedEventArgs e) {
      localSettings.Values[VELOCITY_SETTING] = BitConverter.GetBytes((int)VelocitySlider.Value)[0];
    }


    private void VelocityIndividualCheckBox_CheckedChanged (object sender, RoutedEventArgs e) {
      localSettings.Values[VELOCITY_INDIVIDUAL_SETTING] = VelocityIndividualCheckBox.IsChecked;
     }

    private void VelocityLeftSlider_ValueChanged (object sender, RangeBaseValueChangedEventArgs e) {
      localSettings.Values[VELOCITY_LEFT_SETTING] = BitConverter.GetBytes((int)VelocityLeftSlider.Value)[0];

    }

    private void VelocityRightSlider_ValueChanged (object sender, RangeBaseValueChangedEventArgs e) {
      localSettings.Values[VELOCITY_RIGHT_SETTING] = BitConverter.GetBytes((int)VelocityRightSlider.Value)[0];

    }

    private void SensorThresholdtSlider_ValueChanged (object sender, RangeBaseValueChangedEventArgs e) {
      localSettings.Values[SENSOR_THRESHOLD_SETTING] = (int)SensorThresholdtSlider.Value;

    }

    private void BackPeripdSlider_ValueChanged (object sender, RangeBaseValueChangedEventArgs e) {
      localSettings.Values[BACK_PERIOD_SETTING] = (int)BackPeripdSlider.Value;
    }

    private void TurnPeriodSlider_ValueChanged (object sender, RangeBaseValueChangedEventArgs e) {
      localSettings.Values[TURN_PERIOD_SETTING] = (int)TurnPeriodSlider.Value;
    }
  }
}
