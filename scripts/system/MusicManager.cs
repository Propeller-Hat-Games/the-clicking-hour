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

    private Tween _fadeTween;
    private Tween _pauseTween;
    [Export]
    private float _fadeDuration = 2.0f;

    private int _musicBusIndex;
    private int _lowPassEffectIndex = -1;
    private AudioEffectLowPassFilter _lowPassEffect;

    /// <summary>
    /// Initializes the music manager and preloads audio tracks for menu and game playlists.
    /// </summary>
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        SetupMusicBus();

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
    /// Sets up a dedicated audio bus for music and adds a low-pass filter effect.
    /// </summary>
    private void SetupMusicBus()
    {
        int existingBus = AudioServer.GetBusIndex("Music");
        if (existingBus != -1)
        {
            _musicBusIndex = existingBus;
        }
        else
        {
            _musicBusIndex = AudioServer.GetBusCount();
            AudioServer.AddBus(_musicBusIndex);
            AudioServer.SetBusName(_musicBusIndex, "Music");
            AudioServer.SetBusSend(_musicBusIndex, "Master");
        }
        
        Bus = "Music";

        // Check if low pass already exists
        for (int i = 0; i < AudioServer.GetBusEffectCount(_musicBusIndex); i++)
        {
            if (AudioServer.GetBusEffect(_musicBusIndex, i) is AudioEffectLowPassFilter existingEffect)
            {
                _lowPassEffect = existingEffect;
                _lowPassEffectIndex = i;
                return;
            }
        }

        _lowPassEffect = new AudioEffectLowPassFilter();
        _lowPassEffect.CutoffHz = 20000;
        AudioServer.AddBusEffect(_musicBusIndex, _lowPassEffect);
        _lowPassEffectIndex = AudioServer.GetBusEffectCount(_musicBusIndex) - 1;
        AudioServer.SetBusEffectEnabled(_musicBusIndex, _lowPassEffectIndex, false);
    }

    /// <summary>
    /// Toggles a muffled (low-pass) effect on the music, typically used when the game is paused.
    /// </summary>
    /// <param name="paused">Whether to enable the pause effect.</param>
    public void SetPauseEffect(bool paused)
    {
        if (_lowPassEffect == null || _lowPassEffectIndex == -1) return;

        if (_pauseTween != null && _pauseTween.IsValid())
        {
            _pauseTween.Kill();
        }

        _pauseTween = CreateTween();
        float targetCutoff = paused ? 800f : 20000f;
        float targetVolume = paused ? -6f : 0f;

        AudioServer.SetBusEffectEnabled(_musicBusIndex, _lowPassEffectIndex, true);

        _pauseTween.Parallel().TweenProperty(_lowPassEffect, "cutoff_hz", targetCutoff, 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);
        
        _pauseTween.Parallel().TweenProperty(this, "volume_db", targetVolume, 0.5f)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.Out);

        if (!paused)
        {
            _pauseTween.Finished += () => AudioServer.SetBusEffectEnabled(_musicBusIndex, _lowPassEffectIndex, false);
        }
    }

    /// <summary>
    /// Plays the main menu music with a fade effect.
    /// </summary>
    public async Task PlayMenuMusic()
    {
        await FadeTo(_menuMusic);
        GD.Print("[MUSIC] MENU");
    }

    /// <summary>
    /// Plays a random track from the game playlist (normal or night mode) with a fade effect.
    /// </summary>
    /// <param name="isNightMode">If true, plays from the night mode playlist.</param>
    public async Task PlayGameMusic(bool isNightMode)
    {
        List<AudioStream> playlist = isNightMode ? _nightPlaylist : _normalPlaylist;
        AudioStream nextStream = playlist[_rng.Next(playlist.Count)];
        
        await FadeTo(nextStream);
        
        if (isNightMode) GD.Print("[MUSIC] NIGHT");
        else GD.Print("[MUSIC] NORMAL");
    }

    /// <summary>
    /// Smoothly fades out the current music and stops it.
    /// </summary>
    /// <param name="duration">Duration of the fade-out. If -1, uses half of _fadeDuration.</param>
    public async Task FadeOut()
    {
        if (!Playing) return;

        if (_fadeTween != null && _fadeTween.IsValid())
        {
            _fadeTween.Kill();
        }

        _fadeTween = CreateTween();
        _fadeTween.TweenProperty(this, "volume_db", -80f, _fadeDuration)
            .SetTrans(Tween.TransitionType.Sine)
            .SetEase(Tween.EaseType.In);
        _fadeTween.TweenCallback(Callable.From(() => {
            Stop();
            VolumeDb = 0; // Reset volume for next play if not using FadeTo
        }));

        GD.Print("[MUSIC] FADE OUT");

        await ToSignal(_fadeTween, Tween.SignalName.Finished);
    }

    /// <summary>
    /// Smoothly fades out the current music and fades in the new stream.
    /// </summary>
    /// <param name="nextStream">The audio stream to transition to.</param>
    private async Task FadeTo(AudioStream nextStream)
    {
        if (Stream == nextStream && Playing) return;

        if (_fadeTween != null && _fadeTween.IsValid())
        {
            _fadeTween.Kill();
        }

        _fadeTween = CreateTween();

        // Fade out
        if (Playing)
        {
            _fadeTween.TweenProperty(this, "volume_db", -80f, _fadeDuration / 2f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.In);
        }
        else
        {
            VolumeDb = -80f;
        }

        // Change stream
        _fadeTween.TweenCallback(Callable.From(() =>
        {
            Stream = nextStream;
            if (nextStream != null)
            {
                Play();
            }
            else
            {
                Stop();
            }
        }));

        // Fade in
        if (nextStream != null)
        {
            _fadeTween.TweenProperty(this, "volume_db", 0f, _fadeDuration / 2f)
                .SetTrans(Tween.TransitionType.Sine)
                .SetEase(Tween.EaseType.Out);
        }

        await ToSignal(_fadeTween, Tween.SignalName.Finished);
    }
}