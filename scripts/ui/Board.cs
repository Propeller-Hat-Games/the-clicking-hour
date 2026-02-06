using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Displays the current required glass orders.
/// </summary>
public partial class Board : Sprite2D
{
	[Export]
	private Sprite2D LeftSlot;
	[Export]
	private Sprite2D MiddleSlot;
	[Export]
	private Sprite2D RightSlot;
	[Export]
	private Label LeftNb;
	[Export]
	private Label MiddleNb;
	[Export]
	private Label RightNb;
	[Export]
	private PointLight2D Light;

	private int AmountLeft { get; set; } = 0;
	private int AmountMiddle { get; set; } = 0;
	private int AmountRight { get; set; } = 0;

	/// <summary>
	/// Initializes the board, registers the light, and starts a floating animation.
	/// </summary>
	public override void _Ready()
	{
		if (Light != null)
		{
			Light.AddToGroup("Lights");
		}

		// Initial state: hide all slots and labels
		Sprite2D[] slots = { LeftSlot, MiddleSlot, RightSlot };
		Label[] labels = { LeftNb, MiddleNb, RightNb };
		foreach (var slot in slots) if (slot != null) slot.Visible = false;
		foreach (var label in labels) if (label != null) label.Visible = false;

		// Setup floating animation with Tween
		Tween tween = CreateTween().SetLoops();
		tween.TweenProperty(this, "position:y", Position.Y - 5f, 2.0f)
			 .SetTrans(Tween.TransitionType.Sine)
			 .SetEase(Tween.EaseType.InOut);
		tween.TweenProperty(this, "position:y", Position.Y + 5f, 2.0f)
			 .SetTrans(Tween.TransitionType.Sine)
			 .SetEase(Tween.EaseType.InOut);
	}

	/// <summary>
	/// Performs a flickering flash animation on a canvas item.
	/// </summary>
	/// <param name="item">The item to animate.</param>
	/// <param name="appearing">True if the item is appearing, false if it's disappearing.</param>
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
			
			await ToSignal(GetTree().CreateTimer(0.05f), Timer.SignalName.Timeout);
			if (!IsInsideTree()) return;
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

	/// <summary>
	/// Updates the board slots with new glass textures and counts.
	/// </summary>
	/// <param name="slotCount">Number of active slots.</param>
	/// <param name="sprites">List of Sprite2D templates for textures.</param>
	/// <param name="counts">List of required counts for each slot.</param>
	public void UpdateBoard(int slotCount, List<Sprite2D> sprites, List<int> counts)
	{
		Sprite2D[] slots = { LeftSlot, MiddleSlot, RightSlot };
		Label[] labels = { LeftNb, MiddleNb, RightNb };
		int[] currentAmounts = { AmountLeft, AmountMiddle, AmountRight };

		for (int i = 0; i < slots.Length; i++)
		{
			if (i < slotCount)
			{
				// Ensure slots and sprites are valid before accessing
				if (slots[i] != null && i < sprites.Count && sprites[i] != null)
				{
					slots[i].Texture = sprites[i].Texture;
					
					// Flash logic based on state change
					if (currentAmounts[i] == 0 && counts[i] > 0) 
					{ 
						Flash(slots[i], true);
					}
					else if (currentAmounts[i] > 0 && counts[i] == 0) 
					{ 
						Flash(slots[i], false);
					}
				}

				// Ensure labels are valid before accessing
				if (labels[i] != null && i < counts.Count)
				{
					labels[i].Text = counts[i].ToString();

					if (currentAmounts[i] == 0 && counts[i] > 0) 
					{ 
						Flash(labels[i], true);
					}
					else if (currentAmounts[i] > 0 && counts[i] == 0) 
					{ 
						Flash(labels[i], false);
					}
				}
			}
			else
			{
				// Hide unused slots and labels
				if (slots[i] != null) slots[i].Visible = false;
				if (labels[i] != null) labels[i].Visible = false;
			}
		}

		AmountLeft = slotCount > 0 ? counts[0] : 0;
		AmountMiddle = slotCount > 1 ? counts[1] : 0;
		AmountRight = slotCount > 2 ? counts[2] : 0;
	}
}
