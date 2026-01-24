using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _player;
	private List<AudioStream> _gamePlaylist = new();
	private AudioStream _menuMusic;
	private int _index = 0;
	private float _savedPosition = 0f;
	private bool _isTransitioning = false;
	private float _baseVolumeDb = 0f;
	private Random _rng = new();

	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		AddChild(_player);
		_player.VolumeDb = -80f;
		
		LoadMusic();
	}

	private void LoadMusic()
	{
		_menuMusic = GD.Load<AudioStream>("res://assets/musics/background/Jet27Drink.mp3");
		
		_gamePlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/GroovyDrink.mp3"));
		_gamePlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeasideDrink.mp3"));
		_gamePlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeriousDrink.mp3"));
		
		ShufflePlaylist();
	}

	public async Task PlayMenuMusic(float fadeDuration = 1.5f)
	{
		_player.Stream = _menuMusic;
		_player.VolumeDb = -80f;
		_player.Play(0f);
		await FadeVolume(_baseVolumeDb, fadeDuration);
	}

	public async Task SwitchToGameMusic(float fadeDuration = 1.5f)
	{
		await FadeVolume(-80f, fadeDuration);
		_player.Stop();

		ShufflePlaylist();
		_player.Stream = _gamePlaylist[_index];
		_player.VolumeDb = -80f;
		_player.Play(0f);

		await FadeVolume(_baseVolumeDb, fadeDuration);
	}

	private void ShufflePlaylist()
	{
		for (int i = _gamePlaylist.Count - 1; i > 0; i--)
		{
			int j = _rng.Next(i + 1);
			(_gamePlaylist[i], _gamePlaylist[j]) = (_gamePlaylist[j], _gamePlaylist[i]);
		}
		_index = 0;
	}

	public async Task FadeOutAndSwitchTrack(float fadeDuration = 1.5f)
	{
		if (_isTransitioning) return;
		_isTransitioning = true;

		_savedPosition = _player.GetPlaybackPosition();
		await FadeVolume(-80f, fadeDuration);
		_player.Stop();
		
		_index = (_index + 1) % _gamePlaylist.Count;
		_isTransitioning = false;
	}

	public async Task FadeInNextTrack(float fadeDuration = 1.5f)
	{
		if (_isTransitioning) return;
		_isTransitioning = true;

		_player.Stream = _gamePlaylist[_index];
		_player.VolumeDb = -80f;
		_player.Play(_savedPosition);

		await FadeVolume(_baseVolumeDb, fadeDuration);
		_isTransitioning = false;
	}

	private async Task FadeVolume(float toDb, float duration)
	{
		if (!IsInsideTree()) return;
		Tween tween = CreateTween();
		tween.TweenProperty(_player, "volume_db", toDb, duration);
		await ToSignal(tween, Tween.SignalName.Finished);
	}
}