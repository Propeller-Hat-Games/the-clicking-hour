using Godot;
using System;
using System.Collections.Generic;

public partial class Board : Sprite2D
{
	
	[Export]
	public CompressedTexture2D placeHolder;
	
	[Export]
	public CompressedTexture2D[] sprites = new CompressedTexture2D[0];
	
	[Export]
	public int amountOfSlots {get;set;}= 1;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ChangeImages([0,1,2]);
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
		
		if (amountOfSlots == 1) {
			leftSlot.Visible = false;
			middleSlot.Visible = true;
			rightSlot.Visible = false;
		}
		else if (amountOfSlots == 2) {
			leftSlot.Visible = true;
			middleSlot.Visible = false;
			rightSlot.Visible = true;
		}
		else {
			middleSlot.Visible = true;
		}
	}
	
	public void ChangeImages(int[] indexes) {
		ChangeDisplay();
		
		Sprite2D leftSlot = GetNode<Sprite2D>("LeftSlot");
		Sprite2D middleSlot = GetNode<Sprite2D>("MiddleSlot");
		Sprite2D rightSlot = GetNode<Sprite2D>("RightSlot");
		
		int availableIndex = 0;
		
		if (leftSlot.Visible) {
			leftSlot.Texture = sprites[indexes[availableIndex++]];
		}
		if (middleSlot.Visible) {
			middleSlot.Texture = sprites[indexes[availableIndex++]];
		}
		if (rightSlot.Visible) {
			rightSlot.Texture = sprites[indexes[availableIndex++]];
		}
	}
}
