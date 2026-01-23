using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class GameManager : Node2D 
{
	private Board board;
	private Godot.Collections.Array<Sprite2D> glassSprites;
	private int life;
	private List<int> requiredGlassTypes = new List<int>();
	private List<int> requiredGlassCounts = new List<int>();

	private List<Entity> activeEntities = new List<Entity>();
	private bool isDoorOpen;

	private int quota;
	private int currentQuota;
	private int wavesSurvived = 0;
	private int entitiesKilled = 0;
	private int glassPassed = 0;
	
	private float difficulty;
	private int maxEntities;	
	private Random random = new Random();
	private List<PackedScene> entityScenes = new List<PackedScene>();
	private bool isSpawning = false;
	private SpawnArea spawnArea;
	private MainMenu mainMenu;

	private PackedScene heartScene;
	private Node2D heartContainer;
	private List<Node2D> heartNodes = new List<Node2D>();
	
	// 🎵 Le MusicManager
	private MusicManager musicManager;

	public GlassType GetRandomGlassType()
	{
		var values = Enum.GetValues(typeof(GlassType));
		return (GlassType)values.GetValue(random.Next(values.Length));
	}

	private int GetGlassCountForDifficulty(float diff)
	{
		int count = 1 + (int)((diff - 1.0f) * 4);
		return Math.Clamp(count, 1, 3);
	}

	public void AddKill()
	{
		entitiesKilled++;
	}

	public int GetLife()
	{
		return life;
	}

	public void UpdateLife(int amount)
	{
		life += amount;
		GD.Print($"Life updated: {life}");
		// 🔊 JOUER LE SON DE DÉGÂTS SI ON PERD DE LA VIE
		if (amount < 0)
		{
			SfxManager.Instance?.PlayTakeDamageSound();
		}
	
		Affichecoeur();
		if (CheckGameOver())
		{
			GD.Print("Game Over!");
			isSpawning = false;

			foreach (var entity in activeEntities)
			{
				if (IsInstanceValid(entity))
				{
					entity.QueueFree();
				}
			}
			activeEntities.Clear();

			var gameOverScene = GD.Load<PackedScene>("res://scenes/ui/game_over.tscn");
			var gameOverInstance = gameOverScene.Instantiate<GameOver>();
			AddChild(gameOverInstance);
			gameOverInstance.SetWavesSurvived(wavesSurvived, entitiesKilled, glassPassed);
		}
	}

	public bool CheckGameOver()
	{
		return life <= 0;
	}

	public void SelectRequiredGlassTypes(int count)
	{
		requiredGlassTypes.Clear();
		if (count > 4) count = 4;
		
		var allTypes = Enum.GetValues(typeof(GlassType));
		var tempTypes = new List<int>();
		foreach (var type in allTypes)
		{
			tempTypes.Add((int)type);
		}

		while (requiredGlassTypes.Count < count && tempTypes.Count > 0)
		{
			int index = random.Next(tempTypes.Count);
			requiredGlassTypes.Add(tempTypes[index]);
			tempTypes.RemoveAt(index);
		}
	}
	
	public void CloseDoor()
	{
		isDoorOpen = false;
		SfxManager.Instance?.PlayDoorCloseSound();
	}

	public void OpenDoor()
	{
		isDoorOpen = true;
		SfxManager.Instance?.PlayDoorOpenSound();
	}

	public List<int> GetRequiredGlassTypes()
	{
		return requiredGlassTypes;
	}

	public async void StartWave()
	{
		// Difficulty logic
		int glassCount = GetGlassCountForDifficulty(difficulty);
		SelectRequiredGlassTypes(glassCount);
		
		// Setup Quota
		quota = 5 + (int)((difficulty - 1.0f) * 2);
		currentQuota = 0;

		// Distribute quota
		requiredGlassCounts.Clear();
		int remainingQuota = quota;
		for (int i = 0; i < requiredGlassTypes.Count; i++)
		{
			if (i == requiredGlassTypes.Count - 1)
			{
				requiredGlassCounts.Add(remainingQuota);
			}
			else
			{
				int maxShare = remainingQuota - (requiredGlassTypes.Count - 1 - i);
				int share = random.Next(1, maxShare + 1);
				requiredGlassCounts.Add(share);
				remainingQuota -= share;
			}
		}

		if (board != null)
		{
			board.Visible = true;
			board.amountOfSlots = requiredGlassTypes.Count;
			board.ChangeImages(requiredGlassTypes.ToArray());
			board.UpdateCounts(requiredGlassCounts.ToArray());
		}

		Affichecoeur();
		GD.Print($"Starting wave! Difficulty: {difficulty}. Condition Glasses: {glassCount}. Quota needed: {quota}");
		
		// 🎵 Fade in musique pour waves 2+
		if (musicManager != null && difficulty > 1.0f)
		{
			_ = musicManager.FadeInNextTrack(1.5f);
		}
		
		isSpawning = true;
		OpenDoor();
		
		// Continuous spawning loop until wave ends
		while (isSpawning)
		{
			SpawnEntity();
			float delay = (float)GD.RandRange(2.5f, 5.0f) / difficulty;
			delay = Math.Max(0.5f, delay);
			
			if (!IsInsideTree()) return;
			await ToSignal(GetTree().CreateTimer(delay), "timeout");
			
			if (!IsInstanceValid(this) || !IsInsideTree()) return;
		}
	}

	private void SpawnEntity()
	{
		if (entityScenes.Count == 0 || !isSpawning) return;

		var randomScene = entityScenes[0];
		
		int typeOfEnemy = random.Next(10);
				
		if (typeOfEnemy >= difficulty-1) {
			randomScene = entityScenes[0];
		} else {
			int rareType = random.Next(100);
					
			if (rareType >= 50) {
				randomScene = entityScenes[1];
			} else if (rareType >= 25) {
				randomScene = entityScenes[2];
			} else {
				randomScene = entityScenes[3];
			}
		}
				
		var entity = randomScene.Instantiate<Entity>();

		entity.SetSpeed(entity.GetSpeed() * difficulty); 
		
		if (spawnArea != null)
		{
			spawnArea.AddChild(entity);
			entity.Position = GetRandomLocalPositionInArea();
		}
		else
		{
			entity.Position = GetRandomSpawnPosition();
			AddChild(entity);
		}

		entity.TreeExited += () => OnEntityTreeExited(entity);
		activeEntities.Add(entity);
	}

	private void OnEntityTreeExited(Entity entity)
	{
		if (activeEntities.Contains(entity))
		{
			activeEntities.Remove(entity);
		}
	}

	private Vector2 GetRandomLocalPositionInArea()
	{
		if (spawnArea == null) return Vector2.Zero;
		
		Vector2 size = spawnArea.AreaSize();
		float x = (float)(random.NextDouble() * size.X - size.X / 2);
		float y = (float)(random.NextDouble() * size.Y - size.Y / 2);
		
		return new Vector2(x, y);
	}

	private Vector2 GetRandomSpawnPosition()
	{
		var viewportRect = GetViewportRect();
		float margin = 100f;
		Vector2 pos = Vector2.Zero;
		pos.X = -margin;
		pos.Y = (float)random.Next(50, (int)viewportRect.Size.Y - 50);
		return pos;
	}

	private void Affichecoeur()
	{
		if (heartContainer == null) return;

		// Supprime les anciens cœurs si nécessaire
		foreach (var heart in heartNodes)
		{
			if (IsInstanceValid(heart))
			{
				heart.QueueFree();
			}
		}
		heartNodes.Clear();

		if (heartScene == null)
		{
			GD.PrintErr("HeartScene non chargé !");
			return;
		}

		float spacing = 100; // espace entre les cœurs

		// Boucle pour créer les cœurs
		for (int i = 0; i < GetLife(); i++)
		{
			var heart = heartScene.Instantiate<Node2D>();
			heart.Position = new Vector2(i * spacing, 0);
			heartContainer.AddChild(heart);
			heartNodes.Add(heart);
		}
	}

	public override void _Ready() 
	{
		var sfxManager = new SfxManager();
		AddChild(sfxManager);

		board = GetNodeOrNull<Board>("Board");
		spawnArea = GetNodeOrNull<SpawnArea>("SpawnArea");
		heartContainer = GetNodeOrNull<Node2D>("HeartContainer");
		heartScene = GD.Load<PackedScene>("res://scenes/heart.tscn");

		var trash = GetNodeOrNull<Node2D>("Trash");
		if (trash != null)
		{
			trash.AddToGroup("Trash");
			trash.ZIndex = 100;
		}
		
		// 🎵 Créer le MusicManager
		musicManager = new MusicManager();
		AddChild(musicManager);
		
		var glassScene = GD.Load<PackedScene>("res://scenes/glass.tscn");
		var glassInstance = glassScene.Instantiate<Glass>();

		glassSprites = glassInstance.GetSprites();
		
		GD.Print("Loaded sprites:");
		foreach (var sprite in glassSprites) {
			GD.Print($"- {sprite.Name}");
		}

		var door = GetTree().GetFirstNodeInGroup("Door") as Door;
		if (door != null)
		{
			door.EntityEnteredDoor += OnEntityEnteredDoor;
		}
		
		entityScenes.Add(GD.Load<PackedScene>("res://scenes/environment/entity.tscn"));
		entityScenes.Add(GD.Load<PackedScene>("res://scenes/environment/hiding_entity.tscn"));
		entityScenes.Add(GD.Load<PackedScene>("res://scenes/environment/multi_click_entity.tscn"));
		entityScenes.Add(GD.Load<PackedScene>("res://scenes/environment/teleport_entity.tscn"));

		maxEntities = 5; 
		difficulty = 1.0f;
		life = 3;
		
		mainMenu = GetNodeOrNull<MainMenu>("MainMenu");
		if (mainMenu != null)
		{
			mainMenu.GameStarted += StartGame;
		}
		else 
		{
			StartGameDirectly();
		}
	}
	
	private void StartGame()
	{
		if (mainMenu != null && IsInstanceValid(mainMenu))
		{
			mainMenu.QueueFree();
		}
		
		// 🎵 Démarre la musique
		if (musicManager != null)
		{
			_ = musicManager.StartMusic(1.5f);
		}
		
		StartWave();
	}

	private void StartGameDirectly()
	{
		// 🎵 Démarre la musique
		if (musicManager != null)
		{
			_ = musicManager.StartMusic(1.5f);
		}
		
		StartWave();
	}

	private void OnEntityEnteredDoor(Entity entity)
	{
		if (!isSpawning) return;

		Glass glass = entity.GetGlass();
		if (glass != null)
		{
			GlassType type = glass.GetGlassType();
			int typeIdx = requiredGlassTypes.IndexOf((int)type);
			if (typeIdx != -1)
			{
				if (requiredGlassCounts[typeIdx] > 0)
				{
					requiredGlassCounts[typeIdx]--;
					currentQuota++;
					glassPassed++;
					board.UpdateCounts(requiredGlassCounts.ToArray());
					GD.Print($"Entity passed! Quota: {currentQuota}/{quota}");
					
					if (currentQuota >= quota)
					{
						EndWave();
					}
				}
				else
				{
					GD.Print("Already have enough of this type! -1 Life");
					UpdateLife(-1);
				}
			}
			else
			{
				GD.Print("Wrong entity entered! -1 Life");
				UpdateLife(-1);
			}
		}
	}

	private async void EndWave()
	{
		isSpawning = false;
		CloseDoor();

		if (board != null)
		{
			board.ClearDisplay();
		}
		
		if (musicManager != null)
		{
			_ = musicManager.FadeOutAndSwitchTrack(1.5f);
		}
		
		foreach (var entity in activeEntities)
		{
			if (IsInstanceValid(entity))
			{
				entity.QueueFree();
			}
		}
		activeEntities.Clear();

		wavesSurvived++;
		GD.Print($"Wave {wavesSurvived} Finished!");
		
		// Afficher l'écran de transition
		var transitionScene = GD.Load<PackedScene>("res://scenes/ui/transition.tscn");
		var transitionInstance = transitionScene.Instantiate<Transition>();
		AddChild(transitionInstance);
		transitionInstance.SetCompletedWave(wavesSurvived);
		
		// Wait 3 seconds
		await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
		transitionInstance.CloseWindow();
		
		if (!IsInstanceValid(this) || !IsInsideTree()) return;
		if (CheckGameOver()) return;

		difficulty += 0.25f;
		StartWave();
	}
}
