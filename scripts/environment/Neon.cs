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

    /// <summary>
    /// Starts an asynchronous loop that flickers the neon light.
    /// </summary>
    public override async void _Ready()
    {
        if (NeonOnSprite == null || NeonOffSprite == null)
            return;

        while (IsInstanceValid(this))
        {
            NeonOnSprite.Visible = true;
            NeonOffSprite.Visible = false;
            await ToSignal(GetTree().CreateTimer(GD.RandRange(1f, 5f), false), Timer.SignalName.Timeout);

            NeonOnSprite.Visible = false;
            NeonOffSprite.Visible = true;
            await ToSignal(GetTree().CreateTimer(GD.RandRange(0.5f, 1.5f), false), Timer.SignalName.Timeout);
        }
    }
}
