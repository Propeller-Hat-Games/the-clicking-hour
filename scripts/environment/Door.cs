using Godot;
using System;

/// <summary>
/// Represents a door in the environment that entities can enter.
/// Handles the visual state (Open/Closed) and detects entity entry.
/// </summary>
public partial class Door : Area2D
{
    [Export]
    private Texture2D OpenTexture;
    [Export]
    private Texture2D ClosedTexture;
    [Export]
    private Sprite2D Sprite;

    [Signal]
    public delegate void EntityEnteredDoorEventHandler(Entity entity);

    /// <summary>
    /// Initializes the door, sets up textures and signals.
    /// </summary>
    public override void _Ready()
    {
        AddToGroup("Door");
        BodyEntered += OnBodyEntered;
    }

    /// <summary>
    /// Changes the door sprite to the open texture and plays the open sound.
    /// </summary>
    /// <param name="sfx">Reference to the Sound Effects Manager.</param>
    public void Open(SfxManager sfx)
    {
        Sprite.Texture = OpenTexture;
        sfx.PlayDoorOpenSound();
    }

    /// <summary>
    /// Changes the door sprite to the closed texture and plays the close sound.
    /// </summary>
    /// <param name="sfx">Reference to the Sound Effects Manager.</param>
    public void Close(SfxManager sfx)
    {
        Sprite.Texture = ClosedTexture;
        sfx.PlayDoorCloseSound();
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
