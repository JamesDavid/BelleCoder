using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BLEToyAPITestSectionCustomSequences : BLEToyAPITestSection
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
		this.api.sequenceQueue.OnSequenceQueueEnded += this.HandleSequenceQueueEnded;
		this.SetupSequence();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		foreach (Button button in this.buttons)
		{
			button.onClick.RemoveAllListeners();
		}
		this.api.sequenceQueue.OnSequenceQueueEnded -= this.HandleSequenceQueueEnded;
	}

	private void HandleSequenceQueueEnded()
	{
		Debug.Log("HandleSequenceQueueEnded");
	}

	private void SetupSequence()
	{
		this.step1 = new BLEToyCodingStep();
		this.step1.arms = new BLEToyCodingArms(BLEToyCamPosition.ArmsUp);
		this.step1.wheels = new BLEToyCodingWheels(BLEToyCodingMovement.StepBackward);
		this.step2 = new BLEToyCodingStep();
		this.step2.arms = new BLEToyCodingArms(BLEToyCamPosition.ArmsForward);
		this.step2.wheels = new BLEToyCodingWheels(BLEToyCodingMovement.StepForward);
		this.step3 = new BLEToyCodingStep();
		this.step3.arms = new BLEToyCodingArms(BLEToyCamPosition.LeftArmDown);
		this.step3.wheels = new BLEToyCodingWheels(BLEToyCodingMovement.TurnCW360);
		this.step4 = new BLEToyCodingStep();
		this.step4.wheels = new BLEToyCodingWheels(BLEToyCodingMovement.TurnCCW360);
		this.step5 = new BLEToyCodingStep();
		this.step5.wheels = new BLEToyCodingWheels(BLEToyCodingMovement.TurnCW180);
		this.step6 = new BLEToyCodingStep();
		this.step6.wheels = new BLEToyCodingWheels(BLEToyCodingMovement.TurnCCW180);
	}

	private void SequenceExample()
	{
		BLEToyCodingSequence bletoyCodingSequence = new BLEToyCodingSequence();
		bletoyCodingSequence.SetSong(BLEToyAudioPhraseSong.BeOurGuestA);
		bletoyCodingSequence.AddStep(new BLEToyCodingStep
		{
			wheels = new BLEToyCodingWheels(BLEToyCodingMovement.StepBackward)
		});
		bletoyCodingSequence.AddStep(new BLEToyCodingStep
		{
			wheels = new BLEToyCodingWheels(BLEToyCodingMovement.TurnCCW360)
		});
		bletoyCodingSequence.AddStep(new BLEToyCodingStep
		{
			wheels = new BLEToyCodingWheels(BLEToyCodingMovement.StepForward)
		});
		ushort[] commands = bletoyCodingSequence.GetCommands();
		this.api.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
		this.api.sequenceQueue.AddCommands(commands);
	}

	private void HandleButton(Button clickedButton)
	{
		string name = clickedButton.name;
		BLEToyCodingSequence bletoyCodingSequence;
		ushort[] array;
		if (name != null)
		{
			if (name == "Step1")
			{
				bletoyCodingSequence = new BLEToyCodingSequence();
				bletoyCodingSequence.SetSong(BLEToyAudioPhraseSong.TaleAsOldAsTimeB);
				bletoyCodingSequence.AddStep(this.step1);
				array = bletoyCodingSequence.GetCommands();
				this.api.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
				this.api.sequenceQueue.AddCommands(array);
				return;
			}
			if (name == "Step2")
			{
				bletoyCodingSequence = new BLEToyCodingSequence();
				bletoyCodingSequence.SetSong(BLEToyAudioPhraseSong.BeOurGuestA);
				bletoyCodingSequence.AddStep(this.step2);
				array = bletoyCodingSequence.GetCommands();
				this.api.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
				this.api.sequenceQueue.AddCommands(array);
				return;
			}
			if (name == "Step3")
			{
				bletoyCodingSequence = new BLEToyCodingSequence();
				bletoyCodingSequence.SetSong(BLEToyAudioPhraseSong.SomethingThereA);
				bletoyCodingSequence.AddStep(this.step3);
				array = bletoyCodingSequence.GetCommands();
				this.api.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
				this.api.sequenceQueue.AddCommands(array);
				return;
			}
			if (!(name == "All"))
			{
				if (name == "Music")
				{
					array = new List<ushort> { 12540, 10272, 12540, 10272, 0 }.ToArray();
					this.api.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
					this.api.sequenceQueue.AddCommands(array);
					return;
				}
			}
		}
		bletoyCodingSequence = new BLEToyCodingSequence();
		bletoyCodingSequence.SetSong(BLEToyAudioPhraseSong.OriginalSong6);
		bletoyCodingSequence.AddStep(this.step1);
		bletoyCodingSequence.AddStep(this.step2);
		bletoyCodingSequence.AddStep(this.step3);
		bletoyCodingSequence.AddStep(this.step4);
		bletoyCodingSequence.AddStep(this.step5);
		bletoyCodingSequence.AddStep(this.step6);
		array = bletoyCodingSequence.GetCommands();
		this.api.sequenceQueue.StopAndClear(BLEToyStopSequenceFlag.All);
		this.api.sequenceQueue.AddCommands(array);
	}

	public Button[] buttons;

	private BLEToyCodingStep step1;

	private BLEToyCodingStep step2;

	private BLEToyCodingStep step3;

	private BLEToyCodingStep step4;

	private BLEToyCodingStep step5;

	private BLEToyCodingStep step6;
}
