using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager
{
    private List<PackedScene> _entityScenes = new List<PackedScene>();

    /// <summary>
    /// Loads an entity scene from the given path and adds it to the available pool.
    /// </summary>
    /// <param name="path">The resource path to the entity scene.</param>
    private void LoadEntity(string path)
    {
        _entityScenes.Add(GD.Load<PackedScene>(path));
    }

    /// <summary>
    /// Initializes the entities manager and preloads entity scenes.
    /// </summary>
    public void LoadEntities()
    {
        door.EntityEnteredDoor += EntityEnteredDoor;
        LoadEntity("res://scenes/environment/entity.tscn");
        LoadEntity("res://scenes/environment/hiding_entity.tscn");
        LoadEntity("res://scenes/environment/multi_click_entity.tscn");
        LoadEntity("res://scenes/environment/teleport_entity.tscn");
    }

    /// <summary>
    /// Returns a random entity scene based on the current wave difficulty.
    /// </summary>
    /// <returns>A PackedScene of a random entity.</returns>
    public PackedScene GetRandomEntity()
    {
        if (CurrentWave < 5) return _entityScenes[0];
        int p = Rng.Next(100);
        
        if (p <= 75) return _entityScenes[0];
        if (p <= 85) return _entityScenes[1];
        else if (p <= 95) return _entityScenes[2];
        return _entityScenes[3];
    }

    /// <summary>
    /// Handles an entity entering the door, checking if it matches wave conditions.
    /// </summary>
    /// <param name="entity">The entity that entered the door.</param>
    public void EntityEnteredDoor(Entity entity)
    {
        if (!IsSpawning) return;

        string type = entity.GlassType;
        if (type == "") return;
        
        entity.SetEnteredDoor();
        
        // Delay QueueFree to let it continue its path for a bit visually
        GetTree().CreateTimer(2.0f).Timeout += () => {
            if (IsInstanceValid(entity)) entity.QueueFree();
        };

        GlassPassed++;

        if (TryEnterGlass(type))
        {
            if (IsConditionsDone())
            {
                EndWave();
            }
        } else
        {
            LooseHeart();
        }
    }
}
