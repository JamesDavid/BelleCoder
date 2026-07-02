using System;
using UnityEngine;

namespace startechplus.ble
{
	public class DummyBleBridge : IBleBridge
	{
		public BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
		{
			DummyBleBridge.bluetoothDevice = null;
			if (GameObject.Find("BleBridge") == null)
			{
				GameObject gameObject = new GameObject("BleBridge");
				DummyBleBridge.bluetoothDevice = gameObject.AddComponent<BluetoothLeDevice>();
				if (DummyBleBridge.bluetoothDevice != null)
				{
					DummyBleBridge.bluetoothDevice.StartupAction = action;
					DummyBleBridge.bluetoothDevice.ErrorAction = errorAction;
					DummyBleBridge.bluetoothDevice.StateUpdateAction = stateUpdateAction;
					DummyBleBridge.bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
				}
			}
			DummyBleBridge.bluetoothDevice.OnStartup("Startup");
			DummyBleBridge.bluetoothDevice.OnBleStateUpdate("Powered On");
			return DummyBleBridge.bluetoothDevice;
		}

		public void Shutdown(Action action)
		{
			if (DummyBleBridge.bluetoothDevice != null)
			{
				DummyBleBridge.bluetoothDevice.ShutdownAction = action;
			}
			DummyBleBridge.bluetoothDevice.OnStartup("Shutdown");
		}

		public void Cleanup()
		{
			GameObject gameObject = GameObject.Find("BleBridge");
			if (gameObject != null)
			{
				global::UnityEngine.Object.Destroy(gameObject);
			}
		}

		public void PauseWithState(bool isPaused)
		{
		}

		public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			DummyBleBridge.bluetoothDevice.DiscoveredPeripheralAction = action;
			DummyBleBridge.bluetoothDevice.OnDiscoveredPeripheral("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6617:Star Technologies");
			DummyBleBridge.bluetoothDevice.OnRssiUpdate("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a662:94");
		}

		public void StopScanning()
		{
		}

		public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string> disconnectAction)
		{
			DummyBleBridge.bluetoothDevice.ConnectedPeripheralAction = connectAction;
			DummyBleBridge.bluetoothDevice.DiscoveredServiceAction = serviceAction;
			DummyBleBridge.bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
			DummyBleBridge.bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
			DummyBleBridge.bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;
			DummyBleBridge.bluetoothDevice.OnConnectedPeripheral("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6617:Star Technologies");
			DummyBleBridge.bluetoothDevice.OnDiscoveredService("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:6be6bc00-5c9a-11e4-8ed6-0800200c9a66");
			DummyBleBridge.bluetoothDevice.OnDiscoveredCharacteristic("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:43ECE40F-412E-4F68-9062-3B7C4DED1580");
			DummyBleBridge.bluetoothDevice.OnDiscoveredCharacteristic("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:45A634DC-B675-4EC2-A1F9-8FDAFF8D17F5");
			DummyBleBridge.bluetoothDevice.OnDiscoveredCharacteristic("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a6636:3E9883BD-A699-4ECC-88B8-28DE32292DD8");
		}

		public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action)
		{
		}

		public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			DummyBleBridge.bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
			DummyBleBridge.bluetoothDevice.OnRetrievedPeripheralWithServiceUUIDs("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a674:Acme");
		}

		public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action)
		{
			DummyBleBridge.bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;
			DummyBleBridge.bluetoothDevice.OnRetrievedPeripheralWithUUID("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a684:Acme");
		}

		public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
		{
			if (DummyBleBridge.bluetoothDevice != null)
			{
				DummyBleBridge.bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
				DummyBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
		}

		public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action)
		{
		}

		public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
		{
			if (DummyBleBridge.bluetoothDevice != null)
			{
				DummyBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
		}

		public void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action)
		{
			if (DummyBleBridge.bluetoothDevice != null)
			{
				DummyBleBridge.bluetoothDevice.DidWriteCharacteristicAction = action;
			}
			byte[] array = new byte[5];
			this.lastOn = !this.lastOn;
			array[0] = 128;
			array[1] = 8;
			array[2] = ((!this.lastOn) ? 0 : byte.MaxValue);
			string text = Convert.ToBase64String(array);
			DummyBleBridge.bluetoothDevice.OnDidWriteCharacteristic("36:" + peripheralId + "36:" + characteristicId);
			DummyBleBridge.bluetoothDevice.OnBluetoothData(string.Concat(new object[] { "36:", peripheralId, "36:3E9883BD-A699-4ECC-88B8-28DE32292DD8", text.Length, ":", text }));
		}

		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
		{
		}

		public void WriteDescriptorWithIdentifiers(string identifier, string service, string characteristic, string descriptor, byte[] data, int length, Action<string, string, string, string> action)
		{
		}

		public void ReadRssiWithIdentifier(string peripheralId)
		{
			DummyBleBridge.bluetoothDevice.OnRssiUpdate("36:fc9cbe80-5c99-11e4-8ed6-0800200c9a662:94");
		}

		public void AddAdvertisementDataListeners(Action<string, string> localNameAction, Action<string, byte[]> manufactureDataAction, Action<string, string, byte[]> serviceDataAction, Action<string, string> serviceAction, Action<string, string> overflowServiceAction, Action<string, string> txPowerLevelAction, Action<string, string> isConnectable, Action<string, string> solicitedServiceAction)
		{
		}

		private static BluetoothLeDevice bluetoothDevice;

		private bool lastOn;
	}
}
