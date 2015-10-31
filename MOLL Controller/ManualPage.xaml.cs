using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Gaming.Input;
using Windows.Storage;
using Windows.System;
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
  public sealed partial class ManualPage : Page {

    private static Guid RBL_SERVICE = new Guid("713D0000-503E-4C75-BA94-3148F18D941E");
    private static Guid TX_CHARACTERISTIC = new Guid("713D0003-503E-4C75-BA94-3148F18D941E");

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

    //命令タイプ
    private const byte SET_UP = 0;
    private const byte MOVE = 1;
    private const byte SET_LED = 2;

    //move
    private const int LEFT_P = 1;
    private const int LEFT_N = 2;
    private const int RIGHT_P = 3;
    private const int RIGHT_N = 4;

    //LED
    private const int LED_RED = 1;
    private const int LED_GREEN = 2;
    private const int LED_BLUE = 3;

    private const byte HIGH = 1;
    private const byte LOW = 0;

    //操作キー
    private const VirtualKey FORWARD_KEY = VirtualKey.W;
    private const VirtualKey BACK_KEY = VirtualKey.S;
    private const VirtualKey TURN_LEFT_KEY = VirtualKey.A;
    private const VirtualKey TURN_RIGHT_KEY = VirtualKey.D;
    private const VirtualKey LEFT_FORWARD_KEY = VirtualKey.Q;
    private const VirtualKey RIGHT_FORWARD_KEY = VirtualKey.E;
    private const VirtualKey LEFT_BACK_KEY = VirtualKey.Z;
    private const VirtualKey RIGHT_BACK_KEY = VirtualKey.C;

    //LED
    private const VirtualKey LED_RED_KEY = VirtualKey.Number1;
    private const VirtualKey LED_GREEN_KEY = VirtualKey.Number2;
    private const VirtualKey LED_BLUE_KEY = VirtualKey.Number3;
    private const VirtualKey LED_WHITE_KEY = VirtualKey.Number4;

    private VirtualKey currentKey;

    private byte velocity;
    private bool velocityIndividual;
    private byte velocityL;
    private byte velocityR;
    private int sensorThreshold;
    private int backPeriod;
    private int turnPeriod;

    private ResourceLoader loader = new ResourceLoader();

    private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private GattCharacteristic txCharacteristic;

    private List<VirtualKey> pressedKeyList = new List<VirtualKey>();

    public ManualPage () {
      this.InitializeComponent();

      bool existSettings = (bool)localSettings.Values[EXIST_SETTINGS];

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
      } else {
        velocityL = DEFAULT_VELOCITY;
        velocityR = DEFAULT_VELOCITY;
        sensorThreshold = DEFAULT_SEOSOR_THRESHOLD;
        backPeriod = DEFAULT_BACK_PERIOD;
        turnPeriod = DEFAULT_TURN_PERIOD;
      }
    }

    protected override void OnNavigatedTo (NavigationEventArgs e) {
      //Focusを設定
      this.Loaded += delegate { this.Focus(FocusState.Programmatic); };
      DeviceNameTextBlock.Text = loader.GetString("Unsetting");
    }

    private async void SetDevice (DeviceInformation device) {
      if (device != null) {
        DeviceNameTextBlock.Text = await device.GetDeviceName(cancellationTokenSource.Token);
        ConnectionStatusToggleSwitch.IsEnabled = true;
        ConnectionStatusToggleSwitch.IsOn = true;

        //Characteristicの取得
        var service = await device.GetOtherServiceAsync(RBL_SERVICE, cancellationTokenSource.Token);
        txCharacteristic = service.GetCharacteristics(TX_CHARACTERISTIC).First();

        //MollのSetup
        SetUpMoll();
      }
    }

    //SettingDeviceDialogを開く
    private async void SettingDeviceButton_Click (object sender, RoutedEventArgs e) {

      var dialog = new SettingDeviceDialog();
      var result = await dialog.ShowAsync();
      if (result == ContentDialogResult.Primary) {
        SetDevice((DeviceInformation)dialog.DataContext);
      }
    }

    private async void WriteCharacteristic (byte[] value) {
      try {
        var res = await txCharacteristic.WriteValueAsync(value.AsBuffer(), GattWriteOption.WriteWithoutResponse);
      } catch (System.Exception) {
      }
    }


    private void Grid_KeyUp (object sender, KeyRoutedEventArgs e) {

      if (txCharacteristic != null) {
        //リストの更新
        switch (e.Key) {
          case FORWARD_KEY:
          case BACK_KEY:
          case TURN_LEFT_KEY:
          case TURN_RIGHT_KEY:
          case LEFT_FORWARD_KEY:
          case RIGHT_FORWARD_KEY:
          case LEFT_BACK_KEY:
          case RIGHT_BACK_KEY:
            //一致をとる
            for(int i = 0; i < pressedKeyList.Count(); i++) {
              if (pressedKeyList[i].Equals(e.Key)) {
                //リストから削除
                pressedKeyList.RemoveAt(i);
                //currentKeyの更新
                currentKey = i > 0 ? pressedKeyList[i-1] : VirtualKey.None;
                SendCommand(currentKey);
                break;
              }
            }
            break;
          default:
            return;
        } 
      }
    }

    private void Grid_KeyDown (object sender, KeyRoutedEventArgs e) {

      if (txCharacteristic != null && currentKey != e.Key) {

        switch (e.Key) {
          case FORWARD_KEY:
          case BACK_KEY:
          case TURN_LEFT_KEY:
          case TURN_RIGHT_KEY:
          case LEFT_FORWARD_KEY:
          case RIGHT_FORWARD_KEY:
          case LEFT_BACK_KEY:
          case RIGHT_BACK_KEY:
            //currentKeyの更新
            currentKey = e.Key;
            //リストに追加
            pressedKeyList.Add(e.Key);
            break;
          default:
            //LEDの設定
            SetLed(e.Key);
            return;
        }

        //送信
        SendCommand(e.Key);
      }
      
    }

    private void SetUpMoll () {

      var value = new byte[15];

      //フラグ
      value[0] = SET_UP;
      //速度
      value[1] = velocityL;
      value[2] = velocityR;

      int i = 6;

      //リトルエンディアンな

      //赤外線線センサ
      foreach (byte data in BitConverter.GetBytes(sensorThreshold)) {
        value[i--] = data;
      }
      i = 10;
      //後退時間
      foreach (byte data in BitConverter.GetBytes(backPeriod)) {
        value[i--] = data;
      }
      i = 14;
      //転回時間
      foreach (byte data in BitConverter.GetBytes(turnPeriod)) {
        value[i--] = data;
      }

      //値の書き込み
      WriteCharacteristic(value);
    }

    //MOVEの送信
    private void SendCommand (VirtualKey key) {

      var value = new byte[5] { MOVE, 0, 0, 0, 0 };

      switch (key) {
        case FORWARD_KEY:
          value[LEFT_P] = velocityL;
          value[RIGHT_P] = velocityR;
          break;
        case BACK_KEY:
          value[LEFT_N] = velocityL;
          value[RIGHT_N] = velocityR;
          break;
        case TURN_LEFT_KEY:
          value[LEFT_N] = velocityL;
          value[RIGHT_P] = velocityR;
          break;
        case TURN_RIGHT_KEY:
          value[LEFT_P] = velocityL;
          value[RIGHT_N] = velocityR;
          break;
        case LEFT_FORWARD_KEY:
          value[RIGHT_P] = velocityR;
          break;
        case RIGHT_FORWARD_KEY:
          value[LEFT_P] = velocityL;
          break;
        case LEFT_BACK_KEY:
          value[RIGHT_N] = velocityR;
          break;
        case RIGHT_BACK_KEY:
          value[LEFT_N] = velocityL;
          break;
      }

      //送信
      WriteCharacteristic(value);
    }

    //LEDの設定
    private void SetLed (VirtualKey key) {

      var value = new byte[] { SET_LED, LOW, LOW, LOW };

      switch (key) {
        case LED_RED_KEY:
          value[LED_RED] = HIGH;
          break;
        case LED_GREEN_KEY:
          value[LED_GREEN] = HIGH;
          break;
        case LED_BLUE_KEY:
          value[LED_BLUE] = HIGH;
          break;
        case LED_WHITE_KEY:
          value[LED_RED] = HIGH;
          value[LED_GREEN] = HIGH;
          value[LED_BLUE] = HIGH;
          break;
        default:
          return;
      }

      //送信
      WriteCharacteristic(value);
    }
  }
}
