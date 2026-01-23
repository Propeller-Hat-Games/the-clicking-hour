using Godot;
using System.Threading.Tasks;

public partial class SplashScreen : Control
{
	[Export] private float FadeDuration = 1.5f;
	[Export] private float WaitDuration = 1.0f;
	[Export] private string MainMenuScenePath = "res://scenes/game_manager.tscn";
	
	private AnimatedSprite2D _sprite;
	private AudioStreamPlayer _audio;
	
	public override async void _Ready()
	{ 
		_sprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		_audio = GetNode<AudioStreamPlayer>("AudioStreamPlayer");
		
		_sprite.Position = GetViewportRect().Size / 2f;
		
		Color color = _sprite.Modulate;
		color.A = 0f;
		_sprite.Modulate = color;
		
		_sprite.Play();
		_audio.Play();
		
		await Fade(0f, 1f);
		
		await ToSignal(GetTree().CreateTimer(WaitDuration), "timeout");
		
		await Fade(1f, 0f);
		
		GetTree().ChangeSceneToFile(MainMenuScenePath);
	}
	
	private async Task Fade(float from, float to)
	{
		float time = 0f;
		
		while (time < FadeDuration)
		{
			time += (float)GetProcessDeltaTime();
			float alpha = Mathf.Lerp(from, to, time / FadeDuration);
		
			Color color = _sprite.Modulate;
			color.A = alpha;
			_sprite.Modulate = color;

			await ToSignal(GetTree(), "process_frame");
		}
		
		Color finalColor = _sprite.Modulate;
		finalColor.A = to;
		_sprite.Modulate = finalColor;
	}
}
