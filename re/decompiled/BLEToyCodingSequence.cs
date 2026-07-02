using System;
using System.Collections.Generic;
using UnityEngine;

public class BLEToyCodingSequence
{
	public BLEToyCodingSequence()
	{
		this.steps = new List<BLEToyCodingStep>();
	}

	public void SetSong(BLEToyAudioPhraseSong song)
	{
		this.song = song;
	}

	public void AddStep(BLEToyCodingStep step)
	{
		this.steps.Add(step);
	}

	public ushort[] GetCommands()
	{
		BLEToySequenceQueue.debugCommandList = new List<string>();
		List<ushort> list = new List<ushort>();
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyHighLevelCommand.HL_PLAY_LIST | " + (int)this.song + ")");
		list.Add((ushort)((BLEToyAudioPhraseSong)12288 | this.song));
		int count = this.steps.Count;
		for (int i = 0; i < count; i++)
		{
			BLEToyCodingStep bletoyCodingStep = this.steps[i];
			ushort[] commands = bletoyCodingStep.GetCommands();
			int num = commands.Length;
			for (int j = 0; j < num; j++)
			{
				list.Add(commands[j]);
			}
		}
		Debug.Log("<color=orange>SEQUENCE COMMANDS</color> <color=yellow>" + BLEToySequenceQueue.debugCommandList.Count + "</color>");
		string text = string.Empty;
		foreach (string text2 in BLEToySequenceQueue.debugCommandList)
		{
			text = text + text2 + "\n";
		}
		Debug.Log(text);
		Debug.Log("<color=orange>-------------</color>");
		BLEToySequenceQueue.debugCommandList = new List<string>();
		return list.ToArray();
	}

	public byte[] GetByteArray()
	{
		ushort[] commands = this.GetCommands();
		int num = commands.Length;
		int num2 = num * 2;
		byte[] array = new byte[num2];
		for (int i = 0; i < num; i++)
		{
			int num3 = i * 2;
			byte[] bytes = BitConverter.GetBytes(commands[i]);
			Array.Copy(bytes, 0, array, num3, 2);
		}
		return array;
	}

	private List<BLEToyCodingStep> steps;

	private BLEToyAudioPhraseSong song;
}
