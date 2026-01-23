using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _player;
	private List<AudioStream> _playlist = new();
	private int _index = 0;
	private float _savedPosition = 0f;
	private bool _isTransitioning = false;
	private float _baseVolumeDb = 0f;
	private Random _rng = new();

	public override void _Ready()
	{
		GD.Print($"🎵 MusicManager._Ready() called - Instance: {GetInstanceId()}");
		
		_player = new AudioStreamPlayer();
		AddChild(_player);
		_player.VolumeDb = -80f;
		
		LoadMusic();
		ShufflePlaylist();
	}

	private void LoadMusic()
	{
		_playlist.Add(GD.Load<AudioStream>("res://assets/musics/background/GroovyDrink.mp3"));
		_playlist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeasideDrink.mp3"));
		_playlist.Add(GD.Load<AudioStream>("res://assets/musics/background/Jet27Drink.mp3"));
		_playlist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeriousDrink.mp3"));
		GD.Print($"🎵 [{GetInstanceId()}] Loaded {_playlist.Count} tracks");
	}

	private void ShufflePlaylist()
	{
		for (int i = _playlist.Count - 1; i > 0; i--)
		{
			int j = _rng.Next(i + 1);
			(_playlist[i], _playlist[j]) = (_playlist[j], _playlist[i]);
		}
		_index = 0;
		GD.Print($"🎵 [{GetInstanceId()}] Playlist shuffled");
	}

	// Démarre la musique avec fade in
	public async Task StartMusic(float fadeDuration = 1.5f)
	{
		GD.Print($"🎵 [{GetInstanceId()}] StartMusic() called");
		_player.Stream = _playlist[_index];
		_player.VolumeDb = -80f;
		_player.Play(0f);
		GD.Print($"🎵 [{GetInstanceId()}] Playing track {_index}");
		
		Tween tween = FadeVolume(_baseVolumeDb, fadeDuration);
		if (tween != null)
		{
			await ToSignal(tween, Tween.SignalName.Finished);
		}
	}

	// Fade out, sauvegarde position, change de track
	public async Task FadeOutAndSwitchTrack(float fadeDuration = 1.5f)
	{
		if (_isTransitioning)
		{
			GD.Print($"⚠️ [{GetInstanceId()}] Already transitioning, skipping FadeOut");
			return;
		}
		_isTransitioning = true;

		GD.Print($"🎵 [{GetInstanceId()}] FadeOutAndSwitchTrack() called");
		
		// Sauvegarde position
		_savedPosition = _player.GetPlaybackPosition();
		GD.Print($"🎵 [{GetInstanceId()}] Saved position: {_savedPosition}s");

		// Fade out
		Tween tween = FadeVolume(-80f, fadeDuration);
		if (tween != null)
		{
			await ToSignal(tween, Tween.SignalName.Finished);
			// Stop
			_player.Stop();
			GD.Print($"🎵 [{GetInstanceId()}] Stopped player");
			
			// Prochaine track
			_index = (_index + 1) % _playlist.Count;
			GD.Print($"🎵 [{GetInstanceId()}] Next track index: {_index}");

			_isTransitioning = false;
		}
		else
		{
			_player.Stop();
			_index = (_index + 1) % _playlist.Count;
			_isTransitioning = false;
		}
	}

	// Fade in de la nouvelle track au timestamp sauvegardé
	public async Task FadeInNextTrack(float fadeDuration = 1.5f)
	{
		if (_isTransitioning)
		{
			GD.Print($"⚠️ [{GetInstanceId()}] Already transitioning, skipping FadeIn");
			return;
		}
		_isTransitioning = true;

		GD.Print($"🎵 [{GetInstanceId()}] FadeInNextTrack() called");
		
		// Charge nouvelle musique
		_player.Stream = _playlist[_index];
		_player.VolumeDb = -80f;
		
		// Play au timestamp sauvegardé
		_player.Play(_savedPosition);
		GD.Print($"🎵 [{GetInstanceId()}] Playing track {_index} from {_savedPosition}s");

		// Fade in
		Tween tween = FadeVolume(_baseVolumeDb, fadeDuration);
		if (tween != null)
		{
			await ToSignal(tween, Tween.SignalName.Finished);
			_isTransitioning = false;
		}
		else
		{
			_isTransitioning = false;
		}
	}

	private Tween FadeVolume(float toDb, float duration)
	{
		if (!IsInsideTree()) return null;
		Tween tween = CreateTween();
		tween.TweenProperty(_player, "volume_db", toDb, duration);
		return tween;
	}

	public override void _ExitTree()
	{
		GD.Print($"🛑 [{GetInstanceId()}] MusicManager._ExitTree() - Stopping music");
		if (_player != null && _player.Playing)
		{
			_player.Stop();
		}
	}
}
