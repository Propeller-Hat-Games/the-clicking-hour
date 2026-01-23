using Godot;
using System;

public partial class GameOver : Control
{
    public void SetWavesSurvived(int waves, int kills, int passed)
    {
        var label = GetNode<Label>("CanvasLayer/VDiv/Data");
        label.Text = $"Vous avez survécu pendant {waves} vagues\n" +
                     $"Entités éliminées : {kills}\n" +
                     $"Verres livrés : {passed}";
    }

    public void _on_button_pressed()
    {
        GetTree().ReloadCurrentScene();
    }
}
