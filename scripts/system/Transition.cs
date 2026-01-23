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
		titleLabel = GetNode<Label>("CanvasLayer/VDiv/Title");
		nextButton = GetNode<Button>("CanvasLayer/VDiv/Button");
		
		nextButton.Pressed += OnNextButtonPressed;
	}

	public void SetCompletedWave(int waveNumber)
	{
		completedWave = waveNumber;
		titleLabel.Text = $"STAGE {waveNumber}\nCLEARED !";
	}

	private void OnNextButtonPressed()
	{
		EmitSignal(SignalName.NextStageRequested);
		QueueFree();
	}
}
