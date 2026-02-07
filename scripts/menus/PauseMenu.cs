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
    /// Resumes the game and closes the pause menu.
    /// </summary>
    public void _on_resume_button_pressed()
    {
        GetTree().Paused = false;
        GetNode<MusicManager>("/root/MusicManager").SetPauseEffect(false);
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
        GetNode<MusicManager>("/root/MusicManager").SetPauseEffect(false);
        GetTree().ReloadCurrentScene();
    }
}