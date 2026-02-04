using Godot;
using System;

/// <summary>
/// Controls the Main Menu interactions.
/// </summary>
public partial class MainMenu : GeneralMenu 
{
    [Signal]
    public delegate void PlayButtonPressedEventHandler();

    [Export]
    public PackedScene CreditMenuScene { get; set; }

    [Export]
    public PackedScene OptionsMenuScene { get; set; }

    private Control _creditMenuInstance;
    private Control _optionsMenuInstance;

    /// <summary>
    /// Signals that the play button was pressed.
    /// </summary>
    public void _on_play_button_pressed() 
    {
        EmitSignal(SignalName.PlayButtonPressed);
    }

    /// <summary>
    /// Opens the credit menu.
    /// </summary>
    public void _on_credits_button_pressed() 
    {
        if (CreditMenuScene != null) 
        {
            var creditMenu = CreditMenuScene.Instantiate<CreditMenu>();
            _creditMenuInstance = creditMenu;
            AddChild(_creditMenuInstance);
            creditMenu.CloseRequested += _on_credit_menu_closed;
        }
    }

    /// <summary>
    /// Callback for when the credit menu is closed.
    /// </summary>
    private void _on_credit_menu_closed() 
    {
        if (_creditMenuInstance != null) 
        {
            _creditMenuInstance.QueueFree();
            _creditMenuInstance = null;
        }
    }

    /// <summary>
    /// Opens the options menu.
    /// </summary>
    public void _on_options_button_pressed() 
    {
        PackedScene sceneToInstantiate = OptionsMenuScene;
        
        if (sceneToInstantiate == null) 
        {
            // Fallback if scene is not exported
            sceneToInstantiate = GD.Load<PackedScene>("res://scenes/ui/options_menu.tscn");
        }

        if (sceneToInstantiate != null)
        {
            var optionsMenu = sceneToInstantiate.Instantiate<OptionsMenu>();
            _optionsMenuInstance = optionsMenu;
            AddChild(_optionsMenuInstance);
            optionsMenu.CloseRequested += _on_options_menu_closed;
        }
    }

    /// <summary>
    /// Callback for when the options menu is closed.
    /// </summary>
    private void _on_options_menu_closed() 
    {
        if (_optionsMenuInstance != null) 
        {
            _optionsMenuInstance.QueueFree();
            _optionsMenuInstance = null;
        }
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void _on_quit_button_pressed() 
    {
        GetTree().Quit();
    }
}
