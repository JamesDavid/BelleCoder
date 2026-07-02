using System;
using UnityEngine;
using UnityEngine.UI;

public class BLEToyAPITestSectionInputs : BLEToyAPITestSection
{
	private void Awake()
	{
		this.normalColor = this.interactionEventPanel.color;
		this.grid = base.transform.Find("Grid");
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		this.api.toyInput.OnInputPressed += this.HandleInputPressed;
		this.api.toyInput.OnInputReleased += this.HandleInputReleased;
		this.api.toyInput.OnInteraction += this.HandleInteraction;
		this.api.RequestInputState();
		this.UpdateInputs();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		this.api.toyInput.OnInputPressed -= this.HandleInputPressed;
		this.api.toyInput.OnInputReleased -= this.HandleInputReleased;
		this.api.toyInput.OnInteraction -= this.HandleInteraction;
	}

	private void Start()
	{
		int length = Enum.GetValues(typeof(BLEToyInputCode)).Length;
		this.buttons = new Button[length];
		for (int i = 0; i < length; i++)
		{
			BLEToyInputCode bletoyInputCode = (BLEToyInputCode)i;
			Button button = global::UnityEngine.Object.Instantiate<Button>(this.buttonPrefab, base.transform);
			button.transform.SetParent(this.grid);
			button.transform.localScale = Vector3.one;
			button.gameObject.name = bletoyInputCode.ToString();
			button.GetComponentInChildren<Text>().text = bletoyInputCode.ToString();
			this.buttons[i] = button;
			button.interactable = this.api.toyInput.InputIsPressed(bletoyInputCode);
		}
	}

	private void Update()
	{
		this.interactionEventPanel.color = Color.Lerp(this.interactionEventPanel.color, this.normalColor, Time.deltaTime);
	}

	private void UpdateInputs()
	{
		if (this.buttons == null)
		{
			return;
		}
		int length = Enum.GetValues(typeof(BLEToyInputCode)).Length;
		for (int i = 0; i < length; i++)
		{
			BLEToyInputCode bletoyInputCode = (BLEToyInputCode)i;
			Button button = this.buttons[i];
			button.interactable = this.api.toyInput.InputIsPressed(bletoyInputCode);
		}
	}

	private void HandleInputPressed(BLEToyInputCode inputCode)
	{
		if (this.buttons == null)
		{
			return;
		}
		this.buttons[(int)inputCode].interactable = true;
	}

	private void HandleInputReleased(BLEToyInputCode inputCode)
	{
		if (this.buttons == null)
		{
			return;
		}
		this.buttons[(int)inputCode].interactable = false;
	}

	private void HandleInteraction(BLEToyToyInteraction interaction)
	{
		this.interactionEventPanel.GetComponentInChildren<Text>().text = "Interaction: " + interaction.ToString();
		this.interactionEventPanel.color = Color.magenta;
	}

	public Image interactionEventPanel;

	public Button buttonPrefab;

	private Button[] buttons;

	private Color normalColor;

	private Transform grid;
}
