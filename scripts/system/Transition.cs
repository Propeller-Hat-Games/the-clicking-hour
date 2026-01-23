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
		titleLabel = GetNode<Label>("CanvasLayer/Title");
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
