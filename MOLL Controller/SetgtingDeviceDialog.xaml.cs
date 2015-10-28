using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
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

//コンテンツ ダイアログ項目テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください。

namespace MOLL_Controller {
  public sealed partial class SettingDeviceDialog : ContentDialog {

    private static readonly Guid DeviceInformationServiceUuid = GattDeviceService.ConvertShortIdToUuid(0x180a);
    private static readonly Guid KeyInputServiceUuid = GattDeviceService.ConvertShortIdToUuid(0xffe0);
    private static readonly Guid KeyInputCharacteristicUuid = GattDeviceService.ConvertShortIdToUuid(0xffe1);

    private const string ContainerIdProperty = "System.Devices.ContainerId";

    private ResourceLoader loader = new ResourceLoader();
    private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(); 

    private ObservableCollection<String> deviceCollection = new ObservableCollection<String>();
    private List<DeviceInformation> deviceList = new List<DeviceInformation>();

    private int selectedIndex;

    /*public static readonly DependencyProperty dp = DependencyProperty.Register(
      "Device",
      typeof(DeviceInformation),
      typeof(SettingDeviceDialog),
      null
      );

    public DeviceInformation Device {
      get { return (DeviceInformation)GetValue(dp); }
      set { SetValue(dp, (DeviceInformation)value); }
    }*/

    public SettingDeviceDialog () {
      this.InitializeComponent();
      MessageTextBlock.Text = loader.GetString("SearchingDevices");

    }

    private async void ContentDialog_Opened (ContentDialog sender, ContentDialogOpenedEventArgs args) {

      var cancellationToken = cancellationTokenSource.Token;

      var filter = GattDeviceService.GetDeviceSelectorFromUuid(DeviceInformationServiceUuid);
      var devices = await DeviceInformation.FindAllAsync(filter, new[] { ContainerIdProperty });
      if (devices.Count > 0) {
        foreach (var device in devices) {
          try {
            deviceList.Add(device);
            var decodedDeviceName = await device.GetDeviceName(cancellationToken);
            deviceCollection.Add(decodedDeviceName);
          } catch (NullReferenceException) {
            //デバイス見つからんかったら多分こっち来るねんな
            MessageTextBlock.Text = loader.GetString("NotFoundDevices");
            CollapseStoryboard.Begin();
          }
        }
      }
    }

    private void ContentDialog_Closed (ContentDialog sender, ContentDialogClosedEventArgs args) {
      cancellationTokenSource.Cancel();
    }

    private void ContentDialog_PrimaryButtonClick (ContentDialog sender, ContentDialogButtonClickEventArgs args) {
      selectedIndex =  DevicesListBox.SelectedIndex;
    }

    private void DevicesListBox_SelectionChanged (object sender, SelectionChangedEventArgs e) {
      IsPrimaryButtonEnabled = true;
    }

    public DeviceInformation GetDevice () {
      return deviceList[selectedIndex];
    }

  }
}
