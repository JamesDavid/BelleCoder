using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

public class PlaySequence : MonoBehaviour
{
	private void Awake()
	{
		this.item = base.GetComponent<tk2dUIItem>();
		this.playButton = base.GetComponent<BasePlayButton>();
		this.playDrawnLine = base.GetComponent<PlayDrawnLine>();
	}

	private void Start()
	{
		this.codingBook = global::UnityEngine.Object.FindObjectOfType<CodingBook>();
		this.trayScroll = global::UnityEngine.Object.FindObjectOfType<TrayScroll>();
	}

	private void OnEnable()
	{
		this.item.OnClick += this.OnClick;
		PlayDrawnLine playDrawnLine = this.playDrawnLine;
		playDrawnLine.OnStop = (PlayDrawnLine.OnStopDelegate)Delegate.Combine(playDrawnLine.OnStop, new PlayDrawnLine.OnStopDelegate(this.HandleSequenceQueueEnded));
	}

	private void OnDisable()
	{
		this.item.OnClick -= this.OnClick;
		PlayDrawnLine playDrawnLine = this.playDrawnLine;
		playDrawnLine.OnStop = (PlayDrawnLine.OnStopDelegate)Delegate.Remove(playDrawnLine.OnStop, new PlayDrawnLine.OnStopDelegate(this.HandleSequenceQueueEnded));
	}

	private void Update()
	{
		if (this.isPlaying != this.wasPlaying)
		{
			this.wasPlaying = this.isPlaying;
			this.progressStepIndex = -1;
			if (!this.isPlaying)
			{
				this.progressGlowSprite.transform.position = new Vector3(2000f, 0f, 0f);
			}
		}
		if (this.isPlaying && this.playDrawnLine.currentStepIndex != this.progressStepIndex)
		{
			int currentStepIndex = this.playDrawnLine.currentStepIndex;
			this.progressStepIndex = currentStepIndex;
			string text;
			if (this.codingBook.containerList[currentStepIndex].assignedCommand.GetType() == typeof(CombinedBlockCommand))
			{
				text = "glow-combo";
			}
			else if (this.codingBook.containerList[currentStepIndex].assignedCommand.shape == CommandShape.round)
			{
				text = "glow-circle";
			}
			else
			{
				text = "glow-hex";
			}
			this.setProgressGlowTo(this.codingBook.containerList[currentStepIndex].transform.position + new Vector3(0f, 0f, 1f), text);
		}
	}

	private void setProgressGlowTo(Vector3 position, string spriteName)
	{
		this.glowTween.Kill(false);
		this.glowTween = DOTween.To(() => this.progressGlowSprite.color.a, delegate(float x)
		{
			this.progressGlowSprite.color = new Color(this.progressGlowSprite.color.r, this.progressGlowSprite.color.g, this.progressGlowSprite.color.b, x);
		}, 0f, 0.25f).OnComplete(delegate
		{
			this.progressGlowSprite.SetSprite(spriteName);
			this.progressGlowSprite.transform.position = position;
			this.glowTween = DOTween.To(() => this.progressGlowSprite.color.a, delegate(float x)
			{
				this.progressGlowSprite.color = new Color(this.progressGlowSprite.color.r, this.progressGlowSprite.color.g, this.progressGlowSprite.color.b, x);
			}, 1f, 0.25f);
		});
	}

	private void OnClick()
	{
		if (this.isPlaying)
		{
			this.isPlaying = false;
			if (this.InputBlocker != null)
			{
				this.InputBlocker.SetActive(false);
			}
			this.trayScroll.state = TrayScroll.State.TopIcons;
			this.playButton.OnStopPlaying();
			this.playDrawnLine.stopDrivingDoll();
		}
		else
		{
			BLEToyAPI.instance.ClearMusicFadeOutComplete();
			this.isPlaying = true;
			if (this.InputBlocker != null)
			{
				this.InputBlocker.SetActive(true);
			}
			this.trayScroll.state = TrayScroll.State.Rose;
			this.sendQueueToToy();
			int num = 0;
			for (int i = 0; i < this.codingBook.containerList.Length; i++)
			{
				if (this.codingBook.containerList[i].assignedCommand != null && this.codingBook.containerList[i].assignedCommand.shape != CommandShape.none)
				{
					num++;
				}
			}
			AnalyticsManager.TagEvent("Dance Coding - Played Sequence", new Dictionary<string, string> { 
			{
				"Sequence Length",
				num.ToString()
			} });
			TutorialManager.instance.HideTutorial();
		}
	}

	private void HandleSequenceQueueEnded()
	{
		this.isPlaying = false;
		if (this.InputBlocker != null)
		{
			this.InputBlocker.SetActive(false);
		}
		this.setProgressGlowTo(new Vector3(2000f, 0f, 0f), "glow-circle");
		this.trayScroll.state = TrayScroll.State.TopIcons;
		this.playButton.OnStopPlaying();
	}

	private void sendQueueToToy()
	{
		List<List<wheelSteps>> list = new List<List<wheelSteps>>();
		for (int i = 0; i < this.codingBook.containerList.Length; i++)
		{
			if (this.codingBook.containerList[i].assignedCommand == null || this.codingBook.containerList[i].assignedCommand == this.codingBook.phantomCommand)
			{
				break;
			}
			if (this.codingBook.containerList[i].assignedCommand.GetType() == typeof(CombinedBlockCommand))
			{
				list.Add(this.commandToSteps(this.codingBook.containerList[i].assignedCommand as CombinedBlockCommand));
			}
			else
			{
				list.Add(this.commandToSteps(this.codingBook.containerList[i].assignedCommand));
			}
		}
		this.playDrawnLine.stepList = list;
		this.playDrawnLine.song = this.selectedSong;
		this.playDrawnLine.Play();
		this.progressStepIndex = -1;
	}

	private List<wheelSteps> commandToSteps(CombinedBlockCommand command)
	{
		List<wheelSteps> list = new List<wheelSteps>();
		List<wheelSteps> list2 = this.typeToSteps(command.type);
		List<wheelSteps> list3 = this.typeToSteps(command.secondaryType);
		for (int i = 0; i < list2.Count; i++)
		{
			list.Add(list2[i]);
		}
		for (int i = 0; i < list3.Count; i++)
		{
			list.Add(list3[i]);
		}
		return list;
	}

	private List<wheelSteps> commandToSteps(BlockCommand command)
	{
		return this.typeToSteps(command.type);
	}

	private List<wheelSteps> typeToSteps(CommandType type)
	{
		List<wheelSteps> list = new List<wheelSteps>();
		switch (type)
		{
		case CommandType.Forward:
			list.Add(new wheelSteps(2, 2, 0));
			break;
		case CommandType.Backward:
			list.Add(new wheelSteps(-2, -2, 0));
			break;
		case CommandType.ClockwiseFull:
			list.Add(new wheelSteps(-12, 12, 0));
			break;
		case CommandType.CounterFull:
			list.Add(new wheelSteps(12, -12, 0));
			break;
		case CommandType.ClockwiseHalf:
			list.Add(new wheelSteps(-6, 6, 0));
			break;
		case CommandType.CounterHalf:
			list.Add(new wheelSteps(6, -6, 0));
			break;
		case CommandType.ArmsDown:
			list.Add(new wheelSteps(0, 0, 1));
			break;
		case CommandType.ArmsOut:
			list.Add(new wheelSteps(0, 0, 2));
			break;
		case CommandType.ArmsForward:
			list.Add(new wheelSteps(0, 0, 3));
			break;
		case CommandType.LeftArmOut:
			list.Add(new wheelSteps(0, 0, 4));
			break;
		case CommandType.LeftArmUp:
			list.Add(new wheelSteps(0, 0, 5));
			break;
		case CommandType.ArmsUp:
			list.Add(new wheelSteps(0, 0, 6));
			break;
		case CommandType.LeftArmDown:
			list.Add(new wheelSteps(0, 0, 7));
			break;
		}
		return list;
	}

	public tk2dSprite progressGlowSprite;

	private bool isPlaying;

	private bool wasPlaying;

	private tk2dUIItem item;

	private CodingBook codingBook;

	private TrayScroll trayScroll;

	private BasePlayButton playButton;

	private PlayDrawnLine playDrawnLine;

	private int progressStepIndex = -1;

	public BLEToyAudioPhraseSong selectedSong;

	public GameObject InputBlocker;

	private Tween glowTween;
}
