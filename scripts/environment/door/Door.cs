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
		if (body is Entity entity)
		{
			GD.Print("Une entité est entrée dans la porte.");
			EmitSignal(SignalName.EntityEnteredDoor, entity);
			entity.QueueFree();
		}
	}
}
