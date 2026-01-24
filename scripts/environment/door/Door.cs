using Godot;
using System;

public partial class Door : Area2D
{
	// Signal émis lorsqu'une entité entre dans la zone de la porte
	[Signal]
	public delegate void EntityEnteredDoorEventHandler(Entity entity);

	public override void _Ready()
	{
		AddToGroup("Door");
		BodyEntered += OnBodyEntered;
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Entity entity && entity.IsAlive())
		{
			GD.Print("Une entité est entrée dans la porte.");
			EmitSignal(SignalName.EntityEnteredDoor, entity);
			// On ne QueueFree pas ici car le GameManager peut en avoir besoin pour finir la vague
			// ou pour jouer l'animation de disparition.
			// L'entité sera nettoyée par le GameManager ou par sa propre logique.
		}
	}
}
