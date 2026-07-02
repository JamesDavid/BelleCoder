using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace startechplus.ble
{
	public class iOSBleBridge : IBleBridge
	{
		[DllImport("__Internal")]
		private static extern void iOSBleBridgeStartup(string gameObjName, bool isCentral);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeShutdown();

		[DllImport("__Internal")]
		private static extern void iOSBleBridgePauseWithState(bool isPaused);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeConnectToPeripheralWithIdentifier(string peripheralId);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeDisconnectPeripheralWithIdentifier(string peripheralId);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeScanForPeripheralsWithServiceUUIDs(string uuids);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs(string uuids);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs(string uuids);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeStopScanning();

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeWriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeSubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeWriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length);

		[DllImport("__Internal")]
		private static extern void iOSBleBridgeReadRssiWithIdentifier(string peripheralId);

		public BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
		{
			iOSBleBridge.bluetoothDevice = null;
			if (GameObject.Find("BleBridge") == null)
			{
				GameObject gameObject = new GameObject("BleBridge");
				iOSBleBridge.bluetoothDevice = gameObject.AddComponent<BluetoothLeDevice>();
				if (iOSBleBridge.bluetoothDevice != null)
				{
					iOSBleBridge.bluetoothDevice.StartupAction = action;
					iOSBleBridge.bluetoothDevice.ErrorAction = errorAction;
					iOSBleBridge.bluetoothDevice.StateUpdateAction = stateUpdateAction;
					iOSBleBridge.bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
				}
			}
			iOSBleBridge.iOSBleBridgeStartup("BleBridge", asCentral);
			return iOSBleBridge.bluetoothDevice;
		}

		public void Shutdown(Action action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.ShutdownAction = action;
			}
			iOSBleBridge.iOSBleBridgeShutdown();
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
			iOSBleBridge.iOSBleBridgePauseWithState(isPaused);
		}

		public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DiscoveredPeripheralAction = action;
			}
			string text = null;
			if (serviceUUIDs != null)
			{
				text = string.Empty;
				foreach (string text2 in serviceUUIDs)
				{
					text = text + text2 + "|";
				}
				text = text.Substring(0, text.Length - 1);
			}
			iOSBleBridge.iOSBleBridgeScanForPeripheralsWithServiceUUIDs(text);
		}

		public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string> disconnectAction)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.ConnectedPeripheralAction = connectAction;
				iOSBleBridge.bluetoothDevice.DiscoveredServiceAction = serviceAction;
				iOSBleBridge.bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
				iOSBleBridge.bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
				iOSBleBridge.bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;
			}
			iOSBleBridge.iOSBleBridgeConnectToPeripheralWithIdentifier(peripheralId);
		}

		public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DisconnectedPeripheralAction = action;
			}
			iOSBleBridge.iOSBleBridgeDisconnectPeripheralWithIdentifier(peripheralId);
		}

		public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
			}
			string text = null;
			if (serviceUUIDs != null)
			{
				text = ((serviceUUIDs.Length <= 0) ? null : string.Empty);
				foreach (string text2 in serviceUUIDs)
				{
					text = text + text2 + "|";
				}
				text = text.Substring(0, text.Length - 1);
			}
			iOSBleBridge.iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs(text);
		}

		public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;
			}
			string text = null;
			if (uuids != null)
			{
				text = ((uuids.Length <= 0) ? null : string.Empty);
				foreach (string text2 in uuids)
				{
					text = text + text2 + "|";
				}
				text = text.Substring(0, text.Length - 1);
			}
			iOSBleBridge.iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs(text);
		}

		public void StopScanning()
		{
			iOSBleBridge.iOSBleBridgeStopScanning();
		}

		public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
				iOSBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			iOSBleBridge.iOSBleBridgeSubscribeToCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
		}

		public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action)
		{
			iOSBleBridge.iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
		}

		public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			iOSBleBridge.iOSBleBridgeReadCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
		}

		public void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DidWriteCharacteristicAction = action;
			}
			iOSBleBridge.iOSBleBridgeWriteCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId, data, length, withResponse);
		}

		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DidReadDescriptorValueAction = action;
			}
			iOSBleBridge.iOSBleBridgeReadDescriptorWithIdentifiers(peripheralId, serviceId, characteristicId, descriptorId);
		}

		public void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DidWriteDescriptorAction = action;
			}
			iOSBleBridge.iOSBleBridgeWriteDescriptorWithIdentifiers(peripheralId, serviceId, characteristicId, descriptorId, data, length);
		}

		public void ReadRssiWithIdentifier(string peripheralId)
		{
			iOSBleBridge.iOSBleBridgeReadRssiWithIdentifier(peripheralId);
		}

		public void AddAdvertisementDataListeners(Action<string, string> localNameAction, Action<string, byte[]> manufactureDataAction, Action<string, string, byte[]> serviceDataAction, Action<string, string> serviceAction, Action<string, string> overflowServiceAction, Action<string, string> txPowerLevelAction, Action<string, string> isConnectable, Action<string, string> solicitedServiceAction)
		{
			if (iOSBleBridge.bluetoothDevice != null)
			{
				iOSBleBridge.bluetoothDevice.DidAdvertiseLocalNameAction = localNameAction;
				iOSBleBridge.bluetoothDevice.DidAdvertiseManufactureDataAction = manufactureDataAction;
				iOSBleBridge.bluetoothDevice.DidAdvertiseServiceDataAction = serviceDataAction;
				iOSBleBridge.bluetoothDevice.DidAdvertiseServiceAction = serviceAction;
				iOSBleBridge.bluetoothDevice.DidAdvertiseOverflowServiceAction = overflowServiceAction;
				iOSBleBridge.bluetoothDevice.DidAdvertiseTxPowerLevelAction = txPowerLevelAction;
				iOSBleBridge.bluetoothDevice.DidAdvertiseIsConnectable = isConnectable;
				iOSBleBridge.bluetoothDevice.DidAdvertiseSolicitedServiceAction = solicitedServiceAction;
			}
		}

		private static BluetoothLeDevice bluetoothDevice;
	}
}
