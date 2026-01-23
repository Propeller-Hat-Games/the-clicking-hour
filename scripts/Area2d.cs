using Godot;
using System;

public partial class Area2d : Area2D
{
	public Vector2 AreaSize() {
		CollisionShape2D area = GetNode<CollisionShape2D>("Box");
		if (area.Shape is RectangleShape2D rec)
		{
			return rec.Size;
		}
		return Vector2.Zero;
	}
}
