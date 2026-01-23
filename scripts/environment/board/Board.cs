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
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
	
	public void AddASlot() {
		amountOfSlots++;
	}
	
	public void ChangeDisplay() {
		Sprite2D leftSlot = GetNode<Sprite2D>("LeftSlot");
		Sprite2D middleSlot = GetNode<Sprite2D>("MiddleSlot");
		Sprite2D rightSlot = GetNode<Sprite2D>("RightSlot");
		
		leftSlot.Visible = false;
		middleSlot.Visible = false;
		rightSlot.Visible = false;
		
		if (amountOfSlots == 1) {
			middleSlot.Visible = true;
		}
		else if (amountOfSlots == 2) {
			leftSlot.Visible = true;
			rightSlot.Visible = true;
		}
		else if (amountOfSlots >= 3) {
			leftSlot.Visible = true;
			middleSlot.Visible = true;
			rightSlot.Visible = true;
		}
	}
	
	public void ChangeImages(int[] indexes) {
		ChangeDisplay();
		
		if (indexes == null) return;
		
		Sprite2D leftSlot = GetNode<Sprite2D>("LeftSlot");
		Sprite2D middleSlot = GetNode<Sprite2D>("MiddleSlot");
		Sprite2D rightSlot = GetNode<Sprite2D>("RightSlot");
		
		int availableIndex = 0;
		
		if (leftSlot.Visible && availableIndex < indexes.Length) {
			int spriteIdx = indexes[availableIndex++];
			if (spriteIdx >= 0 && spriteIdx < sprites.Length)
				leftSlot.Texture = sprites[spriteIdx];
		}
		if (middleSlot.Visible && availableIndex < indexes.Length) {
			int spriteIdx = indexes[availableIndex++];
			if (spriteIdx >= 0 && spriteIdx < sprites.Length)
				middleSlot.Texture = sprites[spriteIdx];
		}
		if (rightSlot.Visible && availableIndex < indexes.Length) {
			int spriteIdx = indexes[availableIndex++];
			if (spriteIdx >= 0 && spriteIdx < sprites.Length)
				rightSlot.Texture = sprites[spriteIdx];
		}
	}
}
