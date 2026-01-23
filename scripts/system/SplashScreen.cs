using Godot;
using System.Threading.Tasks;

public partial class SplashScreen : Control
{
	[Export] private float FadeDuration = 1.5f;
	[Export] private float WaitDuration = 1.0f;
	[Export] private string MainMenuScenePath = "res://scenes/game_manager.tscn";
	
	private AnimatedSprite2D _sprite;
	private AudioStreamPlayer _audio;
	private ColorRect _fadeColorRect;
	
	public override async void _Ready()
	{ 
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		
		// Add a dedicated fade overlay
		_fadeColorRect = new ColorRect();
		_fadeColorRect.Color = Colors.Black;
		_fadeColorRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
		_fadeColorRect.Modulate = new Color(1, 1, 1, 0);
		AddChild(_fadeColorRect);

		_sprite.Position = GetViewportRect().Size / 2f;
		
		Color color = _sprite.Modulate;
		color.A = 0f;
		_sprite.Modulate = color;
		
		_sprite.Play();
		_audio.Play();
		
		await FadeSprite(0f, 1f);
		
		await ToSignal(GetTree().CreateTimer(WaitDuration), "timeout");
		
		// Fade out everything to black
		Tween fadeOutTween = CreateTween().SetParallel();
		fadeOutTween.TweenProperty(_sprite, "modulate:a", 0f, FadeDuration);
		fadeOutTween.TweenProperty(_audio, "volume_db", -80f, FadeDuration);
		fadeOutTween.TweenProperty(_fadeColorRect, "modulate:a", 1f, FadeDuration);
		
		await ToSignal(fadeOutTween, "finished");
		
		// Wait one extra frame to be sure black is rendered
		await ToSignal(GetTree(), "process_frame");
		
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
	
	private async Task FadeSprite(float from, float to)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(_sprite, "modulate:a", to, FadeDuration)
			 .From(from);
		
		await ToSignal(tween, "finished");
	}
}
