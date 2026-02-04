using Godot;
using System;

/// <summary>
/// Handles the Pause Menu.
/// </summary>
public partial class PauseMenu : GeneralMenu
{
    [Export]
    public PackedScene OptionsMenuScene { get; set; }

    private Control _optionsMenuInstance;

    /// <summary>
    /// Initializes the pause menu, ensuring it processes even when the tree is paused.
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        Visible = true;
        ProcessMode = ProcessModeEnum.Always;
    }

    /// <summary>
    /// Handles input events, specifically the 'ui_cancel' (ESC) to resume or close options.
    /// </summary>
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

    /// <summary>
    /// Resumes the game and closes the pause menu.
    /// </summary>
    public void _on_resume_button_pressed()
    {
        GetTree().Paused = false;
        QueueFree();
    }

    /// <summary>
    /// Opens the options menu from the pause screen.
    /// </summary>
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

    /// <summary>
    /// Callback for when the options menu is closed within the pause screen.
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
    /// Unpauses the game and reloads the current scene to return to the main menu.
    /// </summary>
    public void _on_main_menu_button_pressed()
    {
        GetTree().Paused = false;
        GetTree().ReloadCurrentScene();
    }
}