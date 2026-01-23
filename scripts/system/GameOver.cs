using Godot;
using System;

public partial class GameOver : Control
{
	public override void _Ready()
	{
		var window = GetNodeOrNull<Control>("CanvasLayer/Window");
		if (window != null)
		{
			var tween = CreateTween().SetLoops();
			tween.TweenProperty(window, "position:y", window.Position.Y - 20f, 2.0f)
				 .SetTrans(Tween.TransitionType.Sine)
				 .SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(window, "position:y", window.Position.Y, 2.0f)
				 .SetTrans(Tween.TransitionType.Sine)
				 .SetEase(Tween.EaseType.InOut);
		}
	}

    public void SetWavesSurvived(int waves, int kills, int passed)
    {
        var label = GetNode<Label>("CanvasLayer/Window/VDiv/Data");
        label.Text = $"Vous avez survécu pendant {waves} vagues\n" +
                     $"Entités éliminées : {kills}\n" +
                     $"Verres livrés : {passed}";
    }

    public void _on_button_pressed()
    {
        GetTree().ReloadCurrentScene();
    }
}
