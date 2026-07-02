using System;
using UnityEngine;
using UnityEngine.UI;

namespace startechplus.ble
{
	public class BluetoothHandler : MonoBehaviour
	{
		private void Awake()
		{
			if (this.bleToyHandlerGameObject != null)
			{
				this.bleToyHandler = this.bleToyHandlerGameObject.GetComponent<IBLEToyHandler>();
			}
		}

		public void Setup(bool _debug, GameObject _bleToyHandlerGameObject)
		{
			this.isDebug = _debug;
			this.bleToyHandler = _bleToyHandlerGameObject.GetComponent<IBLEToyHandler>();
			Debug.Log("bleToyHandler: " + this.bleToyHandler);
		}

		private void OnShutdown()
		{
			this.updateLog("Bridge: DLL resources released...");
		}

		private void OnApplicationQuit()
		{
			this.updateLog("Bridge: OnApplicationQuit()");
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				this.updateLog("Bridge: Unloading DLL resources...");
				this.bleBridge.Shutdown(new Action(this.OnShutdown));
			}
		}

		public void AdvertiseLocalNameAction(string peripherialID, string localName)
		{
			this.updateLog("Bridge: AdvertiseLocalNameAction,  " + peripherialID + ", " + localName);
		}

		public void AdvertiseManufactureDataAction(string peripherialID, byte[] data)
		{
			this.updateLog("Bridge: AdvertiseManufactureDataAction, " + peripherialID + ", " + BitConverter.ToString(data));
		}

		public void AdvertiseServiceDataAction(string peripherialID, string serviceID, byte[] data)
		{
			this.updateLog(string.Concat(new string[]
			{
				"Bridge: AdvertiseServiceDataAction, ",
				peripherialID,
				", ",
				serviceID,
				", ",
				BitConverter.ToString(data)
			}));
		}

		public void AdvertiseServiceAction(string peripherialID, string serviceID)
		{
			this.updateLog("Bridge: AdvertiseServiceAction, " + peripherialID + ", " + serviceID);
		}

		public void AdvertiseOverflowServiceAction(string peripherialID, string serviceID)
		{
			this.updateLog("Bridge: AdvertiseOverflowServiceAction, " + peripherialID + ", " + serviceID);
		}

		public void AdvertiseTxPowerLevelActionAction(string peripherialID, string txPowerLevel)
		{
			this.updateLog("Bridge: AdvertiseTxPowerLevelActionAction, " + peripherialID + ", " + txPowerLevel);
		}

		public void AdvertiseIsConnectableAction(string peripherialID, string isConnectable)
		{
			this.updateLog("Bridge: AdvertiseIsConnectableAction, " + peripherialID + ", " + isConnectable);
		}

		public void AdvertiseSolicitedServiceAction(string peripherialID, string solicitedServiceID)
		{
			this.updateLog("Bridge: AdvertiseSolicitedServiceAction, " + peripherialID + ", " + solicitedServiceID);
		}

		public void SubscribeToCharacteristic(string peripheral, string service, string characteristic, Action<string, string, string> notificationAction, Action<string, string, string, byte[]> action)
		{
			this.bleBridge.SubscribeToCharacteristicWithIdentifiers(peripheral, service, characteristic, notificationAction, action, false);
		}

		public void UnSubscribeToCharacteristic(string peripheral, string service, string characteristic, Action<string, string, string> notificationAction)
		{
			this.bleBridge.UnSubscribeFromCharacteristicWithIdentifiers(peripheral, service, characteristic, notificationAction);
		}

		public void SendCharacteristicData(string peripheral, string service, string characteristic, byte[] data, int length)
		{
			this.bleBridge.WriteCharacteristicWithIdentifiers(peripheral, service, characteristic, data, length, false, new Action<string, string, string>(this.DidWriteCharacteristicAction));
		}

		public void Scan()
		{
			this.updateLog("Applicaton: Scanning for ble devices...");
			this.bleBridge.AddAdvertisementDataListeners(new Action<string, string>(this.AdvertiseLocalNameAction), new Action<string, byte[]>(this.AdvertiseManufactureDataAction), new Action<string, string, byte[]>(this.AdvertiseServiceDataAction), new Action<string, string>(this.AdvertiseServiceAction), new Action<string, string>(this.AdvertiseOverflowServiceAction), new Action<string, string>(this.AdvertiseTxPowerLevelActionAction), new Action<string, string>(this.AdvertiseIsConnectableAction), new Action<string, string>(this.AdvertiseSolicitedServiceAction));
			this.bleBridge.ScanForPeripheralsWithServiceUUIDs(null, new Action<string, string>(this.DiscoveredPeripheralAction));
		}

		public void StopScanning()
		{
			this.updateLog("Applicaton: Stop scanning for ble devices...");
			this.bleBridge.StopScanning();
		}

		public void Connect(string deviceId)
		{
			this.updateLog("Applicaton: Connecting to " + deviceId + "...");
			if (deviceId != null)
			{
				this.bleBridge.ConnectToPeripheralWithIdentifier(deviceId, new Action<string, string>(this.ConnectedPeripheralAction), new Action<string, string>(this.DiscoveredServiceAction), new Action<string, string, string>(this.DiscoveredCharacteristicAction), new Action<string, string, string, string>(this.DiscoveredDescriptorAction), new Action<string, string>(this.DisconnectedPeripheralAction));
			}
		}

		public void Disconnect(string deviceId)
		{
			if (deviceId != null)
			{
				this.bleBridge.DisconnectFromPeripheralWithIdentifier(deviceId, new Action<string, string>(this.DisconnectedPeripheralAction));
			}
		}

		private void updateLog(string newline)
		{
			if (!this.isDebug)
			{
				return;
			}
			if (this.logText != null)
			{
				Text text = this.logText;
				text.text = text.text + newline + "\n\n";
				if (this.scrollRect != null && this.logText.preferredHeight > this.scrollRect.gameObject.GetComponent<RectTransform>().rect.height)
				{
					this.logText.gameObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
					this.scrollRect.verticalNormalizedPosition = 0f;
				}
			}
			else
			{
				Debug.Log(newline);
			}
		}

		private void StateUpdateAction(string state)
		{
			this.updateLog("Adapter: State Update = " + state);
			if (state != null)
			{
				if (state == "Powered On")
				{
					this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.PoweredOn);
					return;
				}
				if (state == "Powered Off")
				{
					this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.PoweredOff);
					return;
				}
				if (state == "Resetting")
				{
					this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.Resetting);
					return;
				}
				if (state == "Unauthorized")
				{
					this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.Unauthorized);
					return;
				}
				if (state == "Unknown")
				{
					this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.Unknown);
					return;
				}
				if (state == "Unsupported")
				{
					this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.Unsupported);
					return;
				}
			}
			this.bleToyHandler.onAdapterUpdate(BLEToyAdapterEvents.Unknown);
		}

		private void StartupAction()
		{
			this.updateLog("Bridge: Startup");
		}

		private void ShutdownAction()
		{
			this.updateLog("Bridge: Shutdown");
		}

		private void ErrorAction(string error)
		{
			this.updateLog("Error: " + error);
		}

		private void DiscoveredPeripheralAction(string peripheralId, string name)
		{
			this.updateLog("Bridge: Discovered Device = " + name + ", " + peripheralId);
			this.bleToyHandler.onDiscoveredPeripherial(peripheralId, name);
		}

		private void RetrievedConnectedPeripheralAction(string peripheralId, string name)
		{
			this.updateLog("Bridge: Retrieved Device = " + name + ", " + peripheralId);
		}

		private void ConnectedPeripheralAction(string peripheralId, string name)
		{
			this.updateLog("Bridge: Connected to Device = " + name + ", " + peripheralId);
			this.bleBridge.ReadRssiWithIdentifier(peripheralId);
			this.bleToyHandler.onConnectionEvent(BLEToyConnectionEvents.Connected, peripheralId, name);
		}

		private void DisconnectedPeripheralAction(string peripheralId, string name)
		{
			this.updateLog("Bridge: Disconnected from device = " + name + ", " + peripheralId);
			this.bleToyHandler.onConnectionEvent(BLEToyConnectionEvents.Disconnected, peripheralId, name);
		}

		private void DiscoveredServiceAction(string peripheralId, string service)
		{
			this.updateLog("Bridge: Discovered Service = " + service + ", " + peripheralId);
		}

		private void DiscoveredCharacteristicAction(string peripheralId, string service, string characteristic)
		{
			this.updateLog(string.Concat(new string[] { "Bridge: Discovered Characteristic = ", characteristic, ", ", service, ", ", peripheralId }));
			this.bleToyHandler.onDiscoveredCharacteristic(peripheralId, service, characteristic);
		}

		private void DidWriteCharacteristicAction(string peripheralId, string service, string characteristic)
		{
			this.updateLog(string.Concat(new string[] { "Bridge: Did Write Characteristic = ", characteristic, ", ", service, ", ", peripheralId }));
		}

		private void DidUpdateNotifiationStateForCharacteristicAction(string peripheralId, string uuid)
		{
			this.updateLog("Bridge: Did Update Notification State = " + uuid + ", " + peripheralId);
		}

		private void DidUpdateCharacteristicValueAction(string peripheralId, string characteristic, byte[] data)
		{
			this.updateLog(string.Concat(new string[]
			{
				"Bridge: Did Update Characteristic Value = ",
				peripheralId,
				", ",
				characteristic,
				", ",
				BitConverter.ToString(data)
			}));
		}

		private void DidWriteDescriptorAction(string peripheralId, string characteristic, string descriptor)
		{
			this.updateLog(string.Concat(new string[] { "Bridge: Did Write Descriptor = ", descriptor, ", ", characteristic, ", ", peripheralId }));
		}

		private void DidReadDescriptorAction(string peripheralId, string characteristic, string descriptor, byte[] data)
		{
			this.updateLog(string.Concat(new string[] { "Bridge: Did Read Descriptor = ", descriptor, ", ", characteristic, ", ", peripheralId }));
		}

		private void DiscoveredDescriptorAction(string peripheralId, string service, string characteristic, string descriptor)
		{
			this.updateLog(string.Concat(new string[] { "Bridge: Discovered Descriptor = ", peripheralId, ", ", characteristic, ", ", peripheralId }));
		}

		private void DidUpdateRssiAction(string peripheralId, string rssi)
		{
			this.updateLog("Bridge: RSSI Update = " + rssi + ", " + peripheralId);
			this.bleToyHandler.onRSSIUpdate(peripheralId, rssi);
		}

		private void Start()
		{
			if (this.logText != null)
			{
				this.logText.text = string.Empty;
			}
			RuntimePlatform platform = Application.platform;
			switch (platform)
			{
			case RuntimePlatform.IPhonePlayer:
				this.bleBridge = new iOSBleBridge();
				break;
			default:
				if (platform != RuntimePlatform.OSXEditor)
				{
					this.bleBridge = new DummyBleBridge();
				}
				else
				{
					this.bleBridge = new OsxBleBridge();
				}
				break;
			case RuntimePlatform.Android:
				this.bleBridge = new AndroidBleBridge();
				break;
			}
			this.updateLog("bleBridge: " + this.bleBridge);
			this.bleBridge.Startup(true, new Action(this.StartupAction), new Action<string>(this.ErrorAction), new Action<string>(this.StateUpdateAction), new Action<string, string>(this.DidUpdateRssiAction));
			GameObject gameObject = GameObject.Find("BleBridge");
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
			}
		}

		private void Update()
		{
		}

		private IBleBridge bleBridge;

		public Text logText;

		public ScrollRect scrollRect;

		public bool isDebug = true;

		public GameObject bleToyHandlerGameObject;

		private IBLEToyHandler bleToyHandler;
	}
}
