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
	
	private float difficulty;
	private int maxEntities;	
	private Random random = new Random();
	private List<PackedScene> entityScenes = new List<PackedScene>();
	private bool isSpawning = false;
	private SpawnArea spawnArea;
	private MainMenu mainMenu;

	private Texture2D heartTexture;
	private List<Sprite2D> heartSprites = new List<Sprite2D>();
	
	// 🎵 Le MusicManager
	private MusicManager musicManager;

	public GlassType GetRandomGlassType()
	{
		var values = Enum.GetValues(typeof(GlassType));
		return (GlassType)values.GetValue(random.Next(values.Length));
	}

	private int GetGlassCountForDifficulty(float diff)
	{
		int count = 3 - (int)(diff - 1.0f);
		return Math.Clamp(count, 1, 3);
	}

	public int GetLife()
	{
		return life;
	}

	public void UpdateLife(int amount)
	{
		life += amount;
		GD.Print($"Life updated: {life}");
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
			gameOverInstance.SetWavesSurvived(wavesSurvived);
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
	}

	public void OpenDoor()
	{
		isDoorOpen = true;
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
			await musicManager.FadeInNextTrack(1.5f);
		}
		
		isSpawning = true;
		OpenDoor();
		
		// Continuous spawning loop until wave ends
		while (isSpawning)
		{
			SpawnEntity();
			float delay = (float)GD.RandRange(1.5f, 3.0f) / difficulty;
			delay = Math.Max(0.5f, delay);
			
			if (!IsInsideTree()) return;
			await ToSignal(GetTree().CreateTimer(delay), "timeout");
			
			if (!IsInstanceValid(this) || !IsInsideTree()) return;
		}
	}

	private void SpawnEntity()
	{
		if (entityScenes.Count == 0 || !isSpawning) return;

		var randomScene = entityScenes[random.Next(entityScenes.Count)];
		var entity = randomScene.Instantiate<Entity>();
		
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
		// Supprime les anciens cœurs si nécessaire
		foreach (var sprite in heartSprites)
		{
			sprite.QueueFree();
		}
		heartSprites.Clear();

		// Charge la texture
		heartTexture = GD.Load<Texture2D>("res://assets/heart.png");
		if (heartTexture == null)
		{
			GD.PrintErr("Impossible de charger heart.png !");
			return;
		}

		// Position initiale
		float startX = 150;
		float startY = 600;
		float spacing = 50; // espace entre les cœurs

		// Boucle pour créer les cœurs
		for (int i = 0; i < GetLife(); i++)
		{
			var heart = new Sprite2D();
			heart.Texture = heartTexture;
			heart.Position = new Vector2(startX + i * spacing, startY);
			AddChild(heart);
			heartSprites.Add(heart);
		}
	}

	public override void _Ready() 
	{
		board = GetNodeOrNull<Board>("Board");
		spawnArea = GetNodeOrNull<SpawnArea>("SpawnArea");
		
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
	
	private async void StartGame()
	{
		if (mainMenu != null && IsInstanceValid(mainMenu))
		{
			mainMenu.QueueFree();
		}
		
		// 🎵 Démarre la musique
		if (musicManager != null)
		{
			await musicManager.StartMusic(1.5f);
		}
		
		StartWave();
	}

	private async void StartGameDirectly()
	{
		// 🎵 Démarre la musique
		if (musicManager != null)
		{
			await musicManager.StartMusic(1.5f);
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
		
		// 🎵 Fade out la musique
		if (musicManager != null)
		{
			await musicManager.FadeOutAndSwitchTrack(1.5f);
		}
		
		foreach (var entity in activeEntities)
		{
			if (IsInstanceValid(entity))
			{
				entity.QueueFree();
			}
		}
		activeEntities.Clear();

		GD.Print("Wave Finished! Next wave in 3s...");
		
		if (!IsInsideTree()) return;
		await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
		
		if (!IsInstanceValid(this) || !IsInsideTree()) return;
		if (CheckGameOver()) return;

		wavesSurvived++;
		difficulty += 0.5f;
		StartWave();
	}
}
