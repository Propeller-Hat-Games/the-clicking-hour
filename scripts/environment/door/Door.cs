using Godot;
using System;

public partial class Door : Sprite2D
{
	// Signal émis lorsqu'une entité entre dans la zone de la porte
	[Signal]
	public delegate void EntityEnteredDoorEventHandler(Entity entity);

	public override void _Ready()
	{
		AddToGroup("Door");
	}

	private void _on_body_entered(Node2D body)
	{
		if (body is Entity entity)
		{
			GD.Print("Une entité est entrée dans la porte.");
			EmitSignal(nameof(EntityEnteredDoorEventHandler), entity);
			// TODO: Gérer la logique de sortie de l'entité
		}
	}
}
