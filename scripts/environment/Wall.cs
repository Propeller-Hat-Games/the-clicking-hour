using Godot;
using System;

public partial class Wall : Node2D
{
	public override void _Ready()
	{
		var sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (sprite != null)
		{
			sprite.Play();
		}
	}
}
