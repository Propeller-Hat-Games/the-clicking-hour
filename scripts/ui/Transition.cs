using Godot;

/// <summary>
/// Handles the transition screen between stages.
/// </summary>
public partial class Transition : Control
{
    private Label _titleLabel;
    
    [Signal]
    public delegate void NextStageRequestedEventHandler();
    
    /// <summary>
    /// Initializes the transition screen and starts a floating animation for the window.
    /// </summary>
    public override void _Ready()
    {
        _titleLabel = GetNode<Label>("CanvasLayer/Window/Title");
        
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
    /// Sets the text to display which wave was completed.
    /// </summary>
    /// <param name="waveNumber">The completed wave number.</param>
    public void SetCompletedWave(int waveNumber)
    {
        _titleLabel.Text = $"STAGE {waveNumber}\nCLEARED !";
    }

    /// <summary>
    /// Closes the transition window and signals for the next stage.
    /// </summary>
    public void CloseWindow()
    {
        EmitSignal(SignalName.NextStageRequested);
        QueueFree();
    }
}
