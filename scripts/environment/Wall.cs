using Godot;
using System;

/// <summary>
/// Wall script that plays the attached AnimatedSprite2D if available.
/// </summary>
public partial class Wall : Node2D
{
    /// <summary>
    /// Starts the wall's animated sprite on ready if it exists.
    /// </summary>
    public override void _Ready()
    {
        var sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        sprite?.Play();
    }
}
