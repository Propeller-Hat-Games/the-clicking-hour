using Godot;
using System;

public partial class Area2d : Area2D
{
	public Vector2 AreaSize() {
		CollisionShape2D area = GetNode<CollisionShape2D>("Box");
		RectangleShape2D rec = (RectangleShape2D)area.Shape;
		return rec.Size;
	}
}
