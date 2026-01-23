using Godot;
using System;

public partial class GameOver : Control
{
    public void SetWavesSurvived(int waves)
    {
        var label = GetNode<Label>("CanvasLayer/VDiv/Data");
        label.Text = $"Vous avez survécu pendant {waves} vagues";
    }

    public void _on_button_pressed()
    {
        GetTree().ReloadCurrentScene();
    }
}
