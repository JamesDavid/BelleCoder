using System;

public enum AppToToyOpCodes
{
	PlayAudioSequence = 16,
	EnqueueSequenceCommand,
	StopSequenceQueuePlayback,
	CheckSequenceQueueFreeSpace,
	MotorRun,
	SetLED,
	PlaySequence = 23,
	StopSequence,
	Motor1Goto,
	RequestInputSate = 32,
	RequestLVDValue,
	CheckToyVolume,
	SetToyVolume,
	SetPassword = 64,
	SecurityChallengeResponse,
	AppModeSignal = 80
}
