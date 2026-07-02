using System;
using UnityEngine;

namespace startechplus.ble
{
	public class BluetoothLeDevice : MonoBehaviour
	{
		public void OnBleStateUpdate(string message)
		{
			if (this.StateUpdateAction != null)
			{
				this.StateUpdateAction(message);
			}
		}

		public void OnError(string message)
		{
			if (this.ErrorAction != null)
			{
				this.ErrorAction(message);
			}
		}

		private string getToken(string currentString, int startIndex, out int stopIndex)
		{
			string text = currentString.Substring(startIndex);
			int num = text.IndexOf(":");
			string text2 = text.Substring(0, num);
			string text3 = text.Substring(num + 1, int.Parse(text2));
			stopIndex = num + 1 + text3.Length + startIndex;
			return text3;
		}

		private string[] ParseMessage(string message, int expectedValues)
		{
			string[] array = new string[expectedValues];
			int num = 0;
			for (int i = 0; i < expectedValues; i++)
			{
				array[i] = this.getToken(message, num, out num);
			}
			return array;
		}

		public void OnDiscoveredPeripheral(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DiscoveredPeripheralAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DiscoveredPeripheralAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.DiscoveredPeripheralAction(array[0], array[1]);
				}
			}
		}

		public void OnStartup(string message)
		{
			if (this.StartupAction != null)
			{
				this.StartupAction();
			}
		}

		public void OnShutdown(string message)
		{
			if (this.ShutdownAction != null)
			{
				this.ShutdownAction();
			}
		}

		public void OnRetrievedPeripheralWithServiceUUIDs(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.RetrievedPeripheralWithServiceAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.RetrievedPeripheralWithServiceAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.RetrievedPeripheralWithServiceAction(array[0], array[1]);
				}
			}
		}

		public void OnRetrievedPeripheralWithUUID(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.RetrievedPeripheralWithUUIDAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.RetrievedPeripheralWithUUIDAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.RetrievedPeripheralWithUUIDAction(array[0], array[1]);
				}
			}
		}

		public void OnConnectedPeripheral(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.ConnectedPeripheralAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.ConnectedPeripheralAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.ConnectedPeripheralAction(array[0], array[1]);
				}
			}
		}

		public void OnDiscoveredService(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DiscoveredServiceAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DiscoveredServiceAction(array[0].ToUpper(), array[1].ToUpper());
				}
				else
				{
					this.DiscoveredServiceAction(array[0], array[1]);
				}
			}
		}

		public void OnDisconnectedPeripheral(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DisconnectedPeripheralAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DisconnectedPeripheralAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.DisconnectedPeripheralAction(array[0], array[1]);
				}
			}
		}

		public void OnDiscoveredCharacteristic(string message)
		{
			string[] array = this.ParseMessage(message, 3);
			if (this.DiscoveredCharacteristicAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DiscoveredCharacteristicAction(array[0].ToUpper(), array[1].ToUpper(), array[2].ToUpper());
				}
				else
				{
					this.DiscoveredCharacteristicAction(array[0], array[1], array[2]);
				}
			}
		}

		public void OnDiscoveredDescriptor(string message)
		{
			string[] array = this.ParseMessage(message, 3);
			if (this.DiscoveredDescriptorAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DiscoveredDescriptorAction(array[0].ToUpper(), string.Empty, array[1].ToUpper(), array[2].ToUpper());
				}
				else
				{
					this.DiscoveredDescriptorAction(array[0], string.Empty, array[1], array[2]);
				}
			}
		}

		public void OnDidWriteCharacteristic(string message)
		{
			string[] array = this.ParseMessage(message, 3);
			Debug.Log(string.Concat(new string[]
			{
				"OnDidWriteCharacteristic ",
				array[0],
				", ",
				array[1],
				", ",
				array[2]
			}));
			if (this.DidWriteCharacteristicAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidWriteCharacteristicAction(array[0].ToUpper(), array[1].ToUpper(), array[2].ToUpper());
				}
				else
				{
					this.DidWriteCharacteristicAction(array[0], array[1], array[2]);
				}
			}
		}

		public void OnDidUpdateNotificationStateForCharacteristicAction(string message)
		{
			string[] array = this.ParseMessage(message, 3);
			if (this.DidUpdateNotificationStateForCharacteristicAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidUpdateNotificationStateForCharacteristicAction(array[0].ToUpper(), array[1].ToUpper(), array[2].ToUpper());
				}
				else
				{
					this.DidUpdateNotificationStateForCharacteristicAction(array[0], array[1], array[2]);
				}
			}
		}

		public void OnBluetoothData(string message)
		{
			string[] array = this.ParseMessage(message, 4);
			string text = array[0];
			string text2 = array[1];
			string text3 = array[2];
			string text4 = array[3];
			if (text4 != null)
			{
				byte[] array2 = Convert.FromBase64String(text4);
				if (array2.Length > 0 && this.DidUpdateCharacteristicValueAction != null)
				{
					if (this.isLowerCaseUUID)
					{
						this.DidUpdateCharacteristicValueAction(text.ToUpper(), text2.ToUpper(), text3.ToUpper(), array2);
					}
					else
					{
						this.DidUpdateCharacteristicValueAction(text, text2, text3, array2);
					}
				}
			}
		}

		public void OnDidWriteDescriptor(string message)
		{
			string[] array = this.ParseMessage(message, 4);
			if (Application.platform == RuntimePlatform.Android)
			{
				string text = array[3].ToUpper();
				string[] array2 = text.Split(new char[] { '-' });
				if (array2[0].Contains("2902"))
				{
					if (this.DidUpdateNotificationStateForCharacteristicAction != null)
					{
						if (this.isLowerCaseUUID)
						{
							this.DidUpdateNotificationStateForCharacteristicAction(array[0].ToUpper(), array[1].ToUpper(), array[2].ToUpper());
						}
						else
						{
							this.DidUpdateNotificationStateForCharacteristicAction(array[0], array[1], array[2]);
						}
					}
				}
				else if (this.DidWriteDescriptorAction != null)
				{
					if (this.isLowerCaseUUID)
					{
						this.DidWriteDescriptorAction(array[0].ToUpper(), array[1].ToUpper(), array[2].ToUpper(), array[3].ToUpper());
					}
					else
					{
						this.DidWriteDescriptorAction(array[0], array[1], array[2], array[3]);
					}
				}
			}
			else if (this.DidWriteDescriptorAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidWriteDescriptorAction(array[0].ToUpper(), array[1].ToUpper(), array[2].ToUpper(), array[3].ToUpper());
				}
				else
				{
					this.DidWriteDescriptorAction(array[0], array[1], array[2], array[3]);
				}
			}
		}

		public void OnDescriptorRead(string message)
		{
			string[] array = this.ParseMessage(message, 4);
			string text = array[0];
			string text2 = array[1];
			string text3 = array[2];
			string text4 = array[3];
			string text5 = array[4];
			if (text5 != null)
			{
				byte[] array2 = Convert.FromBase64String(text5);
				if (array2.Length > 0 && this.DidReadDescriptorValueAction != null)
				{
					if (this.isLowerCaseUUID)
					{
						this.DidReadDescriptorValueAction(text.ToUpper(), text2.ToUpper(), text3.ToUpper(), text4.ToUpper(), array2);
					}
					else
					{
						this.DidReadDescriptorValueAction(text, text2, text3, text4, array2);
					}
				}
			}
		}

		public void OnRssiUpdate(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidUpdateRssiAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidUpdateRssiAction(array[0].ToUpper(), array[1].ToUpper());
				}
				else
				{
					this.DidUpdateRssiAction(array[0], array[1]);
				}
			}
		}

		public void OnAdvertisementDataLocalName(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseLocalNameAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseLocalNameAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.DidAdvertiseLocalNameAction(array[0], array[1]);
				}
			}
		}

		public void OnAdvertisementDataManufactureData(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseManufactureDataAction != null && array[1] != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseManufactureDataAction(array[0].ToUpper(), Convert.FromBase64String(array[1]));
				}
				else
				{
					this.DidAdvertiseManufactureDataAction(array[0], Convert.FromBase64String(array[1]));
				}
			}
		}

		public void OnAdvertisementDataServiceData(string message)
		{
			string[] array = this.ParseMessage(message, 3);
			if (this.DidAdvertiseServiceDataAction != null && array[2] != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseServiceDataAction(array[0].ToUpper(), array[1].ToUpper(), Convert.FromBase64String(array[2]));
				}
				else
				{
					this.DidAdvertiseServiceDataAction(array[0], array[1], Convert.FromBase64String(array[2]));
				}
			}
		}

		public void OnAdvertisementDataServiceUUID(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseServiceAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseServiceAction(array[0].ToUpper(), array[1].ToUpper());
				}
				else
				{
					this.DidAdvertiseServiceAction(array[0], array[1]);
				}
			}
		}

		public void OnAdvertisementDataOverflowServiceUUID(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseOverflowServiceAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseOverflowServiceAction(array[0].ToUpper(), array[1].ToUpper());
				}
				else
				{
					this.DidAdvertiseOverflowServiceAction(array[0], array[1]);
				}
			}
		}

		public void OnAdvertisementDataTxPowerLevel(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseTxPowerLevelAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseTxPowerLevelAction(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.DidAdvertiseTxPowerLevelAction(array[0], array[1]);
				}
			}
		}

		public void OnAdvertisementDataIsConnectable(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseIsConnectable != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseIsConnectable(array[0].ToUpper(), array[1]);
				}
				else
				{
					this.DidAdvertiseIsConnectable(array[0], array[1]);
				}
			}
		}

		public void OnAdvertisementDataSolicitedServiceUUID(string message)
		{
			string[] array = this.ParseMessage(message, 2);
			if (this.DidAdvertiseSolicitedServiceAction != null)
			{
				if (this.isLowerCaseUUID)
				{
					this.DidAdvertiseSolicitedServiceAction(array[0].ToUpper(), array[1].ToUpper());
				}
				else
				{
					this.DidAdvertiseSolicitedServiceAction(array[0], array[1]);
				}
			}
		}

		public Action<string> StateUpdateAction;

		public Action StartupAction;

		public Action ShutdownAction;

		public Action<string> ErrorAction;

		public Action<string, string> ConnectedPeripheralAction;

		public Action<string, string> DisconnectedPeripheralAction;

		public Action<string, string> DiscoveredPeripheralAction;

		public Action<string, string> RetrievedPeripheralWithServiceAction;

		public Action<string, string> RetrievedPeripheralWithUUIDAction;

		public Action<string, string> DiscoveredServiceAction;

		public Action<string, string, string> DiscoveredCharacteristicAction;

		public Action<string, string, string> DidWriteCharacteristicAction;

		public Action<string, string, string> DidUpdateNotificationStateForCharacteristicAction;

		public Action<string, string, string, byte[]> DidUpdateCharacteristicValueAction;

		public Action<string, string, string, string> DidWriteDescriptorAction;

		public Action<string, string, string, string, byte[]> DidReadDescriptorValueAction;

		public Action<string, string, string, string> DiscoveredDescriptorAction;

		public Action<string, string> DidUpdateRssiAction;

		public Action<string, string> DidAdvertiseLocalNameAction;

		public Action<string, byte[]> DidAdvertiseManufactureDataAction;

		public Action<string, string, byte[]> DidAdvertiseServiceDataAction;

		public Action<string, string> DidAdvertiseServiceAction;

		public Action<string, string> DidAdvertiseOverflowServiceAction;

		public Action<string, string> DidAdvertiseTxPowerLevelAction;

		public Action<string, string> DidAdvertiseIsConnectable;

		public Action<string, string> DidAdvertiseSolicitedServiceAction;

		public bool isLowerCaseUUID;

		private bool Initialized;
	}
}
