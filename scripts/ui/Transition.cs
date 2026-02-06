using Godot;

/// <summary>
/// Handles the transition screen between stages.
/// </summary>
public partial class Transition : GeneralMenu
{
    private Label _titleLabel;
    
    [Signal]
    public delegate void NextStageRequestedEventHandler();
    
    /// <summary>
    /// Initializes the transition screen and starts a floating animation for the window.
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        _titleLabel = GetNode<Label>("CanvasLayer/Window/Title");
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
