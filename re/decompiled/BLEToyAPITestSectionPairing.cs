using System;
using UnityEngine.UI;

public class BLEToyAPITestSectionPairing : BLEToyAPITestSection
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Button[] array = this.buttons;
		for (int i = 0; i < array.Length; i++)
		{
			Button button = array[i];
			Button clickedButton = button;
			button.onClick.AddListener(delegate
			{
				this.HandleButton(clickedButton);
			});
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		foreach (Button button in this.buttons)
		{
			button.onClick.RemoveAllListeners();
		}
	}

	private void HandleButton(Button clickedButton)
	{
		string name = clickedButton.name;
		if (name != null)
		{
			if (!(name == "ResetAllPairing"))
			{
			}
		}
	}

	public Button[] buttons;
}
