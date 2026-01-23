using Godot;
using System;

public partial class MainMenu : Node2D {
	[Signal]
	public delegate void GameStartedEventHandler();

	public override void _Ready() {
		var playButton = GetNode<Button>("CanvasLayer/HDiv/VDiv/PlayButton");
		playButton.Pressed += _on_play_button_pressed;
	}

	public override void _Process(double delta) {}
	
	private void _on_play_button_pressed() {
		EmitSignal(SignalName.GameStarted);
		// Hide the menu
		GetNode<CanvasLayer>("CanvasLayer").Visible = false;
	}

	public void _on_quit_button_pressed() {
		GetTree().Quit();
	}
}
