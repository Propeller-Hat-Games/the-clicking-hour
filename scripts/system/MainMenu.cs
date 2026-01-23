using Godot;
using System;

public partial class MainMenu : Control {
	[Signal]
	public delegate void GameStartedEventHandler();

	public void _on_play_button_pressed() {
		EmitSignal(SignalName.GameStarted);
	}

	public void _on_quit_button_pressed() {
		GetTree().Quit();
	}
}
