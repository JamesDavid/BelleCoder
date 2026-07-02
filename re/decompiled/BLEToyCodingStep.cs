using System;
using System.Collections.Generic;
using UnityEngine;

public class BLEToyCodingStep
{
	public BLEToyCodingStep()
	{
		this.led = null;
		this.wheels = null;
		this.arms = null;
	}

	public ushort[] GetCommands()
	{
		List<ushort> list = new List<ushort>();
		float num = 0f;
		Debug.Log("<color=teal>Step Blocks</color>");
		if (this.wheels != null)
		{
			Debug.Log("<color=teal>Movement: " + this.wheels.movement.ToString() + "</color>");
		}
		if (this.arms != null)
		{
			Debug.Log("<color=teal>Cam Position: " + this.arms.camPosition.ToString() + "</color>");
		}
		if (this.led != null)
		{
			Debug.Log("<color=teal>LED: " + this.led.pattern.ToString() + "</color>");
		}
		Debug.Log("<color=teal>-------------</color>");
		if (this.wheels != null && this.wheels.duration > num)
		{
			num = this.wheels.duration;
		}
		if (this.arms != null && this.arms.duration > num)
		{
			num = this.arms.duration;
		}
		if (this.led != null && this.led.minDuration > num)
		{
			num = this.led.minDuration;
		}
		if (this.arms != null)
		{
			ushort[] commands = this.arms.GetCommands();
			foreach (ushort num2 in commands)
			{
				list.Add(num2);
			}
		}
		List<BLEToyCodingWheelsKeyFrame> list2 = new List<BLEToyCodingWheelsKeyFrame>();
		if (this.wheels != null)
		{
			list2 = this.wheels.keyframes;
		}
		List<BLEToyCodingLEDKeyFrame> list3 = new List<BLEToyCodingLEDKeyFrame>();
		ushort[] array2 = this.CommandsForKeyframes(list2, list3, num);
		foreach (ushort num3 in array2)
		{
			list.Add(num3);
		}
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyHighLevelCommand.HL_WAIT_FINISH | (int)BLEToySequenceWaitCommand.WAIT_FINISH_CAM_SEEK)");
		list.Add(10248);
		return list.ToArray();
	}

	private ushort[] CommandsForKeyframes(List<BLEToyCodingWheelsKeyFrame> wheelsFrames, List<BLEToyCodingLEDKeyFrame> ledFrames, float duration)
	{
		Debug.LogWarning(string.Concat(new object[] { "motorframes: ", wheelsFrames.Count, ", ledFrames: ", ledFrames.Count }));
		List<BLEToyCodingWheelsKeyFrame> list = new List<BLEToyCodingWheelsKeyFrame>(wheelsFrames);
		List<BLEToyCodingLEDKeyFrame> list2 = new List<BLEToyCodingLEDKeyFrame>(ledFrames);
		List<ushort> list3 = new List<ushort>();
		float num = 0f;
		BLEToyCodingWheelsKeyFrame bletoyCodingWheelsKeyFrame = default(BLEToyCodingWheelsKeyFrame);
		BLEToyCodingLEDKeyFrame bletoyCodingLEDKeyFrame = default(BLEToyCodingLEDKeyFrame);
		while (list.Count > 0 || list2.Count > 0)
		{
			if (list.Count > 0)
			{
				bletoyCodingWheelsKeyFrame = list[0];
			}
			if (list2.Count > 0)
			{
				bletoyCodingLEDKeyFrame = list2[0];
			}
			if (list.Count > 0 && (list2.Count == 0 || bletoyCodingWheelsKeyFrame.time <= bletoyCodingLEDKeyFrame.time))
			{
				if (bletoyCodingWheelsKeyFrame.time > num)
				{
					float num2 = bletoyCodingWheelsKeyFrame.time - num;
					Debug.LogWarning(num.ToString("F2") + " - MOTOR Command Delay: " + num2);
					list3.Add(BLEToySequenceQueue.HighLevelDelay(num2));
				}
				BLEToyMotorCommand command = bletoyCodingWheelsKeyFrame.command;
				if (command != BLEToyMotorCommand.MOTOR_RUN)
				{
					if (command == BLEToyMotorCommand.MOTOR_STOP)
					{
						Debug.LogWarning(num.ToString("F2") + " - Command STOP Motors: " + bletoyCodingWheelsKeyFrame.motor.ToString());
						list3.Add(BLEToyCodingWheels.BrakeMotor(bletoyCodingWheelsKeyFrame.motor));
					}
				}
				else
				{
					Debug.LogWarning(string.Concat(new object[]
					{
						num.ToString("F2"),
						" - Command Run Motors: ",
						bletoyCodingWheelsKeyFrame.motor.ToString(),
						": ",
						bletoyCodingWheelsKeyFrame.power
					}));
					list3.Add(BLEToyCodingWheels.RunMotor(bletoyCodingWheelsKeyFrame.motor, bletoyCodingWheelsKeyFrame.power));
				}
				num = bletoyCodingWheelsKeyFrame.time;
				list.RemoveAt(0);
			}
			else
			{
				if (list2.Count <= 0 || (list.Count != 0 && bletoyCodingLEDKeyFrame.time > bletoyCodingWheelsKeyFrame.time))
				{
					break;
				}
				if (bletoyCodingLEDKeyFrame.time > num)
				{
					float num2 = bletoyCodingLEDKeyFrame.time - num;
					Debug.LogWarning(num.ToString("F2") + " - LED Command Delay: " + num2);
					list3.Add(BLEToySequenceQueue.HighLevelDelay(num2));
				}
				Debug.LogWarning(string.Concat(new object[]
				{
					num.ToString("F2"),
					" - Command FadeToColor: ",
					bletoyCodingLEDKeyFrame.color,
					": ",
					bletoyCodingLEDKeyFrame.fadeTime
				}));
				ushort[] array = BLEToyCodingLED.FadeToColor(bletoyCodingLEDKeyFrame.color, bletoyCodingLEDKeyFrame.fadeTime);
				int num3 = array.Length;
				for (int i = 0; i < num3; i++)
				{
					list3.Add(array[i]);
				}
				num = bletoyCodingLEDKeyFrame.time;
				list2.RemoveAt(0);
			}
		}
		float num4 = duration - num;
		list3.Add(BLEToySequenceQueue.HighLevelDelay(num4));
		return list3.ToArray();
	}

	public BLEToyCodingWheels wheels;

	public BLEToyCodingArms arms;

	public BLEToyCodingLED led;
}
