using System;
using UnityEngine;
using UnityEngine.UI;

public class BLEToyAPITestSectionHLSequences : BLEToyAPITestSection
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
		int length = Enum.GetValues(typeof(BLEToyHLSequence)).Length;
		this.buttons = new Button[length];
		for (int j = 0; j < length; j++)
		{
			BLEToyHLSequence sequence = (BLEToyHLSequence)j;
			string text = sequence.ToString();
			Button button2 = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, this.contentHolder);
			button2.transform.localScale = Vector3.one;
			button2.gameObject.name = text;
			button2.GetComponentInChildren<Text>().text = text;
			button2.onClick.AddListener(delegate
			{
				this.HandleButton(sequence);
			});
			this.buttons[j] = button2;
		}
	}

	private void HandleButton(BLEToyHLSequence sequence)
	{
		float num = this.api.sequenceQueue.PlayHLSequence(sequence);
		this.infoText.text = num.ToString() + " Seconds";
	}

	public Button buttonPrefab;

	public Transform contentHolder;

	public Text infoText;

	private Button[] buttons;
}
