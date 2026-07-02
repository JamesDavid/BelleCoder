using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace startechplus.ble
{
	public class OsxBleBridge : IBleBridge
	{
		[DllImport("uBluetoothLeOsx")]
		private static extern void ConnectUnitySendMessageCallback([MarshalAs(UnmanagedType.FunctionPtr)] OsxBleBridge.UnitySendMessageCallbackDelegate callbackMethod);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeStartup(string gameObjName, bool isCentral);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeShutdown();

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgePauseWithState(bool isPaused);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeConnectToPeripheralWithIdentifier(string peripheralId);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeDisconnectPeripheralWithIdentifier(string peripheralId);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeScanForPeripheralsWithServiceUUIDs(string uuids);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs(string uuids);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs(string uuids);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeStopScanning();

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeWriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeSubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeWriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length);

		[DllImport("uBluetoothLeOsx")]
		private static extern void iOSBleBridgeReadRssiWithIdentifier(string peripheralId);

		public BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
		{
			OsxBleBridge.bluetoothDevice = null;
			if (GameObject.Find("BleBridge") == null)
			{
				GameObject gameObject = new GameObject("BleBridge");
				OsxBleBridge.bluetoothDevice = gameObject.AddComponent<BluetoothLeDevice>();
				if (OsxBleBridge.bluetoothDevice != null)
				{
					OsxBleBridge.bluetoothDevice.StartupAction = action;
					OsxBleBridge.bluetoothDevice.ErrorAction = errorAction;
					OsxBleBridge.bluetoothDevice.StateUpdateAction = stateUpdateAction;
					OsxBleBridge.bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
				}
				OsxBleBridge.ConnectUnitySendMessageCallback(delegate(IntPtr _objectName, IntPtr _commandName, IntPtr _commandData)
				{
					string text = Marshal.PtrToStringAuto(_objectName);
					string text2 = Marshal.PtrToStringAuto(_commandName);
					string text3 = Marshal.PtrToStringAuto(_commandData);
					GameObject gameObject2 = GameObject.Find(text);
					if (gameObject2 != null)
					{
						gameObject2.SendMessage(text2, text3);
					}
				});
			}
			OsxBleBridge.iOSBleBridgeStartup("BleBridge", asCentral);
			return OsxBleBridge.bluetoothDevice;
		}

		public void Shutdown(Action action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.ShutdownAction = action;
			}
			OsxBleBridge.iOSBleBridgeShutdown();
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
			OsxBleBridge.iOSBleBridgePauseWithState(isPaused);
		}

		public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DiscoveredPeripheralAction = action;
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
			OsxBleBridge.iOSBleBridgeScanForPeripheralsWithServiceUUIDs(text);
		}

		public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string> disconnectAction)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.ConnectedPeripheralAction = connectAction;
				OsxBleBridge.bluetoothDevice.DiscoveredServiceAction = serviceAction;
				OsxBleBridge.bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
				OsxBleBridge.bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
				OsxBleBridge.bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;
			}
			OsxBleBridge.iOSBleBridgeConnectToPeripheralWithIdentifier(peripheralId);
		}

		public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DisconnectedPeripheralAction = action;
			}
			OsxBleBridge.iOSBleBridgeDisconnectPeripheralWithIdentifier(peripheralId);
		}

		public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
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
			OsxBleBridge.iOSBleBridgeRetrieveListOfPeripheralsWithServiceUUIDs(text);
		}

		public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;
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
			OsxBleBridge.iOSBleBridgeRetrieveListOfPeripheralsWithUUIDs(text);
		}

		public void StopScanning()
		{
			OsxBleBridge.iOSBleBridgeStopScanning();
		}

		public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
				OsxBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			OsxBleBridge.iOSBleBridgeSubscribeToCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
		}

		public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action)
		{
			OsxBleBridge.iOSBleBridgeUnSubscribeFromCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
		}

		public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			OsxBleBridge.iOSBleBridgeReadCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId);
		}

		public void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DidWriteCharacteristicAction = action;
			}
			OsxBleBridge.iOSBleBridgeWriteCharacteristicWithIdentifiers(peripheralId, serviceId, characteristicId, data, length, withResponse);
		}

		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DidReadDescriptorValueAction = action;
			}
			OsxBleBridge.iOSBleBridgeReadDescriptorWithIdentifiers(peripheralId, serviceId, characteristicId, descriptorId);
		}

		public void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DidWriteDescriptorAction = action;
			}
			OsxBleBridge.iOSBleBridgeWriteDescriptorWithIdentifiers(peripheralId, serviceId, characteristicId, descriptorId, data, length);
		}

		public void ReadRssiWithIdentifier(string peripheralId)
		{
			OsxBleBridge.iOSBleBridgeReadRssiWithIdentifier(peripheralId);
		}

		public void AddAdvertisementDataListeners(Action<string, string> localNameAction, Action<string, byte[]> manufactureDataAction, Action<string, string, byte[]> serviceDataAction, Action<string, string> serviceAction, Action<string, string> overflowServiceAction, Action<string, string> txPowerLevelAction, Action<string, string> isConnectable, Action<string, string> solicitedServiceAction)
		{
			if (OsxBleBridge.bluetoothDevice != null)
			{
				OsxBleBridge.bluetoothDevice.DidAdvertiseLocalNameAction = localNameAction;
				OsxBleBridge.bluetoothDevice.DidAdvertiseManufactureDataAction = manufactureDataAction;
				OsxBleBridge.bluetoothDevice.DidAdvertiseServiceDataAction = serviceDataAction;
				OsxBleBridge.bluetoothDevice.DidAdvertiseServiceAction = serviceAction;
				OsxBleBridge.bluetoothDevice.DidAdvertiseOverflowServiceAction = overflowServiceAction;
				OsxBleBridge.bluetoothDevice.DidAdvertiseTxPowerLevelAction = txPowerLevelAction;
				OsxBleBridge.bluetoothDevice.DidAdvertiseIsConnectable = isConnectable;
				OsxBleBridge.bluetoothDevice.DidAdvertiseSolicitedServiceAction = solicitedServiceAction;
			}
		}

		private static BluetoothLeDevice bluetoothDevice;

		public delegate void UnitySendMessageCallbackDelegate(IntPtr objectName, IntPtr commandName, IntPtr commandData);
	}
}
