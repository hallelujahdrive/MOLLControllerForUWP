using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

    private ResourceLoader loader = new ResourceLoader();
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    private DeviceInformation device;

    public ManualPage () {
      this.InitializeComponent();
    }

    protected override void OnNavigatedTo (NavigationEventArgs e) {
      DeviceNameTextBlock.Text = loader.GetString("Unsetting");
    }

    private async void SetDevice(DeviceInformation device) {
      this.device = device;
      DeviceNameTextBlock.Text = await device.GetDeviceName(cancellationTokenSource.Token);
      ConnectionStatusToggleSwitch.IsEnabled = true;
      ConnectionStatusToggleSwitch.IsOn = true;

    }

    //SettingDeviceDialogを開く
    private async void SettingDeviceButton_Click (object sender, RoutedEventArgs e) {

      var dialog = new SettingDeviceDialog();
      var result = await dialog.ShowAsync();
      if (result == ContentDialogResult.Primary && (device = dialog.GetDevice()) != null) {
        SetDevice(device);
      }
    }
  }
}
