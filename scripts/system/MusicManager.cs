using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _player;
	private List<AudioStream> _normalPlaylist = new();
	private List<AudioStream> _nightPlaylist = new();
	private AudioStream _menuMusic;
	private int _index = 0;
	private float _savedPosition = 0f;
	private bool _isTransitioning = false;
	private float _baseVolumeDb = 0f;
	private Random _rng = new();
	
	// 🌙 Night Mode
	private bool _useNightMode = false;

	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		AddChild(_player);
		_player.VolumeDb = -80f;
		
		LoadMusic();
	}

	private void LoadMusic()
	{
		// Musique de menu (toujours la même)
		_menuMusic = GD.Load<AudioStream>("res://assets/musics/background/Jet27Drink.mp3");
		
		// 🌞 Playlist normale
		_normalPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/GroovyDrink.mp3"));
		_normalPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeasideDrink.mp3"));
		_normalPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeriousDrink.mp3"));
		
		// 🌙 Playlist Night Mode
		_nightPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/backgroundDarkMode/DrunkGroovy.mp3"));
		_nightPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/backgroundDarkMode/DrunkSeaside.mp3"));
		_nightPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/backgroundDarkMode/DrunkSerious.mp3"));
		
		GD.Print($"✅ Normal playlist loaded: {_normalPlaylist.Count} tracks");
		GD.Print($"✅ Night playlist loaded: {_nightPlaylist.Count} tracks");
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
		
		// Utiliser la playlist normale par défaut
		_useNightMode = false;
		var playlist = new List<AudioStream>(_normalPlaylist);
		ShufflePlaylist(playlist);
		
		_player.Stream = playlist[0];
		_player.VolumeDb = -80f;
		_player.Play(0f);
		await FadeVolume(_baseVolumeDb, fadeDuration);
		
		GD.Print("☀️ Started with NORMAL music");
	}

	private void ShufflePlaylist(List<AudioStream> playlist)
	{
		for (int i = playlist.Count - 1; i > 0; i--)
		{
			int j = _rng.Next(i + 1);
			(playlist[i], playlist[j]) = (playlist[j], playlist[i]);
		}
	}

	public async Task FadeOutAndSwitchTrack(float fadeDuration = 1.5f)
	{
		if (_isTransitioning) return;
		_isTransitioning = true;
		_savedPosition = _player.GetPlaybackPosition();
		await FadeVolume(-80f, fadeDuration);
		_player.Stop();
		_isTransitioning = false;
	}

	public async Task FadeInNextTrack(float fadeDuration = 1.5f, bool isNightMode = false)
	{
		if (_isTransitioning) return;
		_isTransitioning = true;
		
		// 🌙 Choisir la bonne playlist selon le mode
		List<AudioStream> playlist;
		if (isNightMode)
		{
			playlist = new List<AudioStream>(_nightPlaylist);
			GD.Print("🌙 Switching to NIGHT MODE music!");
		}
		else
		{
			playlist = new List<AudioStream>(_normalPlaylist);
			GD.Print("☀️ Switching to NORMAL music!");
		}
		
		// Shuffle et prendre une musique aléatoire
		ShufflePlaylist(playlist);
		_index = 0;
		
		_player.Stream = playlist[_index];
		_player.VolumeDb = -80f;
		_player.Play(0f);  // Commence au début de la nouvelle musique
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
