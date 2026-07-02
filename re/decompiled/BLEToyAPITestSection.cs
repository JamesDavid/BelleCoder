using System;
using UnityEngine;

public class BLEToyAPITestSection : MonoBehaviour
{
	protected virtual void OnEnable()
	{
		this.api = BLEToyAPI.instance;
	}

	protected virtual void OnDisable()
	{
	}

	protected BLEToyAPI api;
}
