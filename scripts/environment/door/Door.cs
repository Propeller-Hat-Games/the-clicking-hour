using Godot;
using System;

public partial class Door : Area2D
{
	[Export]
	public Texture2D OpenTexture { get; set; }
	[Export]
	public Texture2D ClosedTexture { get; set; }

	private Sprite2D _sprite;

	[Signal]
	public delegate void EntityEnteredDoorEventHandler(Entity entity);

	public override void _Ready()
	{
		AddToGroup("Door");
		BodyEntered += OnBodyEntered;
		_sprite = GetNode<Sprite2D>("Sprite");

		if (OpenTexture == null)
		{
			OpenTexture = GD.Load<Texture2D>("res://assets/sprites/environment/background/door_open.png");
		}
		if (ClosedTexture == null)
		{
			ClosedTexture = GD.Load<Texture2D>("res://assets/sprites/environment/background/door_close.png");
		}
	}

	public void Open()
	{
		if (OpenTexture != null)
		{
			_sprite.Texture = OpenTexture;
		}
	}

	public void Close()
	{
		if (ClosedTexture != null)
		{
			_sprite.Texture = ClosedTexture;
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Entity entity && entity.IsAlive())
		{
			GD.Print("Une entité est entrée dans la porte.");
			EmitSignal(SignalName.EntityEnteredDoor, entity);
		}
	}
}
