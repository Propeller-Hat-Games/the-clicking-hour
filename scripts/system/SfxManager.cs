using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages sound effects playback.
/// </summary>
public partial class SfxManager : Node
{
	private Random _random = new Random();

	// Click sounds
	private AudioStream _clic1;
	private AudioStream _clic2;
	private AudioStream _clic3;

	// Death sound
	private AudioStream _death1;

	// Door sounds
	private AudioStream _doorOpen;
	private AudioStream _doorClose;

	// Entity sounds
	private AudioStream _entityDig1;
	private AudioStream _entityEmergence1;

	// Damage sounds
	private AudioStream _takeDamage1;
	// Unused sounds removed/commented out
	// private AudioStream takeDamage2;
	// private AudioStream takeDamage3;

	// Jingle sounds
	private AudioStream _jingle1;
	private AudioStream _jingle2;
	private AudioStream _jingle3;
	
	/// <summary>
	/// Initializes the SFX manager and preloads all sound effect streams.
	/// </summary>
	public override void _Ready()
	{
		// Load all sounds
		_clic1 = GD.Load<AudioStream>("res://assets/sounds/Clic1.mp3");
		_clic2 = GD.Load<AudioStream>("res://assets/sounds/Clic2.mp3");
		_clic3 = GD.Load<AudioStream>("res://assets/sounds/Clic3.mp3");

		_death1 = GD.Load<AudioStream>("res://assets/sounds/Death1.mp3");

		_doorOpen = GD.Load<AudioStream>("res://assets/sounds/DoorOpen.mp3");
		_doorClose = GD.Load<AudioStream>("res://assets/sounds/DoorClose.mp3");

		_entityDig1 = GD.Load<AudioStream>("res://assets/sounds/EntityDig1.mp3");
		_entityEmergence1 = GD.Load<AudioStream>("res://assets/sounds/EntityEmerge1.mp3");

		_takeDamage1 = GD.Load<AudioStream>("res://assets/sounds/TakeDamageBetter.mp3");

		_jingle1 = GD.Load<AudioStream>("res://assets/sounds/Jingle1.mp3");
		_jingle2 = GD.Load<AudioStream>("res://assets/sounds/Jingle2.mp3");
		_jingle3 = GD.Load<AudioStream>("res://assets/sounds/Jingle3.mp3");

		GD.Print("[SFX] Sounds loaded!");
	}

	/// <summary>
	/// Internal helper to play a sound stream with current SFX volume settings.
	/// </summary>
	/// <param name="sound">The audio stream to play.</param>
	private void PlaySound(AudioStream sound)
	{
		if (sound == null) return;

		var settings = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
		float volumeLinear = settings?.SfxVolume ?? 1.0f;

		var player = new AudioStreamPlayer();
		AddChild(player);
		player.Stream = sound;
		player.VolumeDb = Mathf.LinearToDb(volumeLinear);
		player.Play();

		// Remove player when sound finishes
		player.Finished += () => player.QueueFree();
	}

	/// <summary>
	/// Plays a random click sound effect.
	/// </summary>
	public void PlayClickSound()
	{
		var sounds = new List<AudioStream> { _clic1, _clic2, _clic3 };
		sounds.RemoveAll(s => s == null);

		if (sounds.Count > 0)
		{
			PlaySound(sounds[_random.Next(sounds.Count)]);
		}
	}

	/// <summary>
	/// Plays the entity death sound effect.
	/// </summary>
	public void PlayDeathSound()
	{
		PlaySound(_death1);
	}

	/// <summary>
	/// Plays the door opening sound effect.
	/// </summary>
	public void PlayDoorOpenSound()
	{
		PlaySound(_doorOpen);
	}

	/// <summary>
	/// Plays the door closing sound effect.
	/// </summary>
	public void PlayDoorCloseSound()
	{
		PlaySound(_doorClose);
	}

	/// <summary>
	/// Plays the entity digging sound effect.
	/// </summary>
	public void PlayEntityDigSound()
	{
		PlaySound(_entityDig1);
	}

	/// <summary>
	/// Plays the entity emergence sound effect.
	/// </summary>
	public void PlayEntityEmergenceSound()
	{
		PlaySound(_entityEmergence1);
	}

	/// <summary>
	/// Plays the player take damage sound effect.
	/// </summary>
	public void PlayTakeDamageSound()
	{
		// Currently only one damage sound loaded, logic simplified
		PlaySound(_takeDamage1);
	}

	/// <summary>
	/// Plays a random jingle sound effect for wave completion.
	/// </summary>
	public void PlayJingleSound()
	{
		var sounds = new List<AudioStream> { _jingle1, _jingle2, _jingle3 };
		sounds.RemoveAll(s => s == null);

		if (sounds.Count > 0)
		{
			PlaySound(sounds[_random.Next(sounds.Count)]);
		}
	}}
