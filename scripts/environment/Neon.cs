using Godot;
using System;

public partial class Neon : Node2D
{
	[Export]
	public Sprite2D NeonOnSprite;
	[Export]
	public Sprite2D NeonOffSprite;

	public override async void _Ready()
	{
		while (true) {
			NeonOnSprite.Visible = true;
			NeonOffSprite.Visible = false;
			await ToSignal(GetTree().CreateTimer(GD.RandRange(1f, 5f), false), "timeout");

			NeonOnSprite.Visible = false;
			NeonOffSprite.Visible = true;
			await ToSignal(GetTree().CreateTimer(GD.RandRange(0.5f, 1.5f), false), "timeout");
		}
	}
}
