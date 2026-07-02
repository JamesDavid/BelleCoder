using System;
using UnityEngine;
using UnityEngine.UI;

public class BLEToyAPITestSectionAudioPhrases : BLEToyAPITestSection
{
	protected override void OnEnable()
	{
		base.OnEnable();
		this.AdjustUI();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
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
		int length = Enum.GetValues(typeof(BLEToyAudioPhrase)).Length;
		this.buttons = new Button[length];
		for (int j = 0; j < length; j++)
		{
			BLEToyAudioPhrase audioPhrase = (BLEToyAudioPhrase)j;
			string text = audioPhrase.ToString();
			Button button2 = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, this.contentHolder);
			button2.transform.localScale = Vector3.one;
			button2.gameObject.name = text;
			button2.GetComponentInChildren<Text>().text = text;
			button2.onClick.AddListener(delegate
			{
				this.HandleButton(audioPhrase);
			});
			this.buttons[j] = button2;
		}
	}

	private void HandleButton(BLEToyAudioPhrase audioPhrase)
	{
		this.api.PlayAudioSequence(audioPhrase);
	}

	public Button buttonPrefab;

	public Transform contentHolder;

	private Button currentTab;

	private Button[] buttons;
}
