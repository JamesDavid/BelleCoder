using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITestSectionAudioPhrasesSongs : BLEToyAPITestSection
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.AdjustUI();
		this.fadeOutButton.onClick.AddListener(new UnityAction(this.HandleFadeOutButton));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.fadeOutButton.onClick.RemoveAllListeners();
	}

	private void AdjustUI()
	{
		if (this.buttons != null)
		{
			foreach (Button button in this.buttons)
			{
				if (button != null)
				{
					button.onClick.RemoveAllListeners();
					global::UnityEngine.Object.Destroy(button.gameObject);
				}
			}
		}
		int length = Enum.GetValues(typeof(BLEToyAudioPhraseSong)).Length;
		this.buttons = new Button[length];
		for (int j = 0; j < length; j++)
		{
			BLEToyAudioPhraseSong song = (BLEToyAudioPhraseSong)Enum.GetValues(typeof(BLEToyAudioPhraseSong)).GetValue(j);
			string text = song.ToString();
			Button button2 = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, this.contentHolder);
			button2.transform.localScale = Vector3.one;
			button2.gameObject.name = text;
			button2.GetComponentInChildren<Text>().text = text;
			button2.onClick.AddListener(delegate
			{
				this.HandleButton(song);
			});
			this.buttons[j] = button2;
		}
	}

	private void HandleButton(BLEToyAudioPhraseSong song)
	{
		this.api.PlayLoopingSong(song, 8);
	}

	private void HandleFadeOutButton()
	{
		this.api.FadeOutMusic(3f);
	}

	public Button buttonPrefab;

	public Transform contentHolder;

	public Button fadeOutButton;

	private Button currentTab;

	private Button[] buttons;
}
