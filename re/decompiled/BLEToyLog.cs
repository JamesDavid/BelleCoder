using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

public class BLEToyLog
{
	public BLEToyLog()
	{
		this.bufferDebug = new List<byte>();
		this.bufferLog = new List<byte>();
		this.bufferError = new List<byte>();
	}

	public string logAll { get; private set; }

	public string logDebug { get; private set; }

	public string logLog { get; private set; }

	public string logError { get; private set; }

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnDebug;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnLog;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<string> OnError;

	public void ProcessDebug(byte[] data)
	{
		int num = data.Length;
		for (int i = 1; i < num; i++)
		{
			if (data[i] == 0)
			{
				if (this.bufferDebug.Count > 0)
				{
					byte[] array = this.bufferDebug.ToArray();
					string text = Encoding.ASCII.GetString(array);
					text = "[TOY] Debug: " + text + "\n";
					this.logDebug += text;
					this.logAll += text;
					BLEToyAPI.instance.Log(BLEToyLogLevel.Debug, text);
					if (this.OnDebug != null)
					{
						this.OnDebug(text);
					}
					this.bufferDebug = new List<byte>();
				}
			}
			else
			{
				this.bufferDebug.Add(data[i]);
			}
		}
	}

	public void ProcessLog(byte[] data)
	{
		int num = data.Length;
		for (int i = 1; i < num; i++)
		{
			if (data[i] == 0)
			{
				if (this.bufferLog.Count > 0)
				{
					byte[] array = this.bufferLog.ToArray();
					string text = Encoding.ASCII.GetString(array);
					text = "[TOY] Log: " + text + "\n";
					this.logLog += text;
					this.logAll += text;
					BLEToyAPI.instance.Log(BLEToyLogLevel.Debug, text);
					if (this.OnLog != null)
					{
						this.OnLog(text);
					}
					this.bufferLog = new List<byte>();
				}
			}
			else
			{
				this.bufferLog.Add(data[i]);
			}
		}
	}

	public void ProcessError(byte[] data)
	{
		int num = data.Length;
		for (int i = 1; i < num; i++)
		{
			if (data[i] == 0)
			{
				if (this.bufferError.Count > 0)
				{
					byte[] array = this.bufferError.ToArray();
					string text = Encoding.ASCII.GetString(array);
					text = "[TOY] Error: " + text + "\n";
					this.logError += text;
					this.logAll += text;
					BLEToyAPI.instance.Log(BLEToyLogLevel.Warning, text);
					if (this.OnError != null)
					{
						this.OnError(text);
					}
					this.bufferError = new List<byte>();
				}
			}
			else
			{
				this.bufferError.Add(data[i]);
			}
		}
	}

	private List<byte> bufferDebug;

	private List<byte> bufferLog;

	private List<byte> bufferError;
}
