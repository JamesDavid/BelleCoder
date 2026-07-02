using System;

public class BLEToy
{
	public BLEToy(string peripheralId, string peripheralName)
	{
		this.id = peripheralId;
		this.name = peripheralName;
		this.connected = false;
		this.rssi = -9999;
	}

	public string id { get; private set; }

	public string name { get; private set; }

	public bool connected { get; private set; }

	public int rssi { get; private set; }

	public void SetConnected(bool c)
	{
		this.connected = c;
	}

	public void SetRSSI(int i)
	{
		this.rssi = i;
	}
}
