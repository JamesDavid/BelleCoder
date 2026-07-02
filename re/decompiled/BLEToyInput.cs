using System;
using System.Diagnostics;
using UnityEngine;

public class BLEToyInput
{
	public BLEToyInput(BLEToyAPI apiInstance)
	{
		this.api = apiInstance;
		this.camPosition = BLEToyCamPosition.ArmsDown;
		int length = Enum.GetValues(typeof(BLEToyInputCode)).Length;
		this.inputStates = new bool[length];
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToyToyInteraction> OnInteraction;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToyInputCode> OnInputPressed;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToyInputCode> OnInputReleased;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToyMotor, BLEToyMotorDirection, int> OnWheelMoved;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<BLEToyCamPosition> OnCamPositionChanged;

	public BLEToyCamPosition camPosition { get; private set; }

	public bool InputIsPressed(BLEToyInputCode inputCode)
	{
		return this.inputStates[(int)inputCode];
	}

	public void ProcessInput(byte[] data)
	{
		int num = this.inputStates.Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = i / 8 + 1;
			byte b = data[num2];
			int num3 = i % 8;
			bool flag = ((b >> num3) & 1) > 0;
			BLEToyInputCode bletoyInputCode = (BLEToyInputCode)i;
			this.UpdateInputState(bletoyInputCode, flag);
		}
	}

	private void UpdateInputState(BLEToyInputCode inputCode, bool state)
	{
		bool flag = this.inputStates[(int)inputCode];
		if (state != flag)
		{
			this.inputStates[(int)inputCode] = state;
			if (state)
			{
				this.HandleInteractionInputPressed(inputCode);
				if (!this.api.inConnectedFreePlay && this.OnInputPressed != null)
				{
					this.OnInputPressed(inputCode);
				}
			}
			else
			{
				this.HandleInteractionInputReleased(inputCode);
				if (!this.api.inConnectedFreePlay && this.OnInputReleased != null)
				{
					this.OnInputReleased(inputCode);
				}
			}
		}
	}

	public void ResetInteractions()
	{
	}

	private void HandleInteractionInputPressed(BLEToyInputCode inputCode)
	{
		switch (inputCode)
		{
		case BLEToyInputCode.LEFT_LEAF_A_BUTTON_ID:
		case BLEToyInputCode.LEFT_LEAF_B_BUTTON_ID:
		case BLEToyInputCode.RIGHT_LEAF_A_BUTTON_ID:
		case BLEToyInputCode.RIGHT_LEAF_B_BUTTON_ID:
			this.UpdateWheelsState();
			break;
		case BLEToyInputCode.ARM_WIPER_B0_BUTTON_ID:
		case BLEToyInputCode.ARM_WIPER_B1_BUTTON_ID:
		case BLEToyInputCode.ARM_WIPER_B2_BUTTON_ID:
		case BLEToyInputCode.ARM_WIPER_B3_BUTTON_ID:
			this.UpdateArmsState();
			break;
		}
	}

	private void HandleInteractionInputReleased(BLEToyInputCode inputCode)
	{
		switch (inputCode)
		{
		case BLEToyInputCode.LEFT_LEAF_A_BUTTON_ID:
		case BLEToyInputCode.LEFT_LEAF_B_BUTTON_ID:
		case BLEToyInputCode.RIGHT_LEAF_A_BUTTON_ID:
		case BLEToyInputCode.RIGHT_LEAF_B_BUTTON_ID:
			this.UpdateWheelsState();
			break;
		case BLEToyInputCode.ARM_WIPER_B0_BUTTON_ID:
		case BLEToyInputCode.ARM_WIPER_B1_BUTTON_ID:
		case BLEToyInputCode.ARM_WIPER_B2_BUTTON_ID:
		case BLEToyInputCode.ARM_WIPER_B3_BUTTON_ID:
			this.UpdateArmsState();
			break;
		}
	}

	public void UpdateWithDeltaTime(float deltaTime)
	{
		this.timeSinceInteraction += deltaTime;
		if (this.timeSinceInteraction >= 30f)
		{
			this.ResetInteractions();
			return;
		}
	}

	private void InteractionHappened(BLEToyToyInteraction interaction)
	{
		this.timeSinceInteraction = 0f;
		if (this.api.ToyIsConnected() && !this.api.inConnectedFreePlay && this.OnInteraction != null)
		{
			this.OnInteraction(interaction);
		}
	}

	private void UpdateWheelsState()
	{
		bool flag = this.inputStates[8];
		bool flag2 = this.inputStates[9];
		bool flag3 = this.inputStates[10];
		bool flag4 = this.inputStates[11];
		int num = 0;
		if (flag && flag2)
		{
			num = 0;
		}
		else if (flag && !flag2)
		{
			num = 1;
		}
		else if (!flag && !flag2)
		{
			num = 2;
		}
		else if (!flag && flag2)
		{
			num = 3;
		}
		int num2 = 0;
		if (flag3 && flag4)
		{
			num2 = 0;
		}
		else if (flag3 && !flag4)
		{
			num2 = 1;
		}
		else if (!flag3 && !flag4)
		{
			num2 = 2;
		}
		else if (!flag3 && flag4)
		{
			num2 = 3;
		}
		BLEToyMotorDirection motorDirection = this.api.GetMotorDirection(BLEToyMotor.LeftWheel);
		if (motorDirection == BLEToyMotorDirection.Forward && num < this.previousLeftWheelStep)
		{
			num += 4;
		}
		if (motorDirection == BLEToyMotorDirection.Reverse && num > this.previousLeftWheelStep)
		{
			num -= 4;
		}
		int num3 = num - this.previousLeftWheelStep;
		num3 = Mathf.Abs(num3);
		if (num3 > 0)
		{
			if (motorDirection == this.leftLastRecordedDirection && !this.api.inConnectedFreePlay && this.OnWheelMoved != null)
			{
				this.OnWheelMoved(BLEToyMotor.LeftWheel, motorDirection, num3);
			}
			this.leftLastRecordedDirection = motorDirection;
		}
		BLEToyMotorDirection motorDirection2 = this.api.GetMotorDirection(BLEToyMotor.RightWheel);
		if (motorDirection2 == BLEToyMotorDirection.Forward && num2 < this.previousRightWheelStep)
		{
			num2 += 4;
		}
		if (motorDirection2 == BLEToyMotorDirection.Reverse && num2 > this.previousRightWheelStep)
		{
			num2 -= 4;
		}
		int num4 = num2 - this.previousRightWheelStep;
		num4 = Mathf.Abs(num4);
		if (num4 > 0)
		{
			if (motorDirection2 == this.rightLastRecordedDirection && !this.api.inConnectedFreePlay && this.OnWheelMoved != null)
			{
				this.OnWheelMoved(BLEToyMotor.RightWheel, motorDirection2, num4);
			}
			this.rightLastRecordedDirection = motorDirection2;
		}
		this.previousLeftWheelStep = num % 4;
		this.previousRightWheelStep = num2 % 4;
	}

	private void UpdateArmsState()
	{
		bool flag = this.inputStates[12];
		bool flag2 = this.inputStates[13];
		bool flag3 = this.inputStates[14];
		int num = 0;
		if (flag)
		{
			num |= 1;
		}
		if (flag2)
		{
			num |= 2;
		}
		if (flag3)
		{
			num |= 4;
		}
		BLEToyCamWiperCombo bletoyCamWiperCombo = (BLEToyCamWiperCombo)num;
		int num2 = Array.IndexOf<BLEToyCamWiperCombo>(this.camWiperCombos, this.previousWiperCombo);
		int num3 = Array.IndexOf<BLEToyCamWiperCombo>(this.camWiperCombos, bletoyCamWiperCombo);
		if (num2 < 0)
		{
			num2 = 0;
		}
		if (num3 < 0)
		{
			num3 = 0;
		}
		BLEToyCamDirection bletoyCamDirection = BLEToyCamDirection.CW;
		if ((num2 > num3 && (num3 != 0 || num2 != this.camWiperCombos.Length - 1)) || (num2 == 0 && num3 == this.camWiperCombos.Length - 1))
		{
			bletoyCamDirection = BLEToyCamDirection.CCW;
		}
		BLEToyCamPosition bletoyCamPosition = BLEToyCamPosition.ArmsDown;
		if (bletoyCamDirection == BLEToyCamDirection.CW)
		{
			switch (bletoyCamWiperCombo)
			{
			case BLEToyCamWiperCombo.A:
				bletoyCamPosition = BLEToyCamPosition.ArmsDown;
				break;
			case BLEToyCamWiperCombo.B:
				bletoyCamPosition = BLEToyCamPosition.ArmsOut;
				break;
			case BLEToyCamWiperCombo.D:
				bletoyCamPosition = BLEToyCamPosition.LeftArmUp;
				break;
			case BLEToyCamWiperCombo.C:
				bletoyCamPosition = BLEToyCamPosition.LeftArmOut;
				break;
			case BLEToyCamWiperCombo.H:
				bletoyCamPosition = BLEToyCamPosition.ArmsForward;
				break;
			case BLEToyCamWiperCombo.G:
				bletoyCamPosition = BLEToyCamPosition.ArmsForward;
				break;
			case BLEToyCamWiperCombo.E:
				bletoyCamPosition = BLEToyCamPosition.LeftArmDown;
				break;
			case BLEToyCamWiperCombo.F:
				bletoyCamPosition = BLEToyCamPosition.ArmsUp;
				break;
			}
		}
		else
		{
			switch (bletoyCamWiperCombo)
			{
			case BLEToyCamWiperCombo.A:
				bletoyCamPosition = BLEToyCamPosition.ArmsOut;
				break;
			case BLEToyCamWiperCombo.B:
				bletoyCamPosition = BLEToyCamPosition.LeftArmOut;
				break;
			case BLEToyCamWiperCombo.D:
				bletoyCamPosition = BLEToyCamPosition.LeftArmDown;
				break;
			case BLEToyCamWiperCombo.C:
				bletoyCamPosition = BLEToyCamPosition.LeftArmUp;
				break;
			case BLEToyCamWiperCombo.H:
				bletoyCamPosition = BLEToyCamPosition.ArmsDown;
				break;
			case BLEToyCamWiperCombo.G:
				bletoyCamPosition = BLEToyCamPosition.ArmsDown;
				break;
			case BLEToyCamWiperCombo.E:
				bletoyCamPosition = BLEToyCamPosition.ArmsUp;
				break;
			case BLEToyCamWiperCombo.F:
				bletoyCamPosition = BLEToyCamPosition.ArmsForward;
				break;
			}
		}
		if (this.camPosition != bletoyCamPosition)
		{
			this.camPosition = bletoyCamPosition;
			if (!this.api.inConnectedFreePlay && this.OnCamPositionChanged != null)
			{
				this.OnCamPositionChanged(bletoyCamPosition);
			}
		}
		this.previousWiperCombo = bletoyCamWiperCombo;
	}

	private bool[] inputStates;

	private BLEToyAPI api;

	private int previousLeftWheelStep;

	private int previousRightWheelStep;

	private BLEToyMotorDirection leftLastRecordedDirection;

	private BLEToyMotorDirection rightLastRecordedDirection;

	private BLEToyCamWiperCombo previousWiperCombo;

	private BLEToyCamWiperCombo[] camWiperCombos = new BLEToyCamWiperCombo[]
	{
		BLEToyCamWiperCombo.A,
		BLEToyCamWiperCombo.B,
		BLEToyCamWiperCombo.C,
		BLEToyCamWiperCombo.D,
		BLEToyCamWiperCombo.E,
		BLEToyCamWiperCombo.F,
		BLEToyCamWiperCombo.G,
		BLEToyCamWiperCombo.H
	};

	private const float interactionResetThreshold = 30f;

	private float timeSinceInteraction;
}
