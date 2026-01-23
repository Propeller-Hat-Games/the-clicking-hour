using Godot;
using System;
using System.Collections.Generic;

public partial class Board : Sprite2D
{
	
	[Export]
	public Texture2D placeHolder;
	
	[Export]
	public Texture2D[] sprites = new Texture2D[0];
	
	[Export]
	public int amountOfSlots {get;set;}= 1;
	
	private Sprite2D[] slotSprites;
	private Label[] slotLabels;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		slotSprites = new Sprite2D[] {
			GetNode<Sprite2D>("LeftSlot"),
			GetNode<Sprite2D>("MiddleSlot"),
			GetNode<Sprite2D>("RightSlot")
		};
		slotLabels = new Label[] {
			GetNode<Label>("LeftNb"),
			GetNode<Label>("MiddleNb"),
			GetNode<Label>("RightNb")
		};

		foreach (var slot in slotSprites) slot.Visible = false;
		foreach (var label in slotLabels) label.Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}

	private async void Flash(CanvasItem item, bool appearing)
	{
		if (item == null || !IsInstanceValid(item)) return;

		float[] opacities;
		if (appearing)
		{
			opacities = new float[] { 0.0f, 0.25f, 0.0f, 0.5f, 0.0f, 0.75f, 0.0f, 1.0f };
			item.Visible = true;
		}
		else
		{
			opacities = new float[] { 1.0f, 0.0f, 0.75f, 0.0f, 0.5f, 0.0f, 0.25f, 0.0f };
		}

		foreach (float opacity in opacities)
		{
			if (!IsInstanceValid(item)) return;
			item.Modulate = new Color(item.Modulate.R, item.Modulate.G, item.Modulate.B, opacity);
			await ToSignal(GetTree().CreateTimer(0.05f), "timeout");
		}

		if (!IsInstanceValid(item)) return;
		if (!appearing)
		{
			item.Visible = false;
		}
		else
		{
			item.Modulate = new Color(item.Modulate.R, item.Modulate.G, item.Modulate.B, 1.0f);
		}
	}
	
	public void AddASlot() {
		amountOfSlots++;
	}
	
	public void ChangeDisplay() {
		for (int i = 0; i < slotSprites.Length; i++)
		{
			bool shouldBeVisible = i < amountOfSlots;
			bool wasVisible = slotSprites[i].Visible;
			UpdateSlotVisibility(slotSprites[i], slotLabels[i], shouldBeVisible, wasVisible);
		}
	}

	public void UpdateSlotVisibility(Sprite2D slot, Label label, bool shouldBeVisible, bool wasVisible)
	{
		if (shouldBeVisible && !wasVisible)
		{
			Flash(slot, true);
			Flash(label, true);
		}
		else if (!shouldBeVisible && wasVisible)
		{
			Flash(slot, false);
			Flash(label, false);
		}
		else
		{
			slot.Visible = shouldBeVisible;
			label.Visible = shouldBeVisible;
			if (shouldBeVisible)
			{
				slot.Modulate = new Color(slot.Modulate.R, slot.Modulate.G, slot.Modulate.B, 1.0f);
				label.Modulate = new Color(label.Modulate.R, label.Modulate.G, label.Modulate.B, 1.0f);
			}
		}
	}

	public void ClearDisplay() {
		for (int i = 0; i < slotSprites.Length; i++) {
			if (slotSprites[i].Visible) {
				Flash(slotSprites[i], false);
				Flash(slotLabels[i], false);
			}
		}
	}
	
	public void ChangeImages(int[] indexes) {
		ChangeDisplay();
		
		if (indexes == null) return;
		
		for (int i = 0; i < slotSprites.Length; i++)
		{
			if (slotSprites[i].Visible && i < indexes.Length)
			{
				int spriteIdx = indexes[i];
				if (spriteIdx >= 0 && spriteIdx < sprites.Length)
					slotSprites[i].Texture = sprites[spriteIdx];
			}
		}
	}

	public void UpdateCounts(int[] counts) {
		if (counts == null) return;
		
		for (int i = 0; i < slotSprites.Length; i++)
		{
			if (slotLabels[i].Visible && i < counts.Length)
			{
				int count = counts[i];
				if (count == 0 && slotLabels[i].Text != "0" && slotLabels[i].Text != "")
				{
					Flash(slotLabels[i], false);
					Flash(slotSprites[i], false);
				}
				slotLabels[i].Text = count.ToString();
			}
		}
	}
}
