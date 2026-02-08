using Godot;
using System;

/// <summary>
/// Manages application settings, persistence, and global state (like volume and effects).
/// </summary>
public partial class SettingsManager : Node
{
	private static SettingsManager _instance;
	public static SettingsManager Instance => _instance;

	private float _masterVolume = 1.0f;
	private float _sfxVolume = 1.0f;
	private bool _glitchEnabled = true;
	private bool _globalEffectsEnabled = true;
	private bool _hasSeenOnboarding = false;

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

	public bool HasSeenOnboarding
	{
		get => _hasSeenOnboarding;
		set
		{
			_hasSeenOnboarding = value;
			SaveSettings();
		}
	}

	[Signal]
	public delegate void SettingsChangedEventHandler();

	/// <summary>
	/// Initializes the settings manager, loads saved settings, and applies initial volumes.
	/// </summary>
	public override void _Ready()
	{
		_instance = this;
		LoadSettings();
		ApplyMasterVolume();
	}

	/// <summary>
	/// Applies the current master volume setting to the AudioServer.
	/// </summary>
	private void ApplyMasterVolume()
	{
		int masterBusIndex = AudioServer.GetBusIndex("Master");
		AudioServer.SetBusVolumeDb(masterBusIndex, Mathf.LinearToDb(_masterVolume));
	}

	/// <summary>
	/// Persists current settings to a configuration file.
	/// </summary>
	public void SaveSettings()
	{
		using var config = new ConfigFile();
		config.SetValue("audio", "master_volume", _masterVolume);
		config.SetValue("audio", "sfx_volume", _sfxVolume);
		config.SetValue("effects", "glitch_enabled", _glitchEnabled);
		config.SetValue("effects", "global_effects_enabled", _globalEffectsEnabled);
		config.SetValue("gameplay", "has_seen_onboarding", _hasSeenOnboarding);
		config.Save("user://settings.cfg");
	}

	/// <summary>
	/// Loads settings from the configuration file if it exists.
	/// </summary>
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
			_hasSeenOnboarding = (bool)config.GetValue("gameplay", "has_seen_onboarding", false);
		}
	}
}
