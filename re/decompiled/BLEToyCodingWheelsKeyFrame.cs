using System;
using System.Runtime.InteropServices;
using UnityEngine;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BLEToyCodingWheelsKeyFrame
{
	public BLEToyCodingWheelsKeyFrame(float time, BLEToyMotor motor, BLEToyMotorCommand command, float power = 0f)
	{
		this.time = time;
		this.motor = motor;
		this.command = command;
		this.power = Mathf.Clamp(power, -1f, 1f);
	}

	public float time { get; private set; }

	public BLEToyMotor motor { get; private set; }

	public BLEToyMotorCommand command { get; private set; }

	public float power { get; private set; }
}
