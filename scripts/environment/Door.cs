using Godot;
using System;

/// <summary>
/// Represents a door in the environment that entities can enter.
/// Handles the visual state (Open/Closed) and detects entity entry.
/// </summary>
public partial class Door : Area2D
{
    [Export]
    public Texture2D OpenTexture { get; set; }
    [Export]
    public Texture2D ClosedTexture { get; set; }

    private Sprite2D _sprite;

    [Signal]
    public delegate void EntityEnteredDoorEventHandler(Entity entity);

    /// <summary>
    /// Initializes the door, sets up textures and signals.
    /// </summary>
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

    /// <summary>
    /// Changes the door sprite to the open texture and plays the open sound.
    /// </summary>
    /// <param name="sfx">Reference to the Sound Effects Manager.</param>
    public void Open(SfxManager sfx)
    {
        if (OpenTexture != null)
        {
            _sprite.Texture = OpenTexture;
            sfx.PlayDoorOpenSound();
        }
    }

    /// <summary>
    /// Changes the door sprite to the closed texture and plays the close sound.
    /// </summary>
    /// <param name="sfx">Reference to the Sound Effects Manager.</param>
    public void Close(SfxManager sfx)
    {
        if (ClosedTexture != null)
        {
            _sprite.Texture = ClosedTexture;
            sfx.PlayDoorCloseSound();
        }
    }

    /// <summary>
    /// Callback for when a body enters the door's detection area.
    /// </summary>
    /// <param name="body">The node that entered the area.</param>
    private void OnBodyEntered(Node2D body)
    {
        if (body is Entity entity && entity.IsAlive)
        {
            EmitSignal(SignalName.EntityEnteredDoor, entity);
        }
    }
}
