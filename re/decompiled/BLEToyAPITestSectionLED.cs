using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITestSectionLED : BLEToyAPITestSection
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			this.updateInterval = 1f;
		}
		this.redSlider = this.redRow.GetComponentInChildren<Slider>();
		this.greenSlider = this.greenRow.GetComponentInChildren<Slider>();
		this.blueSlider = this.blueRow.GetComponentInChildren<Slider>();
		this.redSliderValue = this.redRow.Find("Value").GetComponentInChildren<Text>();
		this.greenSliderValue = this.greenRow.Find("Value").GetComponentInChildren<Text>();
		this.blueSliderValue = this.blueRow.Find("Value").GetComponentInChildren<Text>();
		this.colorButtons = this.colorButtonsGrid.GetComponentsInChildren<Button>();
		this.redSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		this.greenSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		this.blueSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		Button[] array = this.colorButtons;
		for (int i = 0; i < array.Length; i++)
		{
			Button button = array[i];
			Button clickedButton = button;
			button.onClick.AddListener(delegate
			{
				this.HandleColorButton(clickedButton);
			});
		}
		this.AdjustColor(Color.black);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.redSlider.onValueChanged.RemoveAllListeners();
		this.greenSlider.onValueChanged.RemoveAllListeners();
		this.blueSlider.onValueChanged.RemoveAllListeners();
		foreach (Button button in this.colorButtons)
		{
			button.onClick.RemoveAllListeners();
		}
	}

	private void Update()
	{
		if (this.timeSinceUpdate > this.updateInterval)
		{
			this.api.SetLED(this.colorBar.color);
			this.timeSinceUpdate = 0f;
		}
		else
		{
			this.timeSinceUpdate += Time.deltaTime;
		}
	}

	private void AdjustColor(Color color)
	{
		this.colorBar.color = color;
		int num = Mathf.RoundToInt(color.r * 255f);
		int num2 = Mathf.RoundToInt(color.g * 255f);
		int num3 = Mathf.RoundToInt(color.b * 255f);
		this.redSlider.value = (float)num;
		this.greenSlider.value = (float)num2;
		this.blueSlider.value = (float)num3;
		this.redSliderValue.text = num.ToString();
		this.greenSliderValue.text = num2.ToString();
		this.blueSliderValue.text = num3.ToString();
	}

	private void HandleSliderChanged(float value)
	{
		Color color = new Color(this.redSlider.value / 255f, this.greenSlider.value / 255f, this.blueSlider.value / 255f);
		this.AdjustColor(color);
	}

	private void HandleColorButton(Button button)
	{
		Color color = button.GetComponent<Image>().color;
		this.AdjustColor(color);
	}

	public Image colorBar;

	public Transform redRow;

	public Transform greenRow;

	public Transform blueRow;

	public Transform colorButtonsGrid;

	private Slider redSlider;

	private Slider greenSlider;

	private Slider blueSlider;

	private Text redSliderValue;

	private Text greenSliderValue;

	private Text blueSliderValue;

	private Button[] colorButtons;

	private float updateInterval = 0.1f;

	private float timeSinceUpdate;
}
