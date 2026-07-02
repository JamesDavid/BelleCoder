using System;
using System.Collections.Generic;

public class BLEToyCodingAudio
{
	public BLEToyCodingAudio(BLEToyAudioPhraseSong song)
	{
		this.audioPhraseSong = song;
	}

	public BLEToyCodingDogType dogType { get; private set; }

	public BLEToyAudioPhraseSong audioPhraseSong { get; private set; }

	public float duration { get; private set; }

	public ushort[] GetCommands()
	{
		ushort[] array = new ushort[1];
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyHighLevelCommand.HL_PLAY_LIST | " + (int)this.audioPhraseSong + ")");
		array[0] = (ushort)((BLEToyAudioPhraseSong)12288 | this.audioPhraseSong);
		return array;
	}

	private List<BLEToyAudioPhrase> possiblePhrases;
}
