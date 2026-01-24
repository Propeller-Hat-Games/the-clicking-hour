using Godot;
using System;

public partial class CreditMenu : Control {
	[Signal]
	public delegate void CloseRequestedEventHandler();

	public void _on_close_button_pressed() {
		EmitSignal(SignalName.CloseRequested);
	}
}
