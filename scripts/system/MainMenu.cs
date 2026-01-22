using Godot;
using System;

public partial class MainMenu : Node2D {
	public override void _Ready() {}
	public override void _Process(double delta) {}
	
	public void _on_quit_button_pressed() {
		GetTree().Quit();
	}
}
