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

    private List<Entity> activeEntities = new List<Entity>();
    private bool isDoorOpen;

    private int quota;
    private int currentQuota;

    private float difficulty;
    private int maxEntities;
    
    private Random random = new Random();
    private List<PackedScene> entityScenes = new List<PackedScene>();
    private bool isSpawning = false;
    private SpawnArea spawnArea;

    // We can just use the Enum for glass types, no need for a static list 0-3
    
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
        if (CheckGameOver())
        {
            GD.Print("Game Over!");
            // Handle Game Over (e.g. show menu, restart)
            GetTree().ReloadCurrentScene();
        }
    }
    
    public bool CheckGameOver()
    {
        return life <= 0;
    }

    public void SelectRequiredGlassTypes(int count)
    {
        requiredGlassTypes.Clear();
        // Ensure we don't request more than available types (4)
        if (count > 4) count = 4;
        
        var allTypes = Enum.GetValues(typeof(GlassType));
        var tempTypes = new List<int>();
        foreach (var type in allTypes)
        {
            tempTypes.Add((int)type);
        }

        // Populate requiredGlassTypes with unique random types
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
        // Play animation
    }

    public void OpenDoor()
    {
        isDoorOpen = true;
        // Play animation
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
        
        if (board != null)
        {
            board.Visible = true;
            board.amountOfSlots = requiredGlassTypes.Count;
            board.ChangeImages(requiredGlassTypes.ToArray());
        }

        // Setup Quota
        quota = 5 + (int)((difficulty - 1.0f) * 2);
        currentQuota = 0;

        GD.Print($"Starting wave! Difficulty: {difficulty}. Condition Glasses: {glassCount}. Quota needed: {quota}");
        isSpawning = true;
        OpenDoor();
        
        // Continuous spawning loop until wave ends
        while (isSpawning)
        {
            SpawnEntity();
            // Random delay between spawns, decreases with difficulty
            float delay = (float)GD.RandRange(1.5f, 3.0f) / difficulty;
            delay = Math.Max(0.5f, delay);
            await ToSignal(GetTree().CreateTimer(delay), "timeout");
            
            // Safety check if scene changed or game over
            if (!IsInstanceValid(this)) return;
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

    public override void _Ready() 
    {
        board = GetNodeOrNull<Board>("Board");
        spawnArea = GetNodeOrNull<SpawnArea>("SpawnArea");
        var glassScene = GD.Load<PackedScene>("res://scenes/glass.tscn");
        var glassInstance = glassScene.Instantiate<Glass>();

        glassSprites = glassInstance.GetSprites();
        
        GD.Print("Loaded sprites:");
        foreach (var sprite in glassSprites) {
            GD.Print($"- {sprite.Name}");
        }

        // Connect door signal
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
        life = 3; // Initialize life
        
        var mainMenu = GetNodeOrNull<MainMenu>("MainMenu");
        if (mainMenu != null)
        {
            mainMenu.GameStarted += StartGame;
        }
        else 
        {
            StartWave();
        }
    }
    
    private void StartGame()
    {
        StartWave();
    }

    private void OnEntityEnteredDoor(Entity entity)
    {
        if (!isSpawning) return; // Ignore if wave ended

        Glass glass = entity.GetGlass();
        if (glass != null)
        {
            GlassType type = glass.GetGlassType();
            if (requiredGlassTypes.Contains((int)type))
            {
                // Condition met!
                currentQuota++;
                GD.Print($"Entity passed! Quota: {currentQuota}/{quota}");
                
                if (currentQuota >= quota)
                {
                    EndWave();
                }
            }
            else
            {
                // Condition failed!
                GD.Print("Wrong entity entered! -1 Life");
                UpdateLife(-1);
            }
        }
    }

    private async void EndWave()
    {
        isSpawning = false;
        CloseDoor();
        
        foreach (var entity in activeEntities)
        {
            if (IsInstanceValid(entity))
            {
                entity.QueueFree();
            }
        }
        activeEntities.Clear();

        GD.Print("Wave Finished! Next wave in 3s...");
        await ToSignal(GetTree().CreateTimer(3.0f), "timeout");
        
        difficulty += 0.5f;
        StartWave();
    }
}
