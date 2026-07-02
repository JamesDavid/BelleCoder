using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITestSectionCamPositions : BLEToyAPITestSection
{
	private void Awake()
	{
		this.tabs = this.tabsHolder.GetComponentsInChildren<Button>();
		this.HandleTab(this.tabs[0]);
		this.positionText.text = string.Empty;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Button[] array = this.tabs;
		for (int i = 0; i < array.Length; i++)
		{
			Button button = array[i];
			Button t = button;
			button.onClick.AddListener(delegate
			{
				this.HandleTab(t);
			});
		}
		this.api.toyInput.OnCamPositionChanged += this.HandleCamPositionChanged;
		this.stopButton.onClick.AddListener(new UnityAction(this.HandleStopButton));
		this.AdjustUI();
	}

	protected override void OnDisable()
	{
		foreach (Button button in this.tabs)
		{
			button.onClick.RemoveAllListeners();
		}
		this.api.toyInput.OnCamPositionChanged -= this.HandleCamPositionChanged;
		this.stopButton.onClick.RemoveAllListeners();
		base.OnDisable();
	}

	private void HandleCamPositionChanged(BLEToyCamPosition camPosition)
	{
		this.positionText.text = camPosition.ToString();
	}

	private void AdjustUI()
	{
		if (this.buttons != null)
		{
			foreach (Button button in this.buttons)
			{
				if (button != null)
				{
					button.onClick.RemoveAllListeners();
					global::UnityEngine.Object.Destroy(button.gameObject);
				}
			}
		}
		int length = Enum.GetValues(typeof(BLEToyCamPosition)).Length;
		this.buttons = new Button[length];
		int num = 0;
		IEnumerator enumerator = Enum.GetValues(typeof(BLEToyCamPosition)).GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				BLEToyCamPosition camPosition = (BLEToyCamPosition)obj;
				string text = camPosition.ToString();
				Button button2 = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, this.contentHolder);
				button2.transform.localScale = Vector3.one;
				button2.gameObject.name = text;
				button2.GetComponentInChildren<Text>().text = text;
				button2.onClick.AddListener(delegate
				{
					this.HandleButton(camPosition);
				});
				this.buttons[num] = button2;
				num++;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	private void HandleTab(Button tab)
	{
		this.currentTab = tab;
		foreach (Button button in this.tabs)
		{
			button.interactable = button != tab;
		}
	}

	private void HandleButton(BLEToyCamPosition camPosition)
	{
		BLEToyCamDirection bletoyCamDirection = (BLEToyCamDirection)Enum.Parse(typeof(BLEToyCamDirection), this.currentTab.name);
		if (bletoyCamDirection == BLEToyCamDirection.Closest)
		{
			this.api.MotorRun(BLEToyMotor.LeftWheel, BLEToyMotorState.Run, 0.9f, BLEToyMotorDirection.Forward);
			this.api.MotorRun(BLEToyMotor.RightWheel, BLEToyMotorState.Run, 0.9f, BLEToyMotorDirection.Reverse);
		}
		this.api.CamGotoPosition(camPosition, bletoyCamDirection);
	}

	private void HandleStopButton()
	{
		this.api.StopMotors(BLEToyMotorState.Brake);
	}

	public Button buttonPrefab;

	public Transform contentHolder;

	private Button[] buttons;

	public Transform tabsHolder;

	private Button[] tabs;

	private Button currentTab;

	public Text positionText;

	public Button stopButton;
}
