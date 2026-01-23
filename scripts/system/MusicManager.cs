using Godot;
using System;
using System.Collections.Generic;

public partial class MusicManager : Node
{
	private AudioStreamPlayer _player;
	private List<AudioStream> _playlist = new();
	private int _index = 0;
	private Random _rng = new();

	public override void _Ready()
	{
		_player = new AudioStreamPlayer();
		AddChild(_player);

		LoadMusic();
		ShufflePlaylist();

		_player.Finished += OnMusicFinished;

		PlayCurrent();
	}

	private void LoadMusic()
	{
		_playlist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeasideDrink.mp3"));
		_playlist.Add(GD.Load<AudioStream>("res://assets/musics/background/GroovyDrink.mp3"));
	}

	private void ShufflePlaylist()
	{
		for (int i = _playlist.Count - 1; i > 0; i--)
		{
			int j = _rng.Next(i + 1);
			(_playlist[i], _playlist[j]) = (_playlist[j], _playlist[i]);
		}

		_index = 0;
	}

	private void PlayCurrent()
	{
		_player.Stream = _playlist[_index];
		_player.Play();
	}

	private void OnMusicFinished()
	{
		_index++;

		if (_index >= _playlist.Count)
			_index = 0; // ← AUTO LOOP DU PATTERN

		PlayCurrent();
	}
}
