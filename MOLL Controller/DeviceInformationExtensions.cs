using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;

namespace MOLL_Controller {
  static class DeviceInformationExtensions {

    private const string ContainerIdProperty = "System.Devices.ContainerId";

    public static async Task<string> GetDeviceName (this DeviceInformation device, CancellationToken cancellationToken) {
      var gapService = await GetOtherServiceAsync(device, GattServiceUuids.GenericAccess, cancellationToken);
      var deviceName = gapService.GetCharacteristics(GattDeviceService.ConvertShortIdToUuid(0x2a00)).First();
      var deviceNameValue = await deviceName.ReadValueAsync(BluetoothCacheMode.Uncached).AsTask(cancellationToken);
      return deviceNameValue.Value.DecodeUtf8String();
    }

    private static async Task<GattDeviceService> GetOtherServiceAsync (DeviceInformation serviceInformation, Guid serviceUuid, CancellationToken cancellationToken) {
      var containerId = serviceInformation.Properties[ContainerIdProperty].ToString();
      var selector = GattDeviceService.GetDeviceSelectorFromUuid(serviceUuid);
      var selectorWithContainer = String.Format("{0} AND System.Devices.ContainerId:=\"{{{1}}}\"", selector, containerId);
      var serviceInformations = await DeviceInformation.FindAllAsync(selectorWithContainer, new[] { ContainerIdProperty }).AsTask(cancellationToken);
      return await GattDeviceService.FromIdAsync(serviceInformations.Single().Id);
    }
  }
}
