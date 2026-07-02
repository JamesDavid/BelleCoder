using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BLEToyAPITestSectionVolume : BLEToyAPITestSection
{
	private void Awake()
	{
		this.volumeSlider.minValue = 1f;
		this.volumeSlider.maxValue = 15f;
		this.AdjustUI();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.checkButton.onClick.AddListener(new UnityAction(this.HandleCheckButton));
		this.setButton.onClick.AddListener(new UnityAction(this.HandleSetButton));
		this.soundTestButton.onClick.AddListener(new UnityAction(this.HandleSoundTestButton));
		this.playMusicButton.onClick.AddListener(new UnityAction(this.HandlePlayMusicButton));
		this.fadeOutButton.onClick.AddListener(new UnityAction(this.HandleFadeOutButton));
		this.api.OnToyVolume += this.HandleVolume;
		this.volumeSlider.onValueChanged.AddListener(new UnityAction<float>(this.HandleSliderChanged));
		this.AdjustUI();
		this.api.CheckToyVolume();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.checkButton.onClick.RemoveAllListeners();
		this.setButton.onClick.RemoveAllListeners();
		this.soundTestButton.onClick.RemoveAllListeners();
		this.playMusicButton.onClick.RemoveAllListeners();
		this.fadeOutButton.onClick.RemoveAllListeners();
	}

	private void HandleCheckButton()
	{
		this.api.CheckToyVolume();
	}

	private void HandleSoundTestButton()
	{
		this.api.PlayAudioSequence(BLEToyAudioPhrase.eP001);
	}

	private void HandlePlayMusicButton()
	{
		this.api.PlayAudioSequence(BLEToyAudioPhrase.eP282);
	}

	private void HandleFadeOutButton()
	{
		base.StartCoroutine(this.FadeOutRoutine());
	}

	private IEnumerator FadeOutRoutine()
	{
		float time = 3f;
		int i = this.volume;
		for (int j = 0; j < i; j++)
		{
			this.volume = i - j;
			this.AdjustUI();
			this.HandleSetButton();
			yield return new WaitForSeconds(time / (float)i);
		}
		yield break;
	}

	private void HandleSetButton()
	{
		this.volume = (int)this.volumeSlider.value;
		this.api.SetToyVolume(this.volume);
	}

	private void HandleSliderChanged(float sliderValue)
	{
		this.volume = (int)sliderValue;
		this.AdjustUI();
	}

	private void HandleVolume(int volume)
	{
		this.volume = volume;
		this.AdjustUI();
	}

	private void AdjustUI()
	{
		this.volumeSlider.value = (float)this.volume;
		this.volumeSliderValue.text = this.volume.ToString();
	}

	public Button checkButton;

	public Button setButton;

	public Button soundTestButton;

	public Button playMusicButton;

	public Button fadeOutButton;

	public Slider volumeSlider;

	public Text volumeSliderValue;

	private int volume = 15;
}
