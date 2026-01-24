using Godot;
using System;

public partial class MainMenu : Control {
	[Signal]
	public delegate void GameStartedEventHandler();

	[Export]
	public PackedScene CreditMenuScene { get; set; }

	[Export]
	public PackedScene OptionsMenuScene { get; set; }

	private Control _creditMenuInstance;
	private Control _optionsMenuInstance;

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

	public void _on_credits_button_pressed() {
		if (CreditMenuScene != null) {
			var creditMenu = CreditMenuScene.Instantiate<CreditMenu>();
			_creditMenuInstance = creditMenu;
			AddChild(_creditMenuInstance);
			creditMenu.CloseRequested += _on_credit_menu_closed;
		}
	}

	private void _on_credit_menu_closed() {
		if (_creditMenuInstance != null) {
			_creditMenuInstance.QueueFree();
			_creditMenuInstance = null;
		}
	}

	public void _on_options_button_pressed() {
		if (OptionsMenuScene != null) {
			var optionsMenu = OptionsMenuScene.Instantiate<OptionsMenu>();
			_optionsMenuInstance = optionsMenu;
			AddChild(_optionsMenuInstance);
			optionsMenu.CloseRequested += _on_options_menu_closed;
		} else {
			// Fallback if scene is not exported
			var scene = GD.Load<PackedScene>("res://scenes/ui/options_menu.tscn");
			var optionsMenu = scene.Instantiate<OptionsMenu>();
			_optionsMenuInstance = optionsMenu;
			AddChild(_optionsMenuInstance);
			optionsMenu.CloseRequested += _on_options_menu_closed;
		}
	}

	private void _on_options_menu_closed() {
		if (_optionsMenuInstance != null) {
			_optionsMenuInstance.QueueFree();
			_optionsMenuInstance = null;
		}
	}

	public void _on_quit_button_pressed() {
		GetTree().Quit();
	}
}
