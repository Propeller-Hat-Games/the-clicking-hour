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
    private Random _rng = new Random();

    /// <summary>
    /// Generates a random position within the defined rectangular spawn area.
    /// </summary>
    /// <returns>A Vector2 representing a random position.</returns>
    private Vector2 RandomPosition()
    {
        if (Area == null || Area.Shape is not RectangleShape2D rectShape) return Vector2.Zero;

        Vector2 size = rectShape.Size;
        float x = (float)(_rng.NextDouble() * size.X - size.X / 2);
        float y = (float)(_rng.NextDouble() * size.Y - size.Y / 2);
        
        return new Vector2(x, y);
    }

    /// <summary>
    /// Instantiates and spawns a random entity within the area, applying current wave modifiers.
    /// </summary>
    /// <param name="game">The current game manager instance.</param>
    public void SpawnEntity(GameManager game)
    {
        PackedScene randomScene = game.GetRandomEntity();
        Entity entity = randomScene.Instantiate<Entity>();
        entity.SetSpeed(entity.GetSpeed() * (game.CurrentWave / 3 + 1));
        AddChild(entity);
        
        string glassType = game.RandomGlassType();
        entity.UpdateGlassType(glassType, game.GetGlassSprite(glassType));
        entity.Position = RandomPosition();
        
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
    public async Task KillEveryEntities()
    {
        List<Task> disappearTasks = new List<Task>();
        // Create a copy to iterate safely as list might be modified by TreeExited (though usually safe if we just trigger disappear)
        var entitiesSnapshot = new List<Entity>(_activeEntities);
        
        foreach (var entity in entitiesSnapshot)
        {
            if (IsInstanceValid(entity) && entity.IsAlive)
            {
                if (!entity.HeadingToDoor) continue;
                disappearTasks.Add(entity.Disappear());
            }
        }
        _activeEntities.Clear();

        if (disappearTasks.Count > 0)
        {
            await Task.WhenAny(Task.WhenAll(disappearTasks), Task.Delay(2000));
        }
        
        await Task.Yield();
    }
}
