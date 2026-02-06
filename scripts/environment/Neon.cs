using Godot;
using System;

/// <summary>
/// Controls a Neon light that flickers between On and Off states.
/// </summary>
public partial class Neon : Node2D
{
    [Export]
    public Sprite2D NeonOnSprite { get; set; }

    [Export]
    public Sprite2D NeonOffSprite { get; set; }
    [Export]
    private PointLight2D Light;

    /// <summary>
    /// Starts an asynchronous loop that flickers the neon light sprite and its glow.
    /// </summary>
    public override async void _Ready()
    {
        while (IsInstanceValid(this))
        {
            NeonOnSprite.Visible = true;
            NeonOffSprite.Visible = false;
            Light.Visible = true;
            await ToSignal(GetTree().CreateTimer(GD.RandRange(1f, 5f), false), Timer.SignalName.Timeout);

            NeonOnSprite.Visible = false;
            NeonOffSprite.Visible = true;
            Light.Visible = false;
            await ToSignal(GetTree().CreateTimer(GD.RandRange(0.5f, 1.5f), false), Timer.SignalName.Timeout);
        }
    }
}
