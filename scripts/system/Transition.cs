using Godot;

public partial class Transition : Control
{
	private Label titleLabel;
	private Button nextButton;
	private int completedWave;
	
	[Signal]
	public delegate void NextStageRequestedEventHandler();

	public override void _Ready()
	{
		titleLabel = GetNode<Label>("CanvasLayer/Window/Title");
		
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

	public void SetCompletedWave(int waveNumber)
	{
		completedWave = waveNumber;
		titleLabel.Text = $"STAGE {waveNumber}\nCLEARED !";
	}

	public void CloseWindow()
	{
		EmitSignal(SignalName.NextStageRequested);
		QueueFree();
	}
}
