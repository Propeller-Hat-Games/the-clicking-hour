using Godot;
using System;

/// <summary>
/// Simple script to play the floor animation on Ready.
/// </summary>
public partial class Floor : AnimatedSprite2D
{
    /// <summary>
    /// Starts the floor animation on ready.
    /// </summary>
    public override void _Ready()
    {
        Play();
    }
}
