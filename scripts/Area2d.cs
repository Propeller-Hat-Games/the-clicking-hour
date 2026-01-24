using Godot;
using System;

public partial class Area2d : Area2D
{
	
	public CollisionShape2D area;
	public RectangleShape2D rec;
	
	public override void _Ready() {
		area = GetNode<CollisionShape2D>("Box");
		rec = (RectangleShape2D)area.Shape;
	}
	
	public Vector2 AreaSize() {
		return rec.Size;
	}
	
	public Vector2 RandomPosition() {
		var rand = new Random();
		
		Vector2 size = AreaSize();
		Vector2 position = this.Position;
		Vector2 scale = area.Scale;
		
		int amountX = (int)scale.X * rand.Next((int)(position.X - size.X) ,(int)(position.X + size.X));
		int amountY = (int)scale.Y * rand.Next((int)(position.Y - size.Y) ,(int)(position.Y + size.Y));
		
		return new Vector2(amountX , amountY);
	}
}
