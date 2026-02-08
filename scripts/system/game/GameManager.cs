using Godot;
using System;

/// <summary>
/// Central manager for the game loop, linking all systems and state.
/// Split into multiple partial files for better organization.
/// </summary>
public partial class GameManager : Node2D 
{
	private SettingsManager Settings { get; set; }
	private MusicManager Musics { get; set; }
	private SfxManager SFX { get; set; }
	private Random Rng { get; } = new Random();

	[Export]
	private Door door;
	[Export]
	private MainMenu mainMenu;
	[Export]
	private Node2D trash;
	[Export]
	private GlitchEffect glitchEffect;
	[Export]
	private Button settingsButton;
	[Export]
	private PackedScene pauseMenuScene;

	/// <summary>
	/// Initializes the game manager, sets up dependencies and initial state.
	/// </summary>
	public override async void _Ready() 
	{
		Settings = GetNode<SettingsManager>("/root/SettingsManager");
		Musics = GetNode<MusicManager>("/root/MusicManager");
		SFX = GetNode<SfxManager>("/root/SfxManager");
		
		// These methods are defined in other partial files
		LoadConditionsManager();
		LoadVFX();
		LoadEntities();
	
		YSortEnabled = true;

		ScreenFadeIn();

		mainMenu.PlayButtonPressed += PlayButtonPressed;
		trash.AddToGroup("Trash");
		trash.ZIndex = 100;

		Settings.SettingsChanged += UpdateEffectsVisibility;
		UpdateEffectsVisibility();

		glitchEffect.SetDesaturation(0.0f);

		settingsButton.Pressed += SettingsButtonPressed;

		await Musics.PlayMenuMusic();
		StartHeartAnimationLoop();
	}

	/// <summary>
	/// Performs a fade-in effect from black when the game starts.
	/// </summary>
	private void ScreenFadeIn()
	{
		var fadeLayer = new CanvasLayer();
		fadeLayer.Layer = 128; // On top of everything
		var fadeRect = new ColorRect();
		fadeRect.Color = Colors.Black;
		fadeRect.Modulate = new Color(1, 1, 1, 1); // Start fully opaque
		fadeRect.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
		fadeLayer.AddChild(fadeRect);
		AddChild(fadeLayer);

		var fadeTween = CreateTween();
		fadeTween.TweenProperty(fadeRect, "modulate:a", 0f, 1.5f);
		fadeTween.Finished += () => fadeLayer.QueueFree();
	}

	/// <summary>
	/// Callback for when the play button is pressed on the main menu.
	/// </summary>
	private void PlayButtonPressed()
	{
		mainMenu.Close();
		StartGame();
	}

	/// <summary>
	/// Callback for when the settings button is pressed, opening the pause menu.
	/// </summary>
	private void SettingsButtonPressed()
	{
		var pauseMenu = pauseMenuScene.Instantiate<Control>();
		AddChild(pauseMenu);
		GetTree().Paused = true;
		Musics.SetPauseEffect(true);
	}
}
