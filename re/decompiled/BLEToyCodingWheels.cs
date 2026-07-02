using System;
using System.Collections.Generic;
using UnityEngine;

public class BLEToyCodingWheels
{
	public BLEToyCodingWheels(BLEToyCodingMovement movement)
	{
		this.movement = movement;
		this.keyframes = BLEToyCodingWheels.GetKeyframesForMovement(movement);
		this.duration = BLEToyCodingWheels.GetDurationForKeyframes(this.keyframes);
	}

	public BLEToyCodingMovement movement { get; private set; }

	public List<BLEToyCodingWheelsKeyFrame> keyframes { get; private set; }

	public float duration { get; private set; }

	public ushort[] GetCommands()
	{
		return BLEToyCodingWheels.GetCommandsForMovement(this.movement);
	}

	public static ushort[] GetBlankCommands()
	{
		ushort[] array = new ushort[1];
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_STOP | (int)BLEToyHighLevelCommand.HL_STOP_M1 | (int)BLEToyHighLevelCommand.HL_STOP_M2) | (int)BLEToyHighLevelCommand.HL_STOP_M3");
		array[0] = 28872;
		return array;
	}

	public static List<BLEToyCodingWheelsKeyFrame> GetKeyframesForMovement(BLEToyCodingMovement movement)
	{
		List<BLEToyCodingWheelsKeyFrame> list = new List<BLEToyCodingWheelsKeyFrame>();
		float num = 0f;
		switch (movement)
		{
		case BLEToyCodingMovement.StepForward:
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.BothWheels, BLEToyMotorCommand.MOTOR_RUN, 1f));
			num += BLEToyCodingWheels.stepTime;
			break;
		case BLEToyCodingMovement.StepBackward:
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.BothWheels, BLEToyMotorCommand.MOTOR_RUN, -1f));
			num += BLEToyCodingWheels.stepTime;
			break;
		case BLEToyCodingMovement.TurnCW180:
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.LeftWheel, BLEToyMotorCommand.MOTOR_RUN, 1f));
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.RightWheel, BLEToyMotorCommand.MOTOR_RUN, -1f));
			num += BLEToyCodingWheels.quaterSpinTime * 2f;
			break;
		case BLEToyCodingMovement.TurnCW360:
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.LeftWheel, BLEToyMotorCommand.MOTOR_RUN, 1f));
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.RightWheel, BLEToyMotorCommand.MOTOR_RUN, -1f));
			num += BLEToyCodingWheels.quaterSpinTime * 4f;
			break;
		case BLEToyCodingMovement.TurnCCW180:
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.LeftWheel, BLEToyMotorCommand.MOTOR_RUN, -1f));
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.RightWheel, BLEToyMotorCommand.MOTOR_RUN, 1f));
			num += BLEToyCodingWheels.quaterSpinTime * 2f;
			break;
		case BLEToyCodingMovement.TurnCCW360:
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.LeftWheel, BLEToyMotorCommand.MOTOR_RUN, -1f));
			list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.RightWheel, BLEToyMotorCommand.MOTOR_RUN, 1f));
			num += BLEToyCodingWheels.quaterSpinTime * 4f;
			break;
		}
		list.Add(new BLEToyCodingWheelsKeyFrame(num, BLEToyMotor.BothWheels, BLEToyMotorCommand.MOTOR_RUN, 0f));
		return list;
	}

	public static float GetDurationForKeyframes(List<BLEToyCodingWheelsKeyFrame> keyframes)
	{
		return keyframes[keyframes.Count - 1].time;
	}

	public static ushort[] GetCommandsForKeyframes(List<BLEToyCodingWheelsKeyFrame> keyframes)
	{
		List<ushort> list = new List<ushort>();
		float num = 0f;
		int count = keyframes.Count;
		for (int i = 0; i < count; i++)
		{
			BLEToyCodingWheelsKeyFrame bletoyCodingWheelsKeyFrame = keyframes[i];
			if (bletoyCodingWheelsKeyFrame.time > num)
			{
				float num2 = bletoyCodingWheelsKeyFrame.time - num;
				list.Add(BLEToyCodingWheels.DelayMotors(num2));
			}
			BLEToyMotorCommand command = bletoyCodingWheelsKeyFrame.command;
			if (command != BLEToyMotorCommand.MOTOR_RUN)
			{
				if (command == BLEToyMotorCommand.MOTOR_STOP)
				{
					list.Add(BLEToyCodingWheels.BrakeMotor(bletoyCodingWheelsKeyFrame.motor));
				}
			}
			else
			{
				list.Add(BLEToyCodingWheels.RunMotor(bletoyCodingWheelsKeyFrame.motor, bletoyCodingWheelsKeyFrame.power));
			}
			num = bletoyCodingWheelsKeyFrame.time;
		}
		return list.ToArray();
	}

	public static ushort[] GetCommandsForMovement(BLEToyCodingMovement movement)
	{
		List<BLEToyCodingWheelsKeyFrame> keyframesForMovement = BLEToyCodingWheels.GetKeyframesForMovement(movement);
		return BLEToyCodingWheels.GetCommandsForKeyframes(keyframesForMovement);
	}

	public static ushort RunMotor(BLEToyMotor motor, float power)
	{
		power = Mathf.Clamp(power, -1f, 1f);
		int num = Mathf.RoundToInt(power * 255f);
		if (num < -255)
		{
			num = -255;
		}
		if (num > 255)
		{
			num = 255;
		}
		switch (motor)
		{
		case BLEToyMotor.LeftWheel:
			BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_RUN | (int)BLEToyMotorCommand.MOTOR_M2 | ((int)BLEToyMotorCommand.MOTOR_PWM_DUTY_MASK & " + num + "))");
			return (ushort)(37888 | (511 & num));
		case BLEToyMotor.RightWheel:
			BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_RUN | (int)BLEToyMotorCommand.MOTOR_M3 | ((int)BLEToyMotorCommand.MOTOR_PWM_DUTY_MASK & " + num + "))");
			return (ushort)(38912 | (511 & num));
		}
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_RUN | (int)BLEToyMotorCommand.MOTOR_M2 | (int)BLEToyMotorCommand.MOTOR_M3 | ((int)BLEToyMotorCommand.MOTOR_PWM_DUTY_MASK & " + num + "))");
		return (ushort)(39936 | (511 & num));
	}

	public static ushort BrakeMotor(BLEToyMotor motor)
	{
		switch (motor)
		{
		case BLEToyMotor.LeftWheel:
			BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_STOP | (int)BLEToyHighLevelCommand.HL_STOP_M1)");
			return 28680;
		case BLEToyMotor.RightWheel:
			BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_STOP | (int)BLEToyHighLevelCommand.HL_STOP_M2)");
			return 28736;
		}
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_STOP | (int)BLEToyHighLevelCommand.HL_STOP_M1 | (int)BLEToyHighLevelCommand.HL_STOP_M2)");
		return 28744;
	}

	public static ushort DelayMotors(float seconds)
	{
		if (seconds < 4f)
		{
			seconds = Mathf.Clamp(seconds, 0f, 4f);
			int num = Mathf.RoundToInt(seconds * 1000f);
			BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_DELAY | " + num);
			return (ushort)(4096 | num);
		}
		seconds = Mathf.Clamp(seconds, 0f, 400f);
		int num2 = Mathf.RoundToInt(seconds * 10f);
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_LONG_DELAY | " + num2);
		return (ushort)(8192 | num2);
	}

	private static float quaterSpinTime = 0.825f;

	private static float stepTime = 1f;
}
