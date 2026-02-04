using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages background music playback and playlists.
/// </summary>
public partial class MusicManager : AudioStreamPlayer
{
    private AudioStream _menuMusic;
    private List<AudioStream> _normalPlaylist = new();
    private List<AudioStream> _nightPlaylist = new();
    private Random _rng = new();

    /// <summary>
    /// Initializes the music manager and preloads audio tracks for menu and game playlists.
    /// </summary>
    public override void _Ready()
    {
        // Menu music (static)
        _menuMusic = GD.Load<AudioStream>("res://assets/musics/background/Jet27Drink.mp3");
        
        // 🌞 Normal Playlist
        _normalPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/GroovyDrink.mp3"));
        _normalPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeasideDrink.mp3"));
        _normalPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/background/SeriousDrink.mp3"));
        
        GD.Print($"[MUSIC] Normal playlist loaded: {_normalPlaylist.Count} tracks");

        // 🌙 Night Mode Playlist
        _nightPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/backgroundDarkMode/DrunkGroovy.mp3"));
        _nightPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/backgroundDarkMode/DrunkSeaside.mp3"));
        _nightPlaylist.Add(GD.Load<AudioStream>("res://assets/musics/backgroundDarkMode/DrunkSerious.mp3"));

        GD.Print($"[MUSIC] Night playlist loaded: {_nightPlaylist.Count} tracks");
    }

    /// <summary>
    /// Plays the main menu music.
    /// </summary>
    public void PlayMenuMusic()
    {
        Stop();
        Stream = _menuMusic;
        Play(0f);

        GD.Print("[MUSIC] MENU");
    }

    /// <summary>
    /// Plays a random track from the game playlist (normal or night mode).
    /// </summary>
    /// <param name="isNightMode">If true, plays from the night mode playlist.</param>
    public void PlayGameMusic(bool isNightMode)
    {
        Stop();

        List<AudioStream> playlist = isNightMode ? _nightPlaylist : _normalPlaylist;
        Stream = playlist[_rng.Next(playlist.Count)];
        Play(0f);
        
        if (isNightMode) GD.Print("[MUSIC] NIGHT");
        else GD.Print("[MUSIC] NORMAL");
    }
}
