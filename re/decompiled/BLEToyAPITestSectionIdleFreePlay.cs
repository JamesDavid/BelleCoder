using System;
using UnityEngine.UI;

public class BLEToyAPITestSectionIdleFreePlay : BLEToyAPITestSection
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
		this.api.toyInput.OnInputPressed += this.HandleInputPressed;
		this.api.toyInput.OnInputReleased += this.HandleInputReleased;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		foreach (Button button in this.buttons)
		{
			button.onClick.RemoveAllListeners();
		}
		this.api.toyInput.OnInputPressed -= this.HandleInputPressed;
		this.api.toyInput.OnInputReleased -= this.HandleInputReleased;
	}

	private void HandleInputPressed(BLEToyInputCode inputCode)
	{
		if (inputCode != BLEToyInputCode.NECKLACE_BUTTON_ID)
		{
		}
	}

	private void HandleInputReleased(BLEToyInputCode inputCode)
	{
		if (inputCode != BLEToyInputCode.NECKLACE_BUTTON_ID)
		{
		}
	}

	private void HandleButton(Button clickedButton)
	{
		string name = clickedButton.name;
		if (name != null)
		{
			if (!(name == "StartFreePlay"))
			{
				if (name == "EndFreePlay")
				{
					this.api.EndConnectedFreePlay();
				}
			}
			else
			{
				this.api.StartConnectedFreePlay(true, BLEToyAudioPhrase.eP798);
			}
		}
	}

	public Button[] buttons;
}
