using Godot;

public partial class Transition : Control
{
	private Label titleLabel;
	private Button nextButton;
	private int completedWave;
	
	[Signal]
	public delegate void NextStageRequestedEventHandler();

	private AudioStreamPlayer audioPlayer;
	
	public override void _Ready()
	{
		titleLabel = GetNode<Label>("CanvasLayer/Window/Title");
		
		PlayRandomJingle();
		
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

	private void PlayRandomJingle()
	{
		audioPlayer = new AudioStreamPlayer();
		AddChild(audioPlayer);
		
		int randomIndex = GD.RandRange(1, 3);
		string path = $"res://assets/sounds/Jingle{randomIndex}.mp3";
		
		var stream = GD.Load<AudioStream>(path);
		if (stream != null)
		{
			audioPlayer.Stream = stream;
			audioPlayer.Play();
			GD.Print($"Playing jingle: {path}");
		}
		else
		{
			GD.PrintErr($"Could not load jingle: {path}");
		}
	}
}
