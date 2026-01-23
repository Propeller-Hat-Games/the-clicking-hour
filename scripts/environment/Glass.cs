using Godot;
using System;

public partial class Glass : Node2D {
	[Export] private Godot.Collections.Array<Sprite2D> sprites = new();

	public Godot.Collections.Array<Sprite2D> GetSprites() {
		return sprites;
	}
}
