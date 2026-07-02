using System;
using System.Collections.Generic;
using UnityEngine;

public class BLEToyCodingLED
{
	public BLEToyCodingLED(BLEToyCodingLEDPattern pattern, Color color, float period = 1f, int minCycles = 1)
	{
		this.color = color;
		this.pattern = pattern;
		this.period = period;
		this.minDuration = period * (float)minCycles;
	}

	public Color color { get; private set; }

	public BLEToyCodingLEDPattern pattern { get; private set; }

	public float period { get; private set; }

	public float minDuration { get; private set; }

	public static ushort[] FadeToColor(Color color, float duration)
	{
		int num = Mathf.RoundToInt(color.r * 255f);
		int num2 = Mathf.RoundToInt(color.g * 255f);
		int num3 = Mathf.RoundToInt(color.b * 255f);
		int num4 = Mathf.RoundToInt(duration * 1000f / 8f);
		List<ushort> list = new List<ushort>();
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_TARGET_COLOR | (int)BLEToyLEDCommand.LED_CHANNEL_RED | " + num + ")");
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_TARGET_COLOR | (int)BLEToyLEDCommand.LED_CHANNEL_GREEN | " + num2 + ")");
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_TARGET_COLOR | (int)BLEToyLEDCommand.LED_CHANNEL_BLUE | " + num3 + ")");
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_FADE_DURATION | " + num4 + ")");
		list.Add((ushort)(32768 | num));
		list.Add((ushort)(33792 | num2));
		list.Add((ushort)(34816 | num3));
		list.Add((ushort)(35840 | num4));
		return list.ToArray();
	}

	public static ushort[] Off()
	{
		return BLEToyCodingLED.GetBlankCommands();
	}

	public static ushort[] GetBlankCommands()
	{
		List<ushort> list = new List<ushort>();
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_TARGET_COLOR | (int)BLEToyLEDCommand.LED_CHANNEL_RED | " + 0 + ")");
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_TARGET_COLOR | (int)BLEToyLEDCommand.LED_CHANNEL_GREEN | " + 0 + ")");
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_TARGET_COLOR | (int)BLEToyLEDCommand.LED_CHANNEL_BLUE | " + 0 + ")");
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyLEDCommand.LED_FADE_DURATION | " + 0 + ")");
		list.Add(32768);
		list.Add(33792);
		list.Add(34816);
		list.Add(35840);
		return list.ToArray();
	}

	public static List<BLEToyCodingLEDKeyFrame> GetKeyframesForLED(BLEToyCodingLED led, float duration)
	{
		List<BLEToyCodingLEDKeyFrame> list = new List<BLEToyCodingLEDKeyFrame>();
		float num = 0f;
		float num2 = duration;
		float num3 = led.period * 0.5f;
		switch (led.pattern)
		{
		case BLEToyCodingLEDPattern.Off:
			list.Add(new BLEToyCodingLEDKeyFrame(num, Color.black, 0f));
			break;
		case BLEToyCodingLEDPattern.On:
			list.Add(new BLEToyCodingLEDKeyFrame(num, led.color, 0f));
			break;
		case BLEToyCodingLEDPattern.Blink:
			while (num2 >= led.period)
			{
				list.Add(new BLEToyCodingLEDKeyFrame(num, led.color, 0f));
				num += num3;
				list.Add(new BLEToyCodingLEDKeyFrame(num, Color.black, 0f));
				num += num3;
				num2 = duration - num;
			}
			break;
		case BLEToyCodingLEDPattern.Pulse:
			while (num2 >= led.period)
			{
				list.Add(new BLEToyCodingLEDKeyFrame(num, led.color, num3));
				num += num3;
				list.Add(new BLEToyCodingLEDKeyFrame(num, Color.black, num3));
				num += num3;
				num2 = duration - num;
			}
			break;
		}
		return list;
	}
}
