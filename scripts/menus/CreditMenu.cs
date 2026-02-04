using Godot;
using System;

/// <summary>
/// Displays the game credits.
/// </summary>
public partial class CreditMenu : GeneralMenu 
{
    [Signal]
    public delegate void CloseRequestedEventHandler();

    /// <summary>
    /// Signals that the close button was pressed.
    /// </summary>
    public void _on_close_button_pressed() 
    {
        EmitSignal(SignalName.CloseRequested);
    }
}
