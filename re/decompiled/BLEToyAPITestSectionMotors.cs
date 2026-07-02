using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITestSectionMotors : BLEToyAPITestSection
{
	private void Start()
	{
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			this.updateInterval = 1f;
		}
		float num = 1f;
		this.leftSlider.minValue = -num;
		this.leftSlider.maxValue = num;
		this.leftSlider.value = 0f;
		this.rightSlider.minValue = -num;
		this.rightSlider.maxValue = num;
		this.rightSlider.value = 0f;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.armsSlider = this.armsRow.GetComponentInChildren<Slider>();
		this.leftSlider = this.leftRow.GetComponentInChildren<Slider>();
		this.rightSlider = this.rightRow.GetComponentInChildren<Slider>();
		this.armsSliderValue = this.armsRow.Find("Value").GetComponentInChildren<Text>();
		this.leftSliderValue = this.leftRow.Find("Value").GetComponentInChildren<Text>();
		this.rightSliderValue = this.rightRow.Find("Value").GetComponentInChildren<Text>();
		this.armsSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		this.leftSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		this.rightSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		this.startButton.onClick.AddListener(new UnityAction(this.HandleStartButton));
		this.zeroButton.onClick.AddListener(new UnityAction(this.HandleZeroButton));
		this.stopButton.onClick.AddListener(new UnityAction(this.HandleStopButton));
		this.api.toyInput.OnWheelMoved += this.HandleWheelMoved;
		this.shouldSetPower = false;
		this.AdjustUI();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.armsSlider.onValueChanged.RemoveAllListeners();
		this.leftSlider.onValueChanged.RemoveAllListeners();
		this.rightSlider.onValueChanged.RemoveAllListeners();
		this.startButton.onClick.RemoveAllListeners();
		this.zeroButton.onClick.RemoveAllListeners();
		this.stopButton.onClick.RemoveAllListeners();
		this.api.toyInput.OnWheelMoved -= this.HandleWheelMoved;
		this.api.StopMotors(BLEToyMotorState.Brake);
	}

	private void Update()
	{
		if (this.timeSinceUpdate > this.updateInterval)
		{
			if (this.shouldSetPower)
			{
				this.api.MotorRun(BLEToyMotor.ArmsCam, BLEToyMotorState.Run, this.armsSlider.value, BLEToyMotorDirection.Forward);
				this.api.MotorRun(BLEToyMotor.LeftWheel, BLEToyMotorState.Run, Mathf.Abs(this.leftSlider.value), (this.leftSlider.value < 0f) ? BLEToyMotorDirection.Reverse : BLEToyMotorDirection.Forward);
				this.api.MotorRun(BLEToyMotor.RightWheel, BLEToyMotorState.Run, Mathf.Abs(this.rightSlider.value), (this.rightSlider.value < 0f) ? BLEToyMotorDirection.Reverse : BLEToyMotorDirection.Forward);
			}
			this.timeSinceUpdate = 0f;
		}
		else
		{
			this.timeSinceUpdate += Time.deltaTime;
		}
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			this.leftSlider.value = this.leftSlider.maxValue;
			this.rightSlider.value = this.rightSlider.maxValue;
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			this.leftSlider.value = this.leftSlider.minValue;
			this.rightSlider.value = this.rightSlider.minValue;
		}
		if (Input.GetKeyDown(KeyCode.Space))
		{
			this.leftSlider.value = 0f;
			this.rightSlider.value = 0f;
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.leftSlider.value = this.leftSlider.maxValue;
				this.rightSlider.value = this.rightSlider.minValue;
			}
			else
			{
				this.leftSlider.value = this.leftSlider.maxValue;
				this.rightSlider.value = 0f;
			}
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				this.leftSlider.value = this.leftSlider.minValue;
				this.rightSlider.value = this.rightSlider.maxValue;
			}
			else
			{
				this.leftSlider.value = 0f;
				this.rightSlider.value = this.rightSlider.maxValue;
			}
		}
		this.leftButton.GetComponentInChildren<Text>().text = this.api.toyInput.InputIsPressed(BLEToyInputCode.LEFT_LEAF_A_BUTTON_ID) + " : " + this.api.toyInput.InputIsPressed(BLEToyInputCode.LEFT_LEAF_B_BUTTON_ID);
		this.rightButton.GetComponentInChildren<Text>().text = this.api.toyInput.InputIsPressed(BLEToyInputCode.RIGHT_LEAF_A_BUTTON_ID) + " : " + this.api.toyInput.InputIsPressed(BLEToyInputCode.RIGHT_LEAF_B_BUTTON_ID);
		this.leftButton.interactable = true;
		this.rightButton.interactable = true;
	}

	private void HandleSliderChanged(float value)
	{
		this.AdjustUI();
	}

	private void AdjustUI()
	{
		this.armsSliderValue.text = this.armsSlider.value.ToString("F2");
		this.leftSliderValue.text = this.leftSlider.value.ToString("F2");
		this.rightSliderValue.text = this.rightSlider.value.ToString("F2");
		this.startButton.interactable = !this.shouldSetPower;
		this.armsSlider.interactable = this.shouldSetPower;
		this.leftSlider.interactable = this.shouldSetPower;
		this.rightSlider.interactable = this.shouldSetPower;
		this.zeroButton.interactable = this.shouldSetPower;
		this.stopButton.interactable = this.shouldSetPower;
		this.leftRow.GetComponent<Text>().text = "L : " + this.leftSteps;
		this.rightRow.GetComponent<Text>().text = "R : " + this.rightSteps;
	}

	private void HandleStartButton()
	{
		this.shouldSetPower = true;
		this.AdjustUI();
	}

	private void HandleZeroButton()
	{
		this.armsSlider.value = 0f;
		this.leftSlider.value = 0f;
		this.rightSlider.value = 0f;
		this.shouldSetPower = true;
		this.AdjustUI();
	}

	private void HandleStopButton()
	{
		this.shouldSetPower = false;
		this.AdjustUI();
		this.api.StopMotors(BLEToyMotorState.Brake);
	}

	private void HandleWheelMoved(BLEToyMotor motor, BLEToyMotorDirection direction, int steps)
	{
		if (motor != BLEToyMotor.LeftWheel)
		{
			if (motor == BLEToyMotor.RightWheel)
			{
				this.rightSteps += ((direction != BLEToyMotorDirection.Forward) ? (-steps) : steps);
			}
		}
		else
		{
			this.leftSteps += ((direction != BLEToyMotorDirection.Forward) ? (-steps) : steps);
		}
		this.AdjustUI();
	}

	public Transform armsRow;

	public Transform leftRow;

	public Transform rightRow;

	public Button startButton;

	public Button zeroButton;

	public Button stopButton;

	public Button leftButton;

	public Button rightButton;

	private Slider armsSlider;

	private Slider leftSlider;

	private Slider rightSlider;

	private Text armsSliderValue;

	private Text leftSliderValue;

	private Text rightSliderValue;

	private float updateInterval = 0.05f;

	private float timeSinceUpdate;

	private bool shouldSetPower;

	private int leftSteps;

	private int rightSteps;
}
