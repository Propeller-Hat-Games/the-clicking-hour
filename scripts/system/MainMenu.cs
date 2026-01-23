using Godot;
using System;

public partial class MainMenu : Control {
	[Signal]
	public delegate void GameStartedEventHandler();

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

	public void _on_play_button_pressed() {
		EmitSignal(SignalName.GameStarted);
	}

	public void _on_quit_button_pressed() {
		GetTree().Quit();
	}
}
