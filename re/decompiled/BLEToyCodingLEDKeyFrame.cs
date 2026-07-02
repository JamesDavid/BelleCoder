using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BLEToyCodingLEDKeyFrame
{
	public BLEToyCodingLEDKeyFrame(float time, Color color, float fadeTime)
	{
		this.time = time;
		this.color = color;
		this.fadeTime = fadeTime;
	}

	public float time { get; private set; }

	public Color color { get; private set; }

	public float fadeTime { get; private set; }
}
