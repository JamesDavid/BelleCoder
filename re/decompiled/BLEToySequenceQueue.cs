using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BLEToySequenceQueue
{
	public BLEToySequenceQueue(BLEToyAPI apiInstance)
	{
		this.api = apiInstance;
		this.queue = new List<ushort>();
		if (Application.platform == RuntimePlatform.OSXEditor)
		{
			this.pollFreespaceInterval = 1.5f;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnSequenceQueueEnded;

	public void UpdateWithDeltaTime(float deltaTime)
	{
		if (this.queue.Count > 0)
		{
			this.timeSincePollFreespace += deltaTime;
			if (this.timeSincePollFreespace > this.pollFreespaceInterval)
			{
				this.CheckSequenceQueueFreespace();
			}
		}
		else
		{
			this.timeSincePollFreespace = 0f;
		}
	}

	public void AddCommands(ushort[] commands)
	{
		this.api.Log(BLEToyLogLevel.Verbose, "AddCommands: " + commands.Length);
		int num = commands.Length;
		for (int i = 0; i < num; i++)
		{
			this.queue.Add(commands[i]);
		}
	}

	public void StopAndClear(BLEToyStopSequenceFlag flag = BLEToyStopSequenceFlag.All)
	{
		this.api.Log(BLEToyLogLevel.Debug, "StopAndClear");
		this.queue = new List<ushort>();
		this.StopSequenceQueuePlayback();
		this.StopSequence(flag);
	}

	private void SendCommands()
	{
		if (this.queue.Count == 0)
		{
			return;
		}
		int num = this.queue.Count;
		if (num > this.lastKnownFreespace)
		{
			num = this.lastKnownFreespace;
		}
		if (num > this.assumedFreespace)
		{
			num = this.assumedFreespace;
		}
		if (num > 30)
		{
			num = 30;
		}
		if (num <= 0)
		{
			return;
		}
		ushort[] array = this.queue.GetRange(0, num).ToArray();
		this.queue.RemoveRange(0, num);
		this.assumedFreespace -= num;
		this.api.Log(BLEToyLogLevel.Debug, "Sending <color=teal>" + array.Length + "</color>");
		this.EnqueueSequenceCommands(array);
	}

	private void EnqueueSequenceCommands(ushort[] commands)
	{
		int num = 1 + commands.Length * 2;
		if (num > 15)
		{
			num = 15;
		}
		byte[] array = new byte[num];
		array[0] = 17;
		int num2 = 1;
		int num3 = commands.Length;
		int num4 = 0;
		while (num4 < num3 && num4 < 7)
		{
			ushort num5 = commands[num4];
			byte[] bytes = BitConverter.GetBytes(num5);
			array[num2] = bytes[0];
			num2++;
			array[num2] = bytes[1];
			num2++;
			num4++;
		}
		this.api.SendPacketToToy(array);
		if (commands.Length > 7)
		{
			int num6 = commands.Length - 7;
			ushort[] array2 = new ushort[num6];
			Array.Copy(commands, 7, array2, 0, num6);
			this.EnqueueSequenceCommands(array2);
		}
	}

	private void StopSequenceQueuePlayback()
	{
		byte[] array = new byte[] { 18 };
		this.api.SendPacketToToy(array);
	}

	public void StopSequence(BLEToyStopSequenceFlag flag)
	{
		this.StopSequence(new BLEToyStopSequenceFlag[] { flag });
	}

	public void StopSequence(BLEToyStopSequenceFlag[] flags)
	{
		byte b = 0;
		foreach (BLEToyStopSequenceFlag bletoyStopSequenceFlag in flags)
		{
			b = (byte)bletoyStopSequenceFlag | b;
		}
		byte[] array = new byte[] { 24, b };
		this.api.SendPacketToToy(array);
	}

	public void CheckSequenceQueueFreespace()
	{
		if (!this.api.ToyIsConnected())
		{
			return;
		}
		this.api.Log(BLEToyLogLevel.Debug, "<color=yellow>CheckSequenceQueueFreespace</color>");
		this.waitingForFreespaceResponse = true;
		byte[] array = new byte[] { 19 };
		this.api.SendPacketToToy(array);
		this.timeSincePollFreespace = 0f;
	}

	public float PlayHLSequence(BLEToyHLSequence sequence)
	{
		this.api.CancelFadeOutMusic();
		byte[] array = new byte[4];
		array[0] = 23;
		array[1] = 2;
		byte[] bytes = BitConverter.GetBytes((ushort)sequence);
		Array.Copy(bytes, 0, array, 2, 2);
		this.api.SendPacketToToy(array);
		return BLEToyHLSequences.TimeForSequence(sequence);
	}

	public void ProcessSequenceQueueFreespace(byte[] data)
	{
		if (!this.waitingForFreespaceResponse)
		{
			return;
		}
		this.lastKnownFreespace = (int)data[1];
		if (this.lastKnownFreespace > 0)
		{
			this.lastKnownFreespace--;
		}
		this.api.Log(BLEToyLogLevel.Debug, "ProcessSequenceQueueFreespace: <color=orange>" + this.lastKnownFreespace + "</orange>");
		this.assumedFreespace = this.lastKnownFreespace;
		this.SendCommands();
		this.waitingForFreespaceResponse = false;
	}

	public void ProcessSequenceHasEnded(byte[] data)
	{
		BLEToySequenceType bletoySequenceType = (BLEToySequenceType)data[1];
		ushort num = BitConverter.ToUInt16(data, 2);
		this.api.Log(BLEToyLogLevel.Debug, string.Concat(new object[]
		{
			"ProcessSequenceHasEnded: <color=orange>",
			bletoySequenceType.ToString(),
			", ",
			num,
			"</color>"
		}));
		if (bletoySequenceType == BLEToySequenceType.Queue && !this.api.inConnectedFreePlay && this.OnSequenceQueueEnded != null)
		{
			this.OnSequenceQueueEnded();
		}
	}

	public static ushort HighLevelDelay(float seconds)
	{
		if (seconds < 4f)
		{
			seconds = Mathf.Clamp(seconds, 0f, 4f);
			int num = Mathf.RoundToInt(seconds * 1000f);
			BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyHighLevelCommand.HL_DELAY | " + num + ")");
			return (ushort)(4096 | num);
		}
		seconds = Mathf.Clamp(seconds, 0f, 400f);
		int num2 = Mathf.RoundToInt(seconds * 10f);
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyHighLevelCommand.HL_LONG_DELAY | " + num2 + ")");
		return (ushort)(8192 | num2);
	}

	private BLEToyAPI api;

	private float pollFreespaceInterval = 0.66f;

	private float timeSincePollFreespace;

	private int lastKnownFreespace;

	private int assumedFreespace;

	private List<ushort> queue;

	private bool waitingForFreespaceResponse = true;

	private const int maxCommandsToSend = 30;

	public static List<string> debugCommandList = new List<string>();
}
