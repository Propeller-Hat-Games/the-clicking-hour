using Godot;
using System;

/// <summary>
/// Base class for menus, providing common functionality like floating window animation.
/// </summary>
public partial class GeneralMenu : Control
{
    /// <summary>
    /// Initializes the menu and starts a floating animation for the window.
    /// </summary>
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

    /// <summary>
    /// Closes the menu by freeing the node.
    /// </summary>
    public void Close()
    {
        QueueFree();
    }
}
