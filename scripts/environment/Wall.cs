using Godot;
using System;

/// <summary>
/// Wall script that plays the attached AnimatedSprite2D and synchronizes 
/// the Light color with the animation frames.
/// </summary>
public partial class Wall : Node2D
{
    [Export]
    private AnimatedSprite2D Sprite;
    [Export]
    private PointLight2D Light;

    /// <summary>
    /// Initializes the wall by playing the sprite and connecting the FrameChanged signal.
    /// </summary>
    public override void _Ready()
    {
        Sprite.Play();
        Sprite.FrameChanged += OnFrameChanged;
        UpdateLightColor();
    }

    /// <summary>
    /// Triggered when the sprite's animation frame changes.
    /// </summary>
    private void OnFrameChanged()
    {
        UpdateLightColor();
    }

    /// <summary>
    /// Updates the Light's color based on the current Sprite frame.
    /// </summary>
    private void UpdateLightColor()
    {
        switch (Sprite.Frame)
        {
            case 0:
                Light.Color = Colors.Red;
                break;
            case 1:
                Light.Color = Colors.Green;
                break;
            case 2:
                Light.Color = Colors.Blue;
                break;
            case 3:
                Light.Color = Colors.Yellow;
                break;
        }
    }
}
