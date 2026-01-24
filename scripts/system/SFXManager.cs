using Godot;
using System;
using System.Collections.Generic;

public partial class SfxManager : Node
{
	private static SfxManager instance;
	public static SfxManager Instance => instance;

	private Random random = new Random();

	// Sons de clic
	private AudioStream clic1;
	private AudioStream clic2;
	private AudioStream clic3;

	// Son de mort
	private AudioStream death1;

	// Sons de porte
	private AudioStream doorOpen;
	private AudioStream doorClose;

	// Sons d'entité
	private AudioStream entityDig1;
	private AudioStream entityEmergence1;

	// Sons de dégâts
	private AudioStream takeDamage1;
	private AudioStream takeDamage2;
	private AudioStream takeDamage3;

	public override void _ExitTree()
	{
		if (instance == this)
		{
			instance = null;
		}
	}

	public override void _Ready()
	{
		// Force update instance if it's pointing to a freed object or null
		if (instance != null && IsInstanceValid(instance) && instance != this)
		{
			QueueFree();
			return;
		}
		instance = this;

		// Charger tous les sons
		clic1 = GD.Load<AudioStream>("res://assets/sounds/Clic1.mp3");
		clic2 = GD.Load<AudioStream>("res://assets/sounds/Clic2.mp3");
		clic3 = GD.Load<AudioStream>("res://assets/sounds/Clic3.mp3");

		death1 = GD.Load<AudioStream>("res://assets/sounds/Death1.mp3");

		doorOpen = GD.Load<AudioStream>("res://assets/sounds/DoorOpen.mp3");
		doorClose = GD.Load<AudioStream>("res://assets/sounds/DoorClose.mp3");

		entityDig1 = GD.Load<AudioStream>("res://assets/sounds/EntityDig1.mp3");
		entityEmergence1 = GD.Load<AudioStream>("res://assets/sounds/EntityEmerge1.mp3");

		takeDamage1 = GD.Load<AudioStream>("res://assets/sounds/TakeDamageBetter.mp3");

		GD.Print("SfxManager initialisé !");
	}

	private void PlaySound(AudioStream sound)
	{
		if (sound == null)
		{
			GD.PrintErr("Son null !");
			return;
		}

		var player = new AudioStreamPlayer();
		AddChild(player);
		player.Stream = sound;
		player.Play();

		// Supprimer le player quand le son est fini
		player.Finished += () => player.QueueFree();
	}

	public void PlayClickSound()
	{
		var sounds = new List<AudioStream> { clic1, clic2, clic3 };
		sounds.RemoveAll(s => s == null);
		
		if (sounds.Count > 0)
		{
			PlaySound(sounds[random.Next(sounds.Count)]);
		}
	}

	public void PlayDeathSound()
	{
		PlaySound(death1);
	}

	public void PlayDoorOpenSound()
	{
		PlaySound(doorOpen);
	}

	public void PlayDoorCloseSound()
	{
		PlaySound(doorClose);
	}

	public void PlayEntityDigSound()
	{
		PlaySound(entityDig1);
	}

	public void PlayEntityEmergenceSound()
	{
		PlaySound(entityEmergence1);
	}

	public void PlayTakeDamageSound()
	{
		var sounds = new List<AudioStream> { takeDamage1, takeDamage2, takeDamage3 };
		sounds.RemoveAll(s => s == null);
		
		if (sounds.Count > 0)
		{
			PlaySound(sounds[random.Next(sounds.Count)]);
		}
	}
}
