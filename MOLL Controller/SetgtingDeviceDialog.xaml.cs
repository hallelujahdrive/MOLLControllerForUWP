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

    public SettingDeviceDialog () {
      this.InitializeComponent();
      MessageTextBlock.Text = loader.GetString("SearchingDevices");

    }

    private async void ContentDialog_Opened (ContentDialog sender, ContentDialogOpenedEventArgs args) {

      var cancellationToken = cancellationTokenSource.Token;

      var filter = GattDeviceService.GetDeviceSelectorFromUuid(DeviceInformationServiceUuid);
      var devices = await DeviceInformation.FindAllAsync(filter, new[] { ContainerIdProperty }).AsTask(cancellationToken);
      if (devices.Count > 0) {
        foreach (var device in devices) {
          // Access to Generic Attribute Profile service
          var gapService = await GetOtherServiceAsync(device, GattServiceUuids.GenericAccess, cancellationToken);
          var deviceName = gapService.GetCharacteristics(GattDeviceService.ConvertShortIdToUuid(0x2a00)).First();
          var deviceNameValue = await deviceName.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask(cancellationToken);
          var decodedDeviceName = deviceNameValue.Value.DecodeUtf8String();
          deviceCollection.Add(decodedDeviceName);
        }
      }
    }

    static async Task<GattDeviceService> GetOtherServiceAsync (DeviceInformation serviceInformation, Guid serviceUuid, CancellationToken cancellationToken) {
      var containerId = serviceInformation.Properties[ContainerIdProperty].ToString();
      var selector = GattDeviceService.GetDeviceSelectorFromUuid(serviceUuid);
      var selectorWithContainer = String.Format("{0} AND System.Devices.ContainerId:=\"{{{1}}}\"", selector, containerId);
      var serviceInformations = await DeviceInformation.FindAllAsync(selectorWithContainer, new[] { ContainerIdProperty }).AsTask(cancellationToken);
      return await GattDeviceService.FromIdAsync(serviceInformations.Single().Id);
    }

    private void ContentDialog_Closed (ContentDialog sender, ContentDialogClosedEventArgs args) {

    }

    private void ContentDialog_PrimaryButtonClick (ContentDialog sender, ContentDialogButtonClickEventArgs args) {

    }

    private void ContentDialog_SecondaryButtonClick (ContentDialog sender, ContentDialogButtonClickEventArgs args) {

    }
  }
}
