using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITest : MonoBehaviour
{
	private void Awake()
	{
		this.api = BLEToyAPI.instance;
		this.testSections = this.testSectionsHolder.GetComponentsInChildren<BLEToyAPITestSection>(true);
	}

	private void Start()
	{
		this.connectionButtonLabel = this.connectionButton.GetComponentInChildren<Text>();
		this.UpdateConnectionButton("Bluetooth Not Ready");
		this.logs.header.text = "Not Connected";
		this.toysList.SetActive(false);
		this.SetupTests();
		if (this.api.bluetoothIsReady)
		{
			this.HandleBluetoothReady();
		}
	}

	private void OnEnable()
	{
		this.api.OnBluetoothReady += this.HandleBluetoothReady;
		this.api.OnBluetoothNotReady += this.HandleBluetoothNotReady;
		this.api.OnToyDiscovered += this.HandleToyDiscovered;
		this.api.OnToyRSSIUpdate += this.HandleToyRSSIUpdate;
		this.api.OnToyConnected += this.HandleToyConnected;
		this.api.OnToyDisconnected += this.HandleToyDisconnected;
		this.api.OnToyPairing += this.HandleToyPairing;
		this.api.toyLog.OnDebug += this.HandleToyDebug;
		this.api.toyLog.OnLog += this.HandleToyLog;
		this.api.toyLog.OnError += this.HandleToyError;
		this.connectionButton.onClick.AddListener(new UnityAction(this.HandleConnectionButton));
	}

	private void OnDisable()
	{
		this.api.OnBluetoothReady -= this.HandleBluetoothReady;
		this.api.OnBluetoothNotReady -= this.HandleBluetoothNotReady;
		this.api.OnToyDiscovered -= this.HandleToyDiscovered;
		this.api.OnToyRSSIUpdate -= this.HandleToyRSSIUpdate;
		this.api.OnToyConnected -= this.HandleToyConnected;
		this.api.OnToyDisconnected -= this.HandleToyDisconnected;
		this.api.OnToyPairing -= this.HandleToyPairing;
		this.api.toyLog.OnDebug -= this.HandleToyDebug;
		this.api.toyLog.OnLog -= this.HandleToyLog;
		this.api.toyLog.OnError -= this.HandleToyError;
		this.connectionButton.onClick.RemoveListener(new UnityAction(this.HandleConnectionButton));
	}

	private void HandleToyDiscovered(BLEToy toy)
	{
		this.logs.Log("app", "HandleToyDiscovered: " + toy.id);
		this.UpdateToysList();
	}

	private void HandleToyRSSIUpdate(BLEToy toy)
	{
		this.logs.Log("app", string.Concat(new object[] { "HandleToyRSSIUpdate: ", toy.id, " : ", toy.rssi }));
		this.UpdateToysList();
	}

	private void HandleBluetoothReady()
	{
		this.logs.Log("app", "HandleBluetoothReady");
		this.UpdateConnectionButton("Scan For Toys");
	}

	private void HandleBluetoothNotReady()
	{
		this.logs.Log("app", "HandleBluetoothNotReady");
		this.UpdateConnectionButton("Bluetooth Not Ready");
	}

	private void HandleToyPairing()
	{
		this.logs.Log("app", "HandleToyPairing: " + this.api.currentToy.id);
	}

	private void HandleToyConnected()
	{
		this.logs.Log("app", "HandleToyConnected: ");
		this.logs.header.text = this.api.currentToy.name + "\n" + this.api.currentToy.id;
		this.api.SendAppModeSignal(AppModeSignal.GetFlashVersionCode);
		this.UpdateConnectionButton("Disconnect");
		this.toysList.gameObject.SetActive(false);
		this.testSectionsList.gameObject.SetActive(true);
		this.ShowTestSection(this.testSections[0]);
	}

	private void HandleToyDisconnected()
	{
		this.logs.Log("app", "HandleToyDisconnected");
		this.logs.header.text = "Not Connected";
		this.UpdateConnectionButton("Scan For Toys");
		this.ShowTestSection(null);
		this.testSectionsList.gameObject.SetActive(false);
	}

	private void HandleToyDebug(string message)
	{
		this.logs.Log("app", "Toy Debug: " + message);
	}

	private void HandleToyLog(string message)
	{
		this.logs.Log("app", "Toy Log: " + message);
	}

	private void HandleToyError(string message)
	{
		this.logs.Log("app", "Toy Error: " + message);
	}

	private void HandleConnectionButton()
	{
		string text = this.connectionButtonLabel.text;
		if (text != null)
		{
			if (!(text == "Bluetooth Not Ready"))
			{
				if (!(text == "Scan For Toys") && !(text == "Scanning..."))
				{
					if (text == "Disconnect")
					{
						this.api.DisconnectFromToy(this.api.currentToy.id);
						this.UpdateConnectionButton("Disconnecting...");
					}
				}
				else
				{
					this.api.ScanForToys(30f);
					this.toysList.SetActive(true);
					this.UpdateConnectionButton("Scanning...");
				}
			}
		}
	}

	private void UpdateConnectionButton(string label)
	{
		this.connectionButtonLabel.text = label;
		this.connectionButton.interactable = this.connectionButtonLabel.text != "Bluetooth Not Ready";
	}

	private void UpdateToysList()
	{
		Button[] componentsInChildren = this.toysList.GetComponentsInChildren<Button>();
		foreach (Button button in componentsInChildren)
		{
			button.onClick.RemoveAllListeners();
			global::UnityEngine.Object.Destroy(button.gameObject);
		}
		for (int j = 0; j < this.api.discoveredToys.Count; j++)
		{
			BLEToy toy = this.api.discoveredToys[j];
			Button button2 = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, this.toysList.transform);
			button2.transform.localScale = Vector3.one;
			button2.gameObject.name = toy.name;
			button2.GetComponentInChildren<Text>().text = string.Concat(new object[] { toy.name, " : ", toy.rssi, "\n", toy.id });
			button2.onClick.AddListener(delegate
			{
				this.HandleToyListButton(toy);
			});
		}
	}

	private void HandleToyListButton(BLEToy toy)
	{
		this.UpdateConnectionButton("Connecting...");
		this.toysList.SetActive(false);
		this.api.ConnectToToy(toy.id);
	}

	private void SetupTests()
	{
		int num = this.testSections.Length;
		Transform transform = this.testSectionsList.transform.Find("Scroll View/Viewport/Content");
		for (int i = 0; i < num; i++)
		{
			BLEToyAPITestSection section = this.testSections[i];
			Button button = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, transform);
			button.transform.localScale = Vector3.one;
			button.gameObject.name = section.gameObject.name;
			button.GetComponentInChildren<Text>().text = section.gameObject.name;
			button.onClick.AddListener(delegate
			{
				this.ShowBLEToyAPITestSection(section);
			});
		}
		this.testSectionsList.gameObject.SetActive(false);
		this.ShowTestSection(null);
	}

	private void ShowBLEToyAPITestSection(BLEToyAPITestSection section)
	{
		this.ShowTestSection(section);
	}

	private void ShowTestSection(BLEToyAPITestSection section)
	{
		int num = this.testSections.Length;
		for (int i = 0; i < num; i++)
		{
			BLEToyAPITestSection bletoyAPITestSection = this.testSections[i];
			bletoyAPITestSection.gameObject.SetActive(bletoyAPITestSection == section);
		}
	}

	public APITestLogs logs;

	public Button connectionButton;

	private Text connectionButtonLabel;

	public GameObject toysList;

	public Button buttonPrefab;

	public GameObject testSectionsList;

	public Transform testSectionsHolder;

	private BLEToyAPITestSection[] testSections;

	private BLEToyAPI api;
}
