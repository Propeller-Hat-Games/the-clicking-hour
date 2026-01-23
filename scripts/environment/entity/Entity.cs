using Godot;
using System;

public enum EntityState
{
    Walking, 
    Returning, 
    Hiding,
}

// Classe de base abstraite pour toutes les entités
public abstract partial class Entity : CharacterBody2D
{
    [Export]
    protected float walkSpeed = 100f; 
    
    [Export]
    protected float spawnDelay = 2f; // Temps d'attente avant de bouger
    
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
                
            case EntityState.Returning:
                Position += walkDirection * walkSpeed * (float)delta;
                
                if (Position.X < -100 || Position.X > GetViewportRect().Size.X + 100)
                {
                    QueueFree(); 
                }
                break;
        }
    }
    
    // Méthode virtuelle pour comportements spécifiques durant _Process
    protected virtual void ProcessEntity(double delta) { }
    
    protected void Die()
    {
        isAlive = false;
        currentState = EntityState.Returning;
        walkDirection = -walkDirection;
    }

    public void SetWalkDirection(Vector2 direction)
    {
        walkDirection = direction.Normalized();
    }
}