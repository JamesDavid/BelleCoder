using System;
using UnityEngine;

namespace startechplus.ble
{
	public class AndroidBleBridge : IBleBridge
	{
		public BluetoothLeDevice Startup(bool asCentral, Action action, Action<string> errorAction, Action<string> stateUpdateAction, Action<string, string> rssiUpdateAction)
		{
			AndroidBleBridge.bluetoothDevice = null;
			if (GameObject.Find("BleBridge") == null)
			{
				GameObject gameObject = new GameObject("BleBridge");
				AndroidBleBridge.bluetoothDevice = gameObject.AddComponent<BluetoothLeDevice>();
				if (AndroidBleBridge.bluetoothDevice != null)
				{
					AndroidBleBridge.bluetoothDevice.isLowerCaseUUID = true;
					AndroidBleBridge.bluetoothDevice.StartupAction = action;
					AndroidBleBridge.bluetoothDevice.ErrorAction = errorAction;
					AndroidBleBridge.bluetoothDevice.StateUpdateAction = stateUpdateAction;
					AndroidBleBridge.bluetoothDevice.DidUpdateRssiAction = rssiUpdateAction;
				}
			}
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				this.playerActivityContext = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			}
			using (AndroidJavaClass androidJavaClass2 = new AndroidJavaClass("com.startechplus.unityblebridge.Bridge"))
			{
				if (androidJavaClass2 != null)
				{
					this.bridge = androidJavaClass2.CallStatic<AndroidJavaObject>("instance", new object[0]);
					this.bridge.Call("setContext", new object[] { this.playerActivityContext });
					this.bridge.Call("startup", new object[]
					{
						AndroidBleBridge.bluetoothDevice.gameObject.name,
						asCentral
					});
				}
				else
				{
					Debug.Log("AndroidBleBridge: Error creating Android objects...");
				}
			}
			return AndroidBleBridge.bluetoothDevice;
		}

		public void Shutdown(Action action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.ShutdownAction = action;
			}
			this.bridge.Call("shutdown", new object[0]);
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
			this.bridge.Call("pauseWithState", new object[] { isPaused });
		}

		public void ScanForPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			Debug.Log("AndroidBleBridge : ScanForPeripheralsWithServiceUUIDs : 0");
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				Debug.Log("AndroidBleBridge : ScanForPeripheralsWithServiceUUIDs : 1");
				AndroidBleBridge.bluetoothDevice.DiscoveredPeripheralAction = action;
			}
			string text = null;
			if (serviceUUIDs != null)
			{
				Debug.Log("AndroidBleBridge : ScanForPeripheralsWithServiceUUIDs : 2");
				text = string.Empty;
				foreach (string text2 in serviceUUIDs)
				{
					text = text + text2.ToLower() + "|";
				}
				text = text.Substring(0, text.Length - 1);
			}
			this.bridge.Call("scanForPeripheralsWithServiceUUIDs", new object[] { text });
		}

		public void ConnectToPeripheralWithIdentifier(string peripheralId, Action<string, string> connectAction, Action<string, string> serviceAction, Action<string, string, string> characteristicAction, Action<string, string, string, string> descriptorAction, Action<string, string> disconnectAction)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.ConnectedPeripheralAction = connectAction;
				AndroidBleBridge.bluetoothDevice.DiscoveredServiceAction = serviceAction;
				AndroidBleBridge.bluetoothDevice.DiscoveredCharacteristicAction = characteristicAction;
				AndroidBleBridge.bluetoothDevice.DiscoveredDescriptorAction = descriptorAction;
				AndroidBleBridge.bluetoothDevice.DisconnectedPeripheralAction = disconnectAction;
			}
			this.bridge.Call("connectToPeripheralWithIdentifier", new object[] { peripheralId });
		}

		public void DisconnectFromPeripheralWithIdentifier(string peripheralId, Action<string, string> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DisconnectedPeripheralAction = action;
			}
			this.bridge.Call("disconnectFromPeripheralWithIdentifier", new object[] { peripheralId });
		}

		public void RetrieveListOfPeripheralsWithServiceUUIDs(string[] serviceUUIDs, Action<string, string> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.RetrievedPeripheralWithServiceAction = action;
			}
			string text = null;
			if (serviceUUIDs != null)
			{
				text = ((serviceUUIDs.Length <= 0) ? null : string.Empty);
				foreach (string text2 in serviceUUIDs)
				{
					text = text + text2.ToLower() + "|";
				}
				text = text.Substring(0, text.Length - 1);
			}
			this.bridge.Call("retrieveListOfPeripheralsWithServiceUUIDs", new object[] { text });
		}

		public void RetrieveListOfPeripheralsWithUUIDs(string[] uuids, Action<string, string> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.RetrievedPeripheralWithUUIDAction = action;
			}
			string text = null;
			if (uuids != null)
			{
				text = ((uuids.Length <= 0) ? null : string.Empty);
				foreach (string text2 in uuids)
				{
					text = text + text2.ToLower() + "|";
				}
				text = text.Substring(0, text.Length - 1);
			}
			this.bridge.Call("retrieveListOfPeripheralsWithUUIDs", new object[] { text });
		}

		public void StopScanning()
		{
			this.bridge.Call("stopScanning", new object[0]);
		}

		public void SubscribeToCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action, bool isIndication)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DidUpdateNotificationStateForCharacteristicAction = notificationAction;
				AndroidBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			this.bridge.Call("subscribeToCharacteristicWithIdentifiers", new object[]
			{
				peripheralId,
				serviceId.ToLower(),
				characteristicId.ToLower(),
				isIndication
			});
		}

		public void UnSubscribeFromCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string> action)
		{
			this.bridge.Call("unSubscribeFromCharacteristicWithIdentifiers", new object[]
			{
				peripheralId,
				serviceId.ToLower(),
				characteristicId.ToLower()
			});
		}

		public void ReadCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, Action<string, string, string, byte[]> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DidUpdateCharacteristicValueAction = action;
			}
			this.bridge.Call("readCharacteristicWithIdentifiers", new object[]
			{
				peripheralId,
				serviceId.ToLower(),
				characteristicId.ToLower()
			});
		}

		public void WriteCharacteristicWithIdentifiers(string peripheralId, string serviceId, string characteristicId, byte[] data, int length, bool withResponse, Action<string, string, string> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DidWriteCharacteristicAction = action;
			}
			this.bridge.Call("writeCharacteristicWithIdentifiers", new object[]
			{
				peripheralId,
				serviceId.ToLower(),
				characteristicId.ToLower(),
				data,
				length,
				withResponse
			});
		}

		public void ReadDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, Action<string, string, string, string, byte[]> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DidReadDescriptorValueAction = action;
			}
			this.bridge.Call("readDescriptorWithIdentifiers", new object[]
			{
				peripheralId,
				serviceId.ToLower(),
				characteristicId.ToLower(),
				descriptorId.ToLower()
			});
		}

		public void WriteDescriptorWithIdentifiers(string peripheralId, string serviceId, string characteristicId, string descriptorId, byte[] data, int length, Action<string, string, string, string> action)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DidWriteDescriptorAction = action;
			}
			this.bridge.Call("writeDescriptorWithIdentifiers", new object[]
			{
				peripheralId,
				serviceId.ToLower(),
				characteristicId.ToLower(),
				descriptorId.ToLower(),
				data,
				length
			});
		}

		public void ReadRssiWithIdentifier(string peripheralId)
		{
			this.bridge.Call("readRssiWithIdentifier", new object[] { peripheralId });
		}

		public void AddAdvertisementDataListeners(Action<string, string> localNameAction, Action<string, byte[]> manufactureDataAction, Action<string, string, byte[]> serviceDataAction, Action<string, string> serviceAction, Action<string, string> overflowServiceAction, Action<string, string> txPowerLevelAction, Action<string, string> isConnectable, Action<string, string> solicitedServiceAction)
		{
			if (AndroidBleBridge.bluetoothDevice != null)
			{
				AndroidBleBridge.bluetoothDevice.DidAdvertiseLocalNameAction = localNameAction;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseManufactureDataAction = manufactureDataAction;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseServiceDataAction = serviceDataAction;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseServiceAction = serviceAction;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseOverflowServiceAction = overflowServiceAction;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseTxPowerLevelAction = txPowerLevelAction;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseIsConnectable = isConnectable;
				AndroidBleBridge.bluetoothDevice.DidAdvertiseSolicitedServiceAction = solicitedServiceAction;
			}
		}

		private AndroidJavaObject bridge;

		private AndroidJavaObject playerActivityContext;

		private static BluetoothLeDevice bluetoothDevice;
	}
}
