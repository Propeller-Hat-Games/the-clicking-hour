using Godot;
using System;

/// <summary>
/// Displayed when the game is lost. Shows stats and restart button.
/// </summary>
public partial class GameOver : GeneralMenu
{
    /// <summary>
    /// Updates the game over screen with the session statistics.
    /// </summary>
    /// <param name="waves">Number of waves survived.</param>
    /// <param name="kills">Total entities killed.</param>
    /// <param name="passed">Total glasses/items delivered/passed.</param>
    public void SetWavesSurvived(int waves, int kills, int passed)
    {
        var label = GetNode<Label>("CanvasLayer/Window/VDiv/Data");
        label.Text = $"Vous avez survécu pendant {waves} vagues\n" +
                     $"Entités éliminées : {kills}\n" +
                     $"Verres livrés : {passed}";
    }

    /// <summary>
    /// Reloads the current scene to restart the game.
    /// </summary>
    public void _on_button_pressed()
    {
        GetTree().ReloadCurrentScene();
    }
}
