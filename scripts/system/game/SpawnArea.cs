using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Manages the spawning of entities within a defined area.
/// </summary>
public partial class SpawnArea : Area2D
{
    [Export]
    public CollisionShape2D Area;
    
    private List<Entity> _activeEntities = new List<Entity>();

    /// <summary>
    /// Generates a random position within the defined rectangular spawn area.
    /// </summary>
    /// <returns>A Vector2 representing a random position.</returns>
    public Vector2 GetRandomPosition()
    {
        if (Area?.Shape is not RectangleShape2D rectShape) return Vector2.Zero;

        Vector2 size = rectShape.Size;
        float x = (float)GD.RandRange(-size.X / 2.0, size.X / 2.0);
        float y = (float)GD.RandRange(-size.Y / 2.0, size.Y / 2.0);
        
        return Area.Position + new Vector2(x, y);
    }

    /// <summary>
    /// Checks if a position is valid (not too close to other active entities).
    /// </summary>
    public bool IsPositionValid(Vector2 position, float minDistanceSquared, Entity ignoreEntity = null)
    {
        foreach (var entity in _activeEntities)
        {
            if (!IsInstanceValid(entity) || entity == ignoreEntity) continue;
            if (position.DistanceSquaredTo(entity.Position) < minDistanceSquared) 
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns a valid random position within the spawn area, considering other active entities.
    /// </summary>
    public Vector2 GetValidRandomPosition(float minDist = 250f, int maxAttempts = 50, Entity ignoreEntity = null)
    {
        Vector2 pos = Vector2.Zero;
        for (int i = 0; i < maxAttempts; i++)
        {
            pos = GetRandomPosition();
            float threshold = minDist * (1.0f - (float)i / maxAttempts);
            if (IsPositionValid(pos, threshold * threshold, ignoreEntity)) break;
        }
        return pos;
    }

    /// <summary>
    /// Instantiates and spawns a random entity within the area, applying current wave modifiers.
    /// </summary>
    /// <param name="game">The current game manager instance.</param>
    public void SpawnEntity(GameManager game)
    {
        PackedScene randomScene = game.GetRandomEntity();
        Entity entity = randomScene.Instantiate<Entity>();
        
        // Calculate speed first
        entity.SetSpeed(entity.GetSpeed() * (game.CurrentWave / 3 + 1));
        
        // Find a valid random position
        entity.Position = GetValidRandomPosition();
        AddChild(entity);
        
        string glassType = game.RandomGlassType();
        entity.UpdateGlassType(glassType, game.GetGlassSprite(glassType));

        entity.TreeExited += () => OnEntityTreeExited(entity);
        _activeEntities.Add(entity);
    }

    /// <summary>
    /// Callback for when an entity is removed from the scene tree, cleaning up the active entities list.
    /// </summary>
    /// <param name="entity">The entity that exited.</param>
    private void OnEntityTreeExited(Entity entity)
    {
        if (_activeEntities.Contains(entity))
        {
            _activeEntities.Remove(entity);
        }
    }

    /// <summary>
    /// Asynchronously triggers the disappearance and removal of all currently active entities.
    /// </summary>
    public void KillEveryEntities()
    {
        // Create a copy to iterate safely as list might be modified by TreeExited (though usually safe if we just trigger disappear)
        var entitiesSnapshot = new List<Entity>(_activeEntities);
        
        foreach (var entity in entitiesSnapshot)
        {
            if (IsInstanceValid(entity) && entity.IsAlive)
            {
                if (!entity.HeadingToDoor) continue;
                entity.Disappear();
            }
        }
        _activeEntities.Clear();
    }
}
