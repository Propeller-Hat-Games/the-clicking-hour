using Godot;
using System;

public partial class GameManager : Node2D {
	private Godot.Collections.Array<Sprite2D> glassSprites;

	public override void _Ready() {
		var glassScene = GD.Load<PackedScene>("res://scenes/glass.tscn");
		var glassInstance = glassScene.Instantiate<Glass>();
		AddChild(glassInstance);

		glassSprites = glassInstance.GetSprites();
		
		GD.Print("Loaded sprites:");
		foreach (var sprite in glassSprites) {
			GD.Print($"- {sprite.Name}");
		}
	}
}
