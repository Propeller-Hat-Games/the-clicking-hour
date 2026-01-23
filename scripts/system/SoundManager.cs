using Godot;
using System;
using System.Collections.Generic;

public partial class SoundManager : Node
{
	private static SoundManager instance;
	private List<AudioStreamPlayer> audioPlayers = new List<AudioStreamPlayer>();
	private int maxPlayers = 10; // Nombre max de sons simultanés
	private Random random = new Random();

	// Sons de clic
	private List<AudioStream> clickSounds = new List<AudioStream>();
	
	// Autres sons
	private AudioStream deathSound;
	private AudioStream doorOpenSound;
	private AudioStream doorCloseSound;
	private AudioStream entityDigSound;
	private AudioStream entityEmergenceSound;
	private AudioStream takeDamageSound1;
	private AudioStream takeDamageSound2;
	private AudioStream takeDamageSound3;

	public static SoundManager Instance
	{
		get { return instance; }
	}

	public override void _Ready()
	{
		instance = this;
		
		// Créer un pool d'AudioStreamPlayers
		for (int i = 0; i < maxPlayers; i++)
		{
			var player = new AudioStreamPlayer();
			AddChild(player);
			audioPlayers.Add(player);
		}

		LoadSounds();
	}

	private void LoadSounds()
	{
		// Charger les sons de clic (Clic1, Clic2, Clic3)
		clickSounds.Add(GD.Load<AudioStream>("res://assets/sounds/Clic1.mp3"));
		clickSounds.Add(GD.Load<AudioStream>("res://assets/sounds/Clic2.mp3"));
		clickSounds.Add(GD.Load<AudioStream>("res://assets/sounds/Clic3.mp3"));

		// Charger les autres sons
		deathSound = GD.Load<AudioStream>("res://assets/sounds/Death1.mp3");
		doorOpenSound = GD.Load<AudioStream>("res://assets/sounds/DoorOpen.mp3");
		doorCloseSound = GD.Load<AudioStream>("res://assets/sounds/DoorClose.mp3");
		entityDigSound = GD.Load<AudioStream>("res://assets/sounds/EntityDig1.mp3");
		entityEmergenceSound = GD.Load<AudioStream>("res://assets/sounds/EntityEmergence1.mp3");
		
		// Sons de dégâts (on peut en choisir un aléatoirement)
		takeDamageSound1 = GD.Load<AudioStream>("res://assets/sounds/TakeDamage1.mp3");
		takeDamageSound2 = GD.Load<AudioStream>("res://assets/sounds/TakeDamage2.mp3");
		takeDamageSound3 = GD.Load<AudioStream>("res://assets/sounds/TakeDamage3.mp3");
	}

	private AudioStreamPlayer GetAvailablePlayer()
	{
		// Trouver un lecteur qui ne joue pas
		foreach (var player in audioPlayers)
		{
			if (!player.Playing)
			{
				return player;
			}
		}
		// Si tous jouent, prendre le premier
		return audioPlayers[0];
	}

	public void PlayClickSound()
	{
		if (clickSounds.Count == 0) return;
		
		var player = GetAvailablePlayer();
		var randomSound = clickSounds[random.Next(clickSounds.Count)];
		player.Stream = randomSound;
		player.Play();
	}

	public void PlayDeathSound()
	{
		if (deathSound == null) return;
		var player = GetAvailablePlayer();
		player.Stream = deathSound;
		player.Play();
	}

	public void PlayDoorOpenSound()
	{
		if (doorOpenSound == null) return;
		var player = GetAvailablePlayer();
		player.Stream = doorOpenSound;
		player.Play();
	}

	public void PlayDoorCloseSound()
	{
		if (doorCloseSound == null) return;
		var player = GetAvailablePlayer();
		player.Stream = doorCloseSound;
		player.Play();
	}

	public void PlayEntityDigSound()
	{
		if (entityDigSound == null) return;
		var player = GetAvailablePlayer();
		player.Stream = entityDigSound;
		player.Play();
	}

	public void PlayEntityEmergenceSound()
	{
		if (entityEmergenceSound == null) return;
		var player = GetAvailablePlayer();
		player.Stream = entityEmergenceSound;
		player.Play();
	}

	public void PlayTakeDamageSound()
	{
		List<AudioStream> damageSounds = new List<AudioStream>
		{
			takeDamageSound1,
			takeDamageSound2,
			takeDamageSound3
		};
		
		damageSounds.RemoveAll(s => s == null);
		if (damageSounds.Count == 0) return;

		var player = GetAvailablePlayer();
		var randomSound = damageSounds[random.Next(damageSounds.Count)];
		player.Stream = randomSound;
		player.Play();
	}

	public void PlaySound(AudioStream sound, float volumeDb = 0f)
	{
		if (sound == null) return;
		var player = GetAvailablePlayer();
		player.Stream = sound;
		player.VolumeDb = volumeDb;
		player.Play();
	}
}