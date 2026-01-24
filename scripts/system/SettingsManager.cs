using Godot;
using System;

public partial class SettingsManager : Node
{
    private static SettingsManager _instance;
    public static SettingsManager Instance => _instance;

    private float _masterVolume = 1.0f;
    private float _sfxVolume = 1.0f;
    private bool _glitchEnabled = true;
    private bool _globalEffectsEnabled = true;

    public float MasterVolume
    {
        get => _masterVolume;
        set
        {
            _masterVolume = value;
            ApplyMasterVolume();
        }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set
        {
            _sfxVolume = value;
        }
    }

    public bool GlitchEnabled
    {
        get => _glitchEnabled;
        set
        {
            _glitchEnabled = value;
            EmitSignal(SignalName.SettingsChanged);
        }
    }

    public bool GlobalEffectsEnabled
    {
        get => _globalEffectsEnabled;
        set
        {
            _globalEffectsEnabled = value;
            EmitSignal(SignalName.SettingsChanged);
        }
    }

    [Signal]
    public delegate void SettingsChangedEventHandler();

    public override void _Ready()
    {
        _instance = this;
        LoadSettings();
        ApplyMasterVolume();
    }

    private void ApplyMasterVolume()
    {
        int masterBusIndex = AudioServer.GetBusIndex("Master");
        AudioServer.SetBusVolumeDb(masterBusIndex, Mathf.LinearToDb(_masterVolume));
    }

    public void SaveSettings()
    {
        using var config = new ConfigFile();
        config.SetValue("audio", "master_volume", _masterVolume);
        config.SetValue("audio", "sfx_volume", _sfxVolume);
        config.SetValue("effects", "glitch_enabled", _glitchEnabled);
        config.SetValue("effects", "global_effects_enabled", _globalEffectsEnabled);
        config.Save("user://settings.cfg");
    }

    private void LoadSettings()
    {
        using var config = new ConfigFile();
        Error err = config.Load("user://settings.cfg");
        if (err == Error.Ok)
        {
            _masterVolume = (float)config.GetValue("audio", "master_volume", 1.0f);
            _sfxVolume = (float)config.GetValue("audio", "sfx_volume", 1.0f);
            _glitchEnabled = (bool)config.GetValue("effects", "glitch_enabled", true);
            _globalEffectsEnabled = (bool)config.GetValue("effects", "global_effects_enabled", true);
        }
    }
}
