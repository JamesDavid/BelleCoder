using System;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITestSectionBattery : BLEToyAPITestSection
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.checkButton.onClick.AddListener(new UnityAction(this.HandleCheckButton));
		this.AdjustUI();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.checkButton.onClick.RemoveAllListeners();
	}

	private void HandleCheckButton()
	{
	}

	private void HandleBatteryLevel(float level)
	{
		this.batteryLevel = level;
		this.AdjustUI();
	}

	private void AdjustUI()
	{
		this.batterySlider.value = this.batteryLevel;
		this.batterySliderValue.text = this.batteryLevel.ToString("P0");
	}

	public Button checkButton;

	public Slider batterySlider;

	public Text batterySliderValue;

	private float batteryLevel;
}
