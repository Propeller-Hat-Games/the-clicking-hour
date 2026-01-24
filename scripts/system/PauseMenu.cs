using Godot;
using System;

public partial class PauseMenu : Control
{
	[Export]
	public PackedScene OptionsMenuScene;

	private Control _optionsMenuInstance;

	public override void _Ready()
	{
		Visible = true;
		ProcessMode = ProcessModeEnum.Always;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel")) // ESC
		{
			if (_optionsMenuInstance != null)
			{
				_on_options_menu_closed();
			}
			else
			{
				_on_resume_button_pressed();
			}
			GetViewport().SetInputAsHandled();
		}
	}

	public void _on_resume_button_pressed()
	{
		GetTree().Paused = false;
		QueueFree();
	}

	public void _on_options_button_pressed()
	{
		if (OptionsMenuScene != null)
		{
			var optionsMenu = OptionsMenuScene.Instantiate<OptionsMenu>();
			_optionsMenuInstance = optionsMenu;
			AddChild(_optionsMenuInstance);
			optionsMenu.CloseRequested += _on_options_menu_closed;
		}
	}

	private void _on_options_menu_closed()
	{
		if (_optionsMenuInstance != null)
		{
			_optionsMenuInstance.QueueFree();
			_optionsMenuInstance = null;
		}
	}

	public void _on_main_menu_button_pressed()
	{
		GetTree().Paused = false;
		GetTree().ReloadCurrentScene();
	}
}