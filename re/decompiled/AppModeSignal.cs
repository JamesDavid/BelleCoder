using System;

public enum AppModeSignal
{
	ChecksumROM = 128,
	ChecksumFlash,
	GetFlashVersionCode,
	EndAppMode = 140,
	RCAppModeKeepAlive,
	EnterLowPowerSleepMode = 145
}
