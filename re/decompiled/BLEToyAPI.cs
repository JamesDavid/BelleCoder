using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using startechplus.ble;
using UnityEngine;

public class BLEToyAPI : MonoBehaviour, IBLEToyHandler
{
	public BLEToy currentToy { get; private set; }

	public bool inConnectedFreePlay { get; private set; }

	public bool bluetoothIsReady { get; private set; }

	public BLEToyInput toyInput { get; private set; }

	public BLEToyLog toyLog { get; private set; }

	public BLEToySequenceQueue sequenceQueue { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBluetoothReady;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnBluetoothNotReady;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToy> OnToyDiscovered;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToy> OnToyRSSIUpdate;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnScanTimedOut;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnToyPairing;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnToyConnected;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnToyDisconnected;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnToyFreePlayStarted;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnToyFreePlayEnded;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<AppModeSignal> OnAppModeSignal;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnToyVolume;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnMusicFadeOutComplete;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToyAudioPhrase> OnPlaylistEnded;

	public static BLEToyAPI instance
	{
		get
		{
			if (BLEToyAPI._instance == null)
			{
				GameObject gameObject = new GameObject("BLE Toy API");
				global::UnityEngine.Object.DontDestroyOnLoad(gameObject);
				BLEToyAPI._instance = gameObject.AddComponent<BLEToyAPI>();
			}
			return BLEToyAPI._instance;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Load()
	{
		if (!global::UnityEngine.Object.FindObjectOfType<BluetoothHandler>())
		{
			BLEToyAPI.instance.Setup();
		}
	}

	private void Setup()
	{
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			this.logLevel = BLEToyLogLevel.Error;
		}
		this.Log(BLEToyLogLevel.Verbose, "BLE Toy API Setup!");
		this.toyInput = new BLEToyInput(this);
		this.toyLog = new BLEToyLog();
		this.sequenceQueue = new BLEToySequenceQueue(this);
		this.SetupIdleSequences();
		GameObject gameObject = new GameObject("Bluetooth Handler");
		gameObject.transform.SetParent(base.transform);
		this.bleHandler = gameObject.AddComponent<BluetoothHandler>();
		bool flag = false;
		this.bleHandler.Setup(flag, base.gameObject);
		this.toyInput.OnInputReleased += this.HandleToyInputReleased;
	}

	private void Update()
	{
		if (this.ToyIsConnected())
		{
			if (!this.inConnectedFreePlay)
			{
				this.timeSinceKeepAlive += Time.deltaTime;
				if (this.timeSinceKeepAlive > this.keepAliveInterval)
				{
					this.SendAppModeSignal(AppModeSignal.RCAppModeKeepAlive);
					this.timeSinceKeepAlive = 0f;
				}
				this.sequenceQueue.UpdateWithDeltaTime(Time.deltaTime);
			}
			this.toyInput.UpdateWithDeltaTime(Time.deltaTime);
			this.UpdateIdle(Time.deltaTime);
		}
		if (this.scanningForToys)
		{
			if (this.timeSinceScanStart < this.scanDuration)
			{
				this.timeSinceScanStart += Time.deltaTime;
			}
			else
			{
				this.StopScanningForToys();
				if (this.OnScanTimedOut != null)
				{
					this.OnScanTimedOut();
				}
			}
		}
	}

	private void OnDestroy()
	{
		if (this.toyInput != null)
		{
			this.toyInput.OnInputReleased -= this.HandleToyInputReleased;
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused && this.currentToy != null)
		{
			this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
			this.SendAppModeSignal(AppModeSignal.EndAppMode);
			this.DisconnectFromToy(this.currentToy.id);
			if (this.OnToyDisconnected != null)
			{
				this.OnToyDisconnected();
			}
		}
	}

	public bool ScanForToys(float duration = 30f)
	{
		if (this.bluetoothIsReady)
		{
			this.scanDuration = duration;
			this.timeSinceScanStart = 0f;
			this.scanningForToys = true;
			this.discoveredToys = new List<BLEToy>();
			this.bleHandler.Scan();
			return true;
		}
		this.Log(BLEToyLogLevel.Warning, "ScanForToys: Bluetooth NOT ready to scan.");
		return false;
	}

	public void StopScanningForToys()
	{
		this.bleHandler.StopScanning();
		this.scanningForToys = false;
	}

	public void ConnectToToy(string toyID)
	{
		this.StopScanningForToys();
		this.appToToyCharacteristicFound = false;
		this.toyToAppCharacteristicFound = false;
		this.bleHandler.Connect(toyID);
	}

	public void DisconnectFromToy(string toyID)
	{
		global::UnityEngine.Debug.Log("DisconnectFromToy: " + toyID);
		base.StartCoroutine(this.DisconnectRoutine(toyID));
	}

	private IEnumerator DisconnectRoutine(string toyID)
	{
		this.CancelFadeOutMusic();
		this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
		yield return new WaitForSeconds(0.1f);
		this.bleHandler.Disconnect(toyID);
		yield break;
	}

	private void ToyConnected()
	{
		this.Log(BLEToyLogLevel.Verbose, "ToyConnected");
		base.StartCoroutine("ToyConnectedRoutine");
	}

	private void HandleWaitForNecklaceReleased(BLEToyInputCode inputCode)
	{
		if (inputCode == BLEToyInputCode.NECKLACE_BUTTON_ID)
		{
			this.waitForNecklacePairing = false;
		}
	}

	public string LastConnectedToyID()
	{
		return PlayerPrefs.GetString("lastConnectedToyID", string.Empty);
	}

	private IEnumerator ToyConnectedRoutine()
	{
		string lastConnectedToyID = this.LastConnectedToyID();
		if (lastConnectedToyID != this.currentToy.id)
		{
			if (this.OnToyPairing != null)
			{
				this.OnToyPairing();
			}
			this.toyInput.OnInputReleased += this.HandleWaitForNecklaceReleased;
			this.waitForNecklacePairing = true;
			while (this.waitForNecklacePairing)
			{
				yield return null;
			}
			this.toyInput.OnInputReleased -= this.HandleWaitForNecklaceReleased;
		}
		yield return new WaitForSecondsRealtime(0.5f);
		PlayerPrefs.SetString("lastConnectedToyID", this.currentToy.id);
		this.currentToy.SetConnected(true);
		this.inConnectedFreePlay = false;
		this.ResetIdle();
		this.SendAppModeSignal(AppModeSignal.RCAppModeKeepAlive);
		yield return new WaitForSecondsRealtime(0.5f);
		this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
		this.PlayAudioSequence(BLEToyAudioPhrase.eP276);
		yield return new WaitForSecondsRealtime(0.25f);
		if (this.OnToyConnected != null)
		{
			this.OnToyConnected();
		}
		yield break;
	}

	private void ToyDisconnected()
	{
		base.StopCoroutine("ToyConnectedRoutine");
		this.currentToy = null;
		this.connectedWithNotifications = false;
		if (this.OnToyDisconnected != null)
		{
			this.OnToyDisconnected();
		}
	}

	public bool ToyIsConnected()
	{
		return this.connectedWithNotifications && this.currentToy != null;
	}

	public void StartConnectedFreePlay(bool playExitPhrase = false, BLEToyAudioPhrase exitPhrase = BLEToyAudioPhrase.eP798)
	{
		this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
		if (playExitPhrase)
		{
			this.PlayAudioSequence(exitPhrase);
		}
		this.SendAppModeSignal(AppModeSignal.EndAppMode);
		this.inConnectedFreePlay = true;
		if (this.OnToyFreePlayStarted != null)
		{
			this.OnToyFreePlayStarted();
		}
	}

	public void EndConnectedFreePlay()
	{
		this.inConnectedFreePlay = false;
		this.SendAppModeSignal(AppModeSignal.RCAppModeKeepAlive);
		if (this.OnToyFreePlayEnded != null)
		{
			this.OnToyFreePlayEnded();
		}
	}

	private void ResetIdle()
	{
		this.timeUntilIdle = 20f;
	}

	private void UpdateIdle(float deltaTime)
	{
		if (!this.ToyIsConnected())
		{
			return;
		}
		if (this.inConnectedFreePlay)
		{
			this.ResetIdle();
			return;
		}
		if (Input.GetMouseButton(0))
		{
			this.ResetIdle();
			return;
		}
		if (this.timeUntilIdle > 0f)
		{
			this.timeUntilIdle -= deltaTime;
			if (this.timeUntilIdle <= 0f)
			{
				this.PlayIdleSequence();
			}
		}
	}

	private void SetupIdleSequences()
	{
		this.idleSequenceIndex = 0;
		this.idleSequences = new List<BLEToyHLSequence>();
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_2);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_3);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_5);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_6);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_7);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_8);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_9);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_10);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_12);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_13);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_14);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_15);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_16);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_17);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_18);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_19);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_20);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_22);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_23);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_24);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_25);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_26);
		this.idleSequences.Add(BLEToyHLSequence.eHLS_IdlePhrase_27);
	}

	private void PlayIdleSequence()
	{
		int num;
		for (num = this.idleSequenceIndex; num == this.idleSequenceIndex; num = global::UnityEngine.Random.Range(0, this.idleSequences.Count))
		{
		}
		this.sequenceQueue.PlayHLSequence(this.idleSequences[num]);
		this.idleSequenceIndex = num;
	}

	private void HandleToyInputReleased(BLEToyInputCode inputcode)
	{
		if (!this.ToyIsConnected())
		{
			return;
		}
		if (this.waitForNecklacePairing)
		{
			return;
		}
		if (inputcode == BLEToyInputCode.NECKLACE_BUTTON_ID)
		{
			this.StartConnectedFreePlay(false, BLEToyAudioPhrase.eP798);
		}
	}

	public void SendPacketToToy(byte[] packet)
	{
		if (this.bleHandler == null)
		{
			this.Log(BLEToyLogLevel.Warning, "SendPacketToToy Failed: bleHandler is NULL");
			return;
		}
		if (this.currentToy == null)
		{
			this.Log(BLEToyLogLevel.Warning, "SendPacketToToy Failed: currentToy is NULL");
			return;
		}
		if (packet.Length > 20)
		{
			this.Log(BLEToyLogLevel.Warning, "SendPacketToToy Packet TOO Large: " + packet.Length);
		}
		if (this.inConnectedFreePlay)
		{
			this.EndConnectedFreePlay();
		}
		if (packet[0] != 80)
		{
			this.ResetIdle();
		}
		BLEToyLogLevel bletoyLogLevel = BLEToyLogLevel.Debug;
		object[] array = new object[6];
		array[0] = "SendPacketToToy: (";
		array[1] = packet.Length;
		array[2] = " bytes) <color=green>";
		int num = 3;
		AppToToyOpCodes appToToyOpCodes = (AppToToyOpCodes)packet[0];
		array[num] = appToToyOpCodes.ToString();
		array[4] = "</color> ";
		array[5] = BitConverter.ToString(packet);
		this.Log(bletoyLogLevel, string.Concat(array));
		this.bleHandler.SendCharacteristicData(this.currentToy.id, this.currentServiceID, "51901383-030F-4859-B643-256B0B2F5562", packet, packet.Length);
	}

	public void RequestInputState()
	{
		if (!this.ToyIsConnected())
		{
			this.Log(BLEToyLogLevel.Warning, "NOT doing RequestInputState because toy not connected.");
			return;
		}
		this.Log(BLEToyLogLevel.Debug, "RequestInputState");
		this.SendPacketToToy(new byte[] { 32 });
	}

	public void SendAppModeSignal(AppModeSignal signal)
	{
		this.Log(BLEToyLogLevel.Verbose, "SendAppModeSignal: " + signal);
		this.SendPacketToToy(new byte[]
		{
			80,
			(byte)signal
		});
	}

	private void ProcessAppModeSignal(byte[] data)
	{
		AppModeSignal appModeSignal = (AppModeSignal)data[1];
		this.Log(BLEToyLogLevel.Verbose, "ProcessAppModeSignal: " + appModeSignal);
		if (this.OnAppModeSignal != null)
		{
			this.OnAppModeSignal(appModeSignal);
		}
	}

	public void SetLED(Color color)
	{
		this.SendPacketToToy(new byte[]
		{
			21,
			(byte)Mathf.RoundToInt(color.r * 255f),
			(byte)Mathf.RoundToInt(color.g * 255f),
			(byte)Mathf.RoundToInt(color.b * 255f)
		});
	}

	public void FadeLED(Color color, float duration)
	{
		ushort[] array = BLEToyCodingLED.FadeToColor(color, duration);
		this.sequenceQueue.AddCommands(array);
	}

	public void StartLEDPattern(Color color, BLEToyCodingLEDPattern pattern, float period = 1f)
	{
		this.StopLEDPattern();
		this.currentLEDPattern = pattern;
		this.currentLEDPatternColor = color;
		this.currentLEDPatternHalfPeriod = period * 0.5f;
		base.StartCoroutine("LEDPatternRoutine");
	}

	public void StopLEDPattern()
	{
		base.StopCoroutine("LEDPatternRoutine");
	}

	private IEnumerator LEDPatternRoutine()
	{
		if (!this.ToyIsConnected())
		{
			yield break;
		}
		switch (this.currentLEDPattern)
		{
		case BLEToyCodingLEDPattern.On:
			this.SetLED(this.currentLEDPatternColor);
			goto IL_01CC;
		case BLEToyCodingLEDPattern.Blink:
			for (;;)
			{
				this.SetLED(this.currentLEDPatternColor);
				yield return new WaitForSeconds(this.currentLEDPatternHalfPeriod);
				this.SetLED(Color.black);
				yield return new WaitForSeconds(this.currentLEDPatternHalfPeriod);
			}
			break;
		case BLEToyCodingLEDPattern.Pulse:
			for (;;)
			{
				this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
				this.FadeLED(this.currentLEDPatternColor, this.currentLEDPatternHalfPeriod);
				yield return new WaitForSeconds(this.currentLEDPatternHalfPeriod);
				this.FadeLED(Color.black, this.currentLEDPatternHalfPeriod);
				yield return new WaitForSeconds(this.currentLEDPatternHalfPeriod);
			}
			break;
		}
		this.SetLED(Color.black);
		IL_01CC:
		yield break;
	}

	public void PlayLoopingSong(BLEToyAudioPhraseSong song, int loops = 8)
	{
		this.CancelFadeOutMusic();
		List<ushort> list = new List<ushort>();
		for (int i = 0; i < loops; i++)
		{
			list.Add((ushort)((BLEToyAudioPhraseSong)12288 | song));
			list.Add(10272);
		}
		this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
		this.sequenceQueue.AddCommands(list.ToArray());
		this.SetToyVolume(5);
	}

	private void StopMusic()
	{
		this.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.AudioPlayList);
	}

	public void FadeOutMusic(float time = 3f)
	{
		base.StartCoroutine("FadeOutRoutine", time);
	}

	public void CancelFadeOutMusic()
	{
		base.StopCoroutine("FadeOutRoutine");
		this.FadeOutComplete(true);
	}

	private IEnumerator FadeOutRoutine(float time)
	{
		if (this.ToyIsConnected())
		{
			for (int i = 4; i >= 0; i--)
			{
				yield return new WaitForSeconds(time / 5f);
				this.SetToyVolume(i);
			}
		}
		else
		{
			yield return new WaitForSeconds(0.25f);
		}
		this.FadeOutComplete(false);
		yield break;
	}

	private void FadeOutComplete(bool wasInterrupted)
	{
		this.StopMusic();
		if (this.fadeVolume != 5)
		{
			this.SetToyVolume(5);
		}
		if (!wasInterrupted && this.OnMusicFadeOutComplete != null)
		{
			this.OnMusicFadeOutComplete();
		}
	}

	public void ClearMusicFadeOutComplete()
	{
		this.OnMusicFadeOutComplete = null;
	}

	public void CheckToyVolume()
	{
		this.Log(BLEToyLogLevel.Debug, "CheckToyVolume");
		this.SendPacketToToy(new byte[] { 34 });
	}

	private void ProcessToyVolume(byte[] data)
	{
		ushort num = (ushort)data[1];
		this.Log(BLEToyLogLevel.Debug, "ProcessToyVolume: " + num);
		if (this.OnToyVolume != null)
		{
			this.OnToyVolume((int)num);
		}
	}

	public void SetToyVolume(int volume)
	{
		this.Log(BLEToyLogLevel.Debug, "SetToyVolume: " + volume);
		this.fadeVolume = volume;
		volume = Mathf.Clamp(volume, 0, 5);
		this.SendPacketToToy(new byte[]
		{
			35,
			(byte)volume
		});
	}

	public void CamGotoPosition(BLEToyCamPosition position, BLEToyCamDirection direction)
	{
		this.SendPacketToToy(new byte[]
		{
			25,
			(byte)direction,
			(byte)position
		});
	}

	public void MotorRun(BLEToyMotor motor, BLEToyMotorState state, float power, BLEToyMotorDirection direction)
	{
		byte[] array = new byte[5];
		array[0] = 20;
		array[1] = (byte)motor;
		array[2] = (byte)state;
		int num = Mathf.RoundToInt(Mathf.Clamp01(power) * 255f);
		array[3] = (byte)num;
		array[4] = (byte)direction;
		this.SendPacketToToy(array);
		this.motorDirections[motor] = direction;
	}

	public void StopMotors(BLEToyMotorState state = BLEToyMotorState.Brake)
	{
		this.MotorRun(BLEToyMotor.ArmsCam, state, 0f, BLEToyMotorDirection.Forward);
		this.MotorRun(BLEToyMotor.LeftWheel, state, 0f, BLEToyMotorDirection.Forward);
		this.MotorRun(BLEToyMotor.RightWheel, state, 0f, BLEToyMotorDirection.Forward);
	}

	public BLEToyMotorDirection GetMotorDirection(BLEToyMotor motor)
	{
		if (this.motorDirections.ContainsKey(motor))
		{
			return this.motorDirections[motor];
		}
		return BLEToyMotorDirection.Forward;
	}

	public float PlayAudioSequence(BLEToyAudioPhrase audioPhrase)
	{
		this.CancelFadeOutMusic();
		byte[] array = new byte[3];
		array[0] = 16;
		byte[] bytes = BitConverter.GetBytes((ushort)audioPhrase);
		Array.Copy(bytes, 0, array, 1, 2);
		this.SendPacketToToy(array);
		return BLEToyAudio.TimeForPhrase(audioPhrase);
	}

	public void PlayAudioSequence(ushort audioPhraseIndex)
	{
		this.CancelFadeOutMusic();
		byte[] array = new byte[3];
		array[0] = 16;
		byte[] bytes = BitConverter.GetBytes(audioPhraseIndex);
		Array.Copy(bytes, 0, array, 1, 2);
		this.SendPacketToToy(array);
	}

	public void ProcessPlaylistEnded(byte[] data)
	{
		ushort num = BitConverter.ToUInt16(data, 1);
		BLEToyAudioPhrase bletoyAudioPhrase = (BLEToyAudioPhrase)num;
		this.Log(BLEToyLogLevel.Debug, "ProcessPlaylistEnded: <color=orange>" + bletoyAudioPhrase.ToString() + "</color>");
		if (!this.inConnectedFreePlay && this.OnPlaylistEnded != null)
		{
			this.OnPlaylistEnded(bletoyAudioPhrase);
		}
	}

	public void onError(string error)
	{
		this.Log(BLEToyLogLevel.Error, "onError " + error);
	}

	public void onAdapterUpdate(BLEToyAdapterEvents state)
	{
		this.Log(BLEToyLogLevel.Verbose, "onAdapterUpdate " + state);
		switch (state)
		{
		case BLEToyAdapterEvents.PoweredOn:
			this.bluetoothIsReady = true;
			if (this.OnBluetoothReady != null)
			{
				this.OnBluetoothReady();
			}
			return;
		}
		this.bluetoothIsReady = false;
		if (this.OnBluetoothNotReady != null)
		{
			this.OnBluetoothNotReady();
		}
	}

	public void onDiscoveredPeripherial(string peripheralID, string name)
	{
		this.Log(BLEToyLogLevel.Verbose, "onDiscoveredPeripherial " + peripheralID + ", " + name);
		if (name == this.toyNameFilter || this.toyNameFilter == null || this.toyNameFilter.Length == 0)
		{
			bool flag = false;
			foreach (BLEToy bletoy in this.discoveredToys)
			{
				if (bletoy.id == peripheralID)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				BLEToy bletoy2 = new BLEToy(peripheralID, name);
				this.discoveredToys.Add(bletoy2);
				if (this.OnToyDiscovered != null)
				{
					this.OnToyDiscovered(bletoy2);
				}
			}
		}
	}

	public void onRSSIUpdate(string peripheralID, string rssi)
	{
		this.Log(BLEToyLogLevel.Verbose, "onRSSIUpdate: " + peripheralID + " : " + rssi);
		foreach (BLEToy bletoy in this.discoveredToys)
		{
			if (bletoy.id == peripheralID)
			{
				int num = int.Parse(rssi);
				bletoy.SetRSSI(num);
				if (this.OnToyRSSIUpdate != null)
				{
					this.OnToyRSSIUpdate(bletoy);
				}
			}
		}
	}

	public void onConnectionEvent(BLEToyConnectionEvents type, string peripheralID, string name)
	{
		this.Log(BLEToyLogLevel.Verbose, "onConnectionEvent: " + type);
		if (type != BLEToyConnectionEvents.Connected)
		{
			if (type == BLEToyConnectionEvents.Disconnected)
			{
				this.ToyDisconnected();
			}
		}
		else
		{
			this.currentToy = new BLEToy(peripheralID, name);
		}
	}

	public void onDiscoveredCharacteristic(string peripheralId, string service, string characteristic)
	{
		this.Log(BLEToyLogLevel.Verbose, string.Concat(new string[] { "onDiscoveredCharacteristic: ", peripheralId, ", ", service, ", ", characteristic }));
		if (peripheralId == this.currentToy.id)
		{
			if (characteristic == "51901383-030F-4859-B643-256B0B2F5562")
			{
				this.Log(BLEToyLogLevel.Warning, "Discovered appToToyCharacteristic");
				this.appToToyCharacteristicFound = true;
				this.currentServiceID = service;
				this.enableNotifications();
			}
			else if (characteristic == "51901382-030F-4859-B643-256B0B2F5562")
			{
				this.Log(BLEToyLogLevel.Warning, "Discovered toyToAppCharacteristic");
				this.toyToAppCharacteristicFound = true;
				this.currentServiceID = service;
				this.enableNotifications();
			}
		}
	}

	private void enableNotifications()
	{
		if (this.appToToyCharacteristicFound && this.toyToAppCharacteristicFound)
		{
			this.Log(BLEToyLogLevel.Warning, "enableNotifications SubscribeToCharacteristic");
			this.bleHandler.SubscribeToCharacteristic(this.currentToy.id, this.currentServiceID, "51901382-030F-4859-B643-256B0B2F5562", new Action<string, string, string>(this.onNotifcationStateChange), new Action<string, string, string, byte[]>(this.onCharacteristicData));
		}
	}

	private void onNotifcationStateChange(string peripheralId, string service, string characteristic)
	{
		this.Log(BLEToyLogLevel.Verbose, string.Concat(new string[] { "onNotifcationStateChange: ", peripheralId, ", ", service, ", ", characteristic }));
		if (characteristic == "51901382-030F-4859-B643-256B0B2F5562")
		{
			this.connectedWithNotifications = true;
			this.ToyConnected();
		}
	}

	private void onCharacteristicData(string peripheralId, string service, string characteristic, byte[] data)
	{
		BLEToyLogLevel bletoyLogLevel = BLEToyLogLevel.Verbose;
		string text = "onCharacteristicUpdate: <color=teal>";
		ToyToAppOpCodes toyToAppOpCodes = (ToyToAppOpCodes)data[0];
		this.Log(bletoyLogLevel, text + toyToAppOpCodes.ToString() + "</color> " + BitConverter.ToString(data));
		ToyToAppOpCodes toyToAppOpCodes2 = (ToyToAppOpCodes)data[0];
		switch (toyToAppOpCodes2)
		{
		case ToyToAppOpCodes.DebugString:
			this.toyLog.ProcessDebug(data);
			break;
		case ToyToAppOpCodes.LogString:
			this.toyLog.ProcessLog(data);
			break;
		case ToyToAppOpCodes.ErrorString:
			this.toyLog.ProcessError(data);
			break;
		default:
			switch (toyToAppOpCodes2)
			{
			case ToyToAppOpCodes.PlaylistEnded:
				this.ProcessPlaylistEnded(data);
				break;
			default:
				switch (toyToAppOpCodes2)
				{
				case ToyToAppOpCodes.SendInputState:
					this.toyInput.ProcessInput(data);
					break;
				default:
					if (toyToAppOpCodes2 != ToyToAppOpCodes.SequenceHasEnded)
					{
						if (toyToAppOpCodes2 != ToyToAppOpCodes.AppModeSignal)
						{
							this.Log(BLEToyLogLevel.Warning, "ToyToAppOpCode NOT recognized: " + data[0]);
						}
						else
						{
							this.ProcessAppModeSignal(data);
						}
					}
					else
					{
						this.sequenceQueue.ProcessSequenceHasEnded(data);
					}
					break;
				case ToyToAppOpCodes.IndicateToyVolume:
					this.ProcessToyVolume(data);
					break;
				}
				break;
			case ToyToAppOpCodes.IndicateSequenceQueueFreeSpace:
				this.sequenceQueue.ProcessSequenceQueueFreespace(data);
				break;
			}
			break;
		}
	}

	public void Log(BLEToyLogLevel level, string message)
	{
		if (level < this.logLevel)
		{
			return;
		}
		string text = "grey";
		switch (level)
		{
		case BLEToyLogLevel.Verbose:
			text = "grey";
			break;
		case BLEToyLogLevel.Debug:
			text = "lightblue";
			break;
		case BLEToyLogLevel.Warning:
			text = "olive";
			break;
		case BLEToyLogLevel.Error:
			text = "brown";
			break;
		}
		string text2 = string.Concat(new string[]
		{
			"(",
			Time.time.ToString("F2"),
			") <color=",
			text,
			">[BLEToyAPI] ",
			message,
			"</color>"
		});
		if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			text2 = this.StripHTML(text2);
		}
		switch (level)
		{
		case BLEToyLogLevel.Verbose:
		case BLEToyLogLevel.Debug:
			global::UnityEngine.Debug.Log(text2);
			break;
		case BLEToyLogLevel.Warning:
			global::UnityEngine.Debug.LogWarning(text2);
			break;
		case BLEToyLogLevel.Error:
			global::UnityEngine.Debug.LogError(text2);
			break;
		}
	}

	private string StripHTML(string input)
	{
		return Regex.Replace(input, "<.*?>", string.Empty);
	}

	public BLEToyLogLevel logLevel = BLEToyLogLevel.Debug;

	private BluetoothHandler bleHandler;

	private string toyNameFilter = "DanceCD";

	private const string appToToyCharacteristic = "51901383-030F-4859-B643-256B0B2F5562";

	private const string toyToAppCharacteristic = "51901382-030F-4859-B643-256B0B2F5562";

	private bool appToToyCharacteristicFound;

	private bool toyToAppCharacteristicFound;

	private string currentServiceID;

	private bool connectedWithNotifications;

	public List<BLEToy> discoveredToys = new List<BLEToy>();

	private float keepAliveInterval = 1f;

	private float timeSinceKeepAlive;

	private bool scanningForToys;

	private float scanDuration = 10f;

	private float timeSinceScanStart;

	private const float idleTimeThreshold = 20f;

	private float timeUntilIdle = float.MaxValue;

	private List<BLEToyHLSequence> idleSequences;

	private int idleSequenceIndex;

	private static BLEToyAPI _instance;

	private bool waitForNecklacePairing;

	private Color currentLEDPatternColor;

	private float currentLEDPatternHalfPeriod;

	private BLEToyCodingLEDPattern currentLEDPattern;

	private int fadeVolume = 5;

	private Dictionary<BLEToyMotor, BLEToyMotorDirection> motorDirections = new Dictionary<BLEToyMotor, BLEToyMotorDirection>();
}
