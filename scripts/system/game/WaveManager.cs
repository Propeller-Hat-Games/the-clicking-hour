using Godot;
using System;
using System.Threading.Tasks;

public partial class GameManager
{
    public int CurrentWave { get; protected set; }
    private bool IsNightMode { get; set; } = false;
    public int EntitiesKilled { get; set; }
    public int GlassPassed { get; set; }
    
    [Export]
    private SpawnArea Spawn;
    
    private bool IsSpawning { get; set; } = false;
    
    [Export]
    private Sprite2D Unboarding;
    
    private TaskCompletionSource<bool> _unboardingClosedTcs;

    /// <summary>
    /// Starts the main game loop, initializing wave and stats.
    /// </summary>
    public async void StartGame()
    {
        CurrentWave = 0;
        Hearts = 3;
        UpdateHearts();
        EntitiesKilled = 0;
        GlassPassed = 0;

        Musics.FadeOut();

        if (!Settings.HasSeenOnboarding)
        {
            Unboarding.Visible = true;
            _unboardingClosedTcs = new TaskCompletionSource<bool>();
            await _unboardingClosedTcs.Task;
            Settings.HasSeenOnboarding = true;
        }

        NextWave();
    }

    /// <summary>
    /// Handles unhandled input, specifically mouse clicks for entity interaction or closing onboarding.
    /// </summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.Pressed && 
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            if (Unboarding.Visible)
            {
                Unboarding.Visible = false;
                _unboardingClosedTcs?.TrySetResult(true);
                GetViewport().SetInputAsHandled();
                return;
            }
            HandleEntityClick(mouseEvent.Position);
        }
    }

    /// <summary>
    /// Performs a raycast-like check at the mouse position to find and click entities.
    /// </summary>
    /// <param name="mousePosition">The screen position of the mouse click.</param>
    private void HandleEntityClick(Vector2 mousePosition)
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        var query = new PhysicsPointQueryParameters2D();
        query.Position = GetGlobalMousePosition();
        query.CollideWithAreas = true;
        query.CollideWithBodies = true;
        
        var results = spaceState.IntersectPoint(query);
        
        Entity topEntity = null;
        int maxZ = int.MinValue;
        
        foreach (var result in results)
        {
            var collider = result["collider"].As<Node>();
            Entity entity = null;
            
            if (collider is Entity e) entity = e;
            else if (collider.GetParent() is Entity p) entity = p;
            
            if (entity != null && entity.IsAlive)
            {
                if (entity.ZIndex > maxZ)
                {
                    maxZ = entity.ZIndex;
                    topEntity = entity;
                }
                else if (entity.ZIndex == maxZ)
                {
                     // In case of ZIndex tie, pick the one "visually lower" (higher Y) to simulate depth sorting
                     if (topEntity != null && entity.GlobalPosition.Y > topEntity.GlobalPosition.Y)
                     {
                         topEntity = entity; 
                     }
                     else if (topEntity == null)
                     {
                         topEntity = entity;
                     }
                }
            }
        }
        
        if (topEntity != null)
        {
            topEntity.TryClick(this);
            GetViewport().SetInputAsHandled();
        }
    }

    /// <summary>
    /// Prepares and starts the next wave of entities.
    /// </summary>
    public async void NextWave()
    {
        GenerateConditions();
        CurrentWave++;

        // 30% chance of night mode starting from wave 3
        IsNightMode = CurrentWave > 2 && !IsNightMode && Rng.NextDouble() < 0.3;
        UpdateNightMode();

        await ToSignal(GetTree().CreateTimer(1.0f, false), Timer.SignalName.Timeout);
        if (!IsInsideTree()) return;
        
        door.Open(SFX);
        GD.Print($"[WAVE] Wave {CurrentWave} started!");
        if (IsNightMode) GD.Print($"[WAVE] This wave is night mode!");

        Musics.PlayGameMusic(IsNightMode);

        await ToSignal(GetTree().CreateTimer(1.0f, false), Timer.SignalName.Timeout);
        if (!IsInsideTree()) return;

        IsSpawning = true;

        while (IsSpawning)
        {
            Spawn.SpawnEntity(this);
            float waveModifier = Math.Max(1.0f, CurrentWave / 3.0f);
            float delay = (float)GD.RandRange(1.5f, 3.0f) / waveModifier;
            delay = Math.Max(0.25f, delay);
            
            await ToSignal(GetTree().CreateTimer(delay, false), Timer.SignalName.Timeout);
            if (!IsInsideTree()) return;
            
            if (!IsInstanceValid(this) || !IsInsideTree()) return;
        }
    }

    /// <summary>
    /// Ends the current wave, cleans up entities, and shows the transition screen.
    /// </summary>
    public async void EndWave()
    {
        if (!IsSpawning) return;
        IsSpawning = false;

        GD.Print($"[WAVE] Wave {CurrentWave} finished!");
        
        door.Close(SFX);
        Spawn.KillEveryEntities();
        await Musics.FadeOut();
        if (!IsInsideTree()) return;

        // Wait 0.5 seconds
        await ToSignal(GetTree().CreateTimer(0.5f, false), Timer.SignalName.Timeout);
        if (!IsInsideTree()) return;

        SFX.PlayJingleSound();

        // Show transition screen
        var transitionScene = GD.Load<PackedScene>("res://scenes/ui/transition.tscn");
        var transitionInstance = transitionScene.Instantiate<Transition>();
        AddChild(transitionInstance);
        transitionInstance.SetCompletedWave(CurrentWave);
        
        // Wait 5 seconds (transition + jingle)
        await ToSignal(GetTree().CreateTimer(5.0f, false), Timer.SignalName.Timeout);
        if (!IsInsideTree()) return;

        transitionInstance.CloseWindow();
        
        if (!IsInstanceValid(this) || !IsInsideTree()) return;

        NextWave();
    }

    /// <summary>
    /// Ends the game, showing the game over screen and final stats.
    /// </summary>
    public async Task EndGame()
    {
        IsSpawning = false;
        door.Close(SFX);
        Spawn.KillEveryEntities();
        await Musics.FadeOut();

        var gameOverScene = GD.Load<PackedScene>("res://scenes/ui/game_over.tscn");
        var gameOverInstance = gameOverScene.Instantiate<GameOver>();
        AddChild(gameOverInstance);
        gameOverInstance.SetWavesSurvived(CurrentWave - 1, EntitiesKilled, GlassPassed);
        await Musics.PlayMenuMusic();
    }
}
