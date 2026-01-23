using Godot;
using System;

public enum EntityState
{
    Walking, 
    Hiding,
}

// Classe de base abstraite pour toutes les entités
public abstract partial class Entity : CharacterBody2D
{
    [Export]
    protected float walkSpeed = 100f; 
    
    [Export]
    protected float spawnDelay = 2f; // Temps d'attente avant de bouger

    [Export]
    protected PackedScene glassScene;
    
    protected EntityState currentState = EntityState.Walking;
    protected Sprite2D sprite;
    protected Vector2 walkDirection = Vector2.Right;
    protected int clicksRemaining;
    protected bool isAlive = true;
    protected float spawnTimer = 0f;

    public override void _Ready()
    {
        sprite = GetNode<Sprite2D>("Sprite2D");
        InputPickable = true;
        InitializeEntity();

        if (glassScene != null)
        {
            var glassInstance = glassScene.Instantiate<Glass>();
            AddChild(glassInstance);
            float halfHeight = sprite.GetRect().Size.Y * sprite.Scale.Y / 2.0f;
            // Account for sprite position (e.g. if shifted up)
            glassInstance.Position = new Vector2(0, sprite.Position.Y - halfHeight + 45);
            
            var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
            if (gameManager != null)
            {
                glassInstance.SetGlassType(gameManager.GetRandomGlassType());
            }
        }
        
        CallDeferred(nameof(SetDirectionTowardsDoor));
    }

    private void SetDirectionTowardsDoor()
    {
        var door = GetTree().GetFirstNodeInGroup("Door") as Node2D;
        if (door != null)
        {
            walkDirection = (door.GlobalPosition - GlobalPosition).Normalized();
        }
    }
    
    // Méthode abstraite pour initialiser les paramètres spécifiques de chaque type d'entité
    protected abstract void InitializeEntity();
    
    // Méthode abstraite pour gérer le clic spécifique à chaque type
    protected abstract void OnClicked();
    
    public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
    {
        if (@event is InputEventMouseButton mouseEvent && 
            mouseEvent.Pressed && 
            mouseEvent.ButtonIndex == MouseButton.Left &&
            isAlive)
        {
            OnClicked();
        }
    }

    public override void _Process(double delta)
    {
        // Attendre le délai de spawn avant de bouger
        if (spawnTimer < spawnDelay)
        {
            spawnTimer += (float)delta;
            return;
        }
        
        ProcessEntity(delta);
        
        switch (currentState)
        {
            case EntityState.Walking:
                Position += walkDirection * walkSpeed * (float)delta;
                break;
        }
    }
    
    // Méthode virtuelle pour comportements spécifiques durant _Process
    protected virtual void ProcessEntity(double delta) { }
    
    protected void Die()
    {
        QueueFree();
    }

    public Glass GetGlass()
    {
        foreach(var child in GetChildren())
        {
            if (child is Glass glass)
            {
                return glass;
            }
        }
        return null;
    }

    public void SetWalkDirection(Vector2 direction)
    {
        walkDirection = direction.Normalized();
    }
}