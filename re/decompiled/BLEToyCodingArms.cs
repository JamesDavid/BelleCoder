using System;

public class BLEToyCodingArms
{
	public BLEToyCodingArms(BLEToyCamPosition camPosition)
	{
		this.camPosition = camPosition;
	}

	public BLEToyCamPosition camPosition { get; private set; }

	public float duration { get; private set; }

	public ushort[] GetCommands()
	{
		ushort[] array = new ushort[1];
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_M1_GOTO | " + (int)this.camPosition + ")");
		array[0] = (ushort)((BLEToyCamPosition)32768 | this.camPosition);
		return array;
	}

	public static ushort Stop()
	{
		BLEToySequenceQueue.debugCommandList.Add("((int)BLEToyMotorCommand.MOTOR_STOP | (int)BLEToyMotorCommand.MOTOR_M1)");
		return 29184;
	}
}
