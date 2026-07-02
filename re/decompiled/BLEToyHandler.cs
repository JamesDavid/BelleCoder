using System;
using startechplus.ble;
using UnityEngine;
using UnityEngine.UI;

public class BLEToyHandler : MonoBehaviour, IBLEToyHandler
{
	private void Start()
	{
		this.infoText.text = "Initializing...";
	}

	private void Update()
	{
	}

	public void ScanAndConnect()
	{
		if (this.isReady)
		{
			this.infoText.text = "Scanning...";
			this.bleHandler.Scan();
		}
		else
		{
			this.infoText.text = "Please wait...";
		}
	}

	public void onAdapterUpdate(BLEToyAdapterEvents state)
	{
		Debug.Log("onAdapterUpdate " + state);
		if (state == BLEToyAdapterEvents.PoweredOn)
		{
			this.isReady = true;
			this.infoText.text = "Ready...";
		}
	}

	public void onConnectionEvent(BLEToyConnectionEvents type, string peripheralId, string name)
	{
		Debug.Log("onConnectionEvent" + type);
		if (type != BLEToyConnectionEvents.Connected)
		{
			if (type == BLEToyConnectionEvents.Disconnected)
			{
				this.currentToyId = null;
			}
		}
		else
		{
			this.currentToyId = peripheralId;
		}
	}

	public void onError(string error)
	{
		Debug.Log("onError " + error);
	}

	public void onDiscoveredPeripherial(string peripheralId, string name)
	{
		if (name == "Makers")
		{
			this.characteristicCount = 0;
			this.bleHandler.Connect(peripheralId);
		}
	}

	private void onNotifcationStateChange(string peripheralId, string service, string characteristic)
	{
		Debug.Log("onNotifcationStateChange");
		if (characteristic == this.toyToAppCharacteristic)
		{
			this.infoText.text = "Connected...";
			byte[] array = new byte[] { 32 };
			this.bleHandler.SendCharacteristicData(this.currentToyId, this.currentServiceId, this.appToToyCharacteristic, array, array.Length);
		}
	}

	private void processInput(byte[] data)
	{
		byte b = data[1];
		byte b2 = b ^ this.lastInputState;
		for (int i = 0; i < 8; i++)
		{
			if (((b2 >> i) & 1) > 0)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Bit ",
					i,
					" changed: ",
					(b >> i) & 1
				}));
				switch (i)
				{
				case 1:
					this.nose.isOn = ((b >> 1) & 1) > 0;
					break;
				case 2:
					this.tail.isOn = ((b >> 2) & 1) > 0;
					break;
				case 3:
					this.lift.isOn = ((b >> 3) & 1) > 0;
					break;
				case 6:
					this.leftEar.isOn = ((b >> 6) & 1) > 0;
					break;
				case 7:
					this.rightEar.isOn = ((b >> 7) & 1) > 0;
					break;
				}
			}
		}
		this.lastInputState = b;
		byte b3 = data[2];
		b2 = b3 ^ this.lastTouchState;
		for (int j = 0; j < 8; j++)
		{
			if (((b2 >> j) & 1) > 0)
			{
				Debug.Log("Bit " + j + " changed...");
				switch (j)
				{
				}
			}
		}
		this.lastTouchState = b3;
	}

	private void onCharacteristicData(string peripheralId, string service, string characteristic, byte[] data)
	{
		Debug.Log("onCharacteristicUpdate: " + BitConverter.ToString(data));
		BLEToyHandler.toyToAppOpCodes toyToAppOpCodes = (BLEToyHandler.toyToAppOpCodes)data[0];
		switch (toyToAppOpCodes)
		{
		case BLEToyHandler.toyToAppOpCodes.PlayListEnabled:
			break;
		default:
			switch (toyToAppOpCodes)
			{
			case BLEToyHandler.toyToAppOpCodes.SendInputState:
				this.processInput(data);
				break;
			case BLEToyHandler.toyToAppOpCodes.IndicateLVDValue:
				Debug.Log("Battery Level = " + data[1]);
				break;
			case BLEToyHandler.toyToAppOpCodes.IndicateToyVolume:
				break;
			default:
				switch (toyToAppOpCodes)
				{
				case BLEToyHandler.toyToAppOpCodes.DebugString:
					break;
				case BLEToyHandler.toyToAppOpCodes.LogString:
					break;
				case BLEToyHandler.toyToAppOpCodes.ErrorString:
					break;
				default:
					if (toyToAppOpCodes != BLEToyHandler.toyToAppOpCodes.PasswordStatus)
					{
						if (toyToAppOpCodes != BLEToyHandler.toyToAppOpCodes.SecurityChallenge)
						{
							if (toyToAppOpCodes != BLEToyHandler.toyToAppOpCodes.MicEvent)
							{
								if (toyToAppOpCodes != BLEToyHandler.toyToAppOpCodes.AppModeSignal)
								{
									if (toyToAppOpCodes != BLEToyHandler.toyToAppOpCodes.FileTransferResponse)
									{
									}
								}
							}
						}
					}
					break;
				}
				break;
			}
			break;
		case BLEToyHandler.toyToAppOpCodes.IndicateSequenceQueueFreespace:
			break;
		case BLEToyHandler.toyToAppOpCodes.ReportAccelerometerValues:
			break;
		case BLEToyHandler.toyToAppOpCodes.SequenceHasEnded:
			break;
		}
	}

	public void enableNotifications()
	{
		if (this.characteristicCount == 2)
		{
			this.bleHandler.SubscribeToCharacteristic(this.currentToyId, this.currentServiceId, this.toyToAppCharacteristic, new Action<string, string, string>(this.onNotifcationStateChange), new Action<string, string, string, byte[]>(this.onCharacteristicData));
		}
	}

	public void onDiscoveredCharacteristic(string peripheralId, string service, string characteristic)
	{
		this.infoText.text = "Connecting...";
		Debug.Log("onDiscoveredCharacteristic: " + characteristic);
		if (characteristic == this.appToToyCharacteristic)
		{
			this.characteristicCount++;
			this.currentServiceId = service;
			this.enableNotifications();
		}
		else if (characteristic == this.toyToAppCharacteristic)
		{
			this.characteristicCount++;
			this.currentServiceId = service;
			this.enableNotifications();
		}
	}

	public void onRSSIUpdate(string peripheralId, string rssi)
	{
		Debug.Log("onRSSIUpdate: " + peripheralId + " : " + rssi);
	}

	public Text infoText;

	public Toggle nose;

	public Toggle lift;

	public Toggle tail;

	public Toggle rightEar;

	public Toggle leftEar;

	private bool isReady;

	public BluetoothHandler bleHandler;

	private string appToToyCharacteristic = "5DAE1383-4D7A-4E00-8DA8-43C2E1954626";

	private string toyToAppCharacteristic = "5DAE1382-4D7A-4E00-8DA8-43C2E1954626";

	private int characteristicCount;

	private string currentToyId;

	private string currentServiceId;

	private byte lastInputState;

	private byte lastTouchState;

	public enum appToToyOpCodes
	{
		PlayAudioSequence = 16,
		RequestInputSate = 32,
		SetPassword = 64,
		SecurityChallengeResponse,
		AppModeSignal = 80,
		FileTransferRequest = 83,
		FileTransferData = 192,
		FileTransferStatus = 84,
		RequestLVDValue = 33,
		CheckToyVolume,
		SetToyVolumue,
		PlaySequence = 23,
		StopSequence,
		EnqueueSequenceCommand = 17,
		StopSequenceQueuePlayback,
		CheckSequenceQueueFreeSpace,
		EnableAccelerometerReporting
	}

	public enum toyToAppOpCodes
	{
		PlayListEnabled = 16,
		SendInputState = 32,
		PasswordStatus = 64,
		SecurityChallenge,
		AppModeSignal = 80,
		FileTransferResponse = 84,
		DebugString = 96,
		LogString,
		ErrorString,
		IndicateLVDValue = 33,
		IndicateToyVolume,
		MicEvent = 27,
		SequenceHasEnded = 23,
		IndicateSequenceQueueFreespace = 19,
		ReportAccelerometerValues
	}
}
