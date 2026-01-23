using Godot;
using System;

public enum EntityState
{
	Walking, 
	Hiding,
}

public abstract partial class Entity : CharacterBody2D
{
	[Export]
	protected float walkSpeed = 100f;
	
	[Export]
	protected float spawnDelay = 2f;

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
		InitializeEntity();

		if (glassScene != null)
		{
			var glassInstance = glassScene.Instantiate<Glass>();
			AddChild(glassInstance);
			float halfHeight = sprite.GetRect().Size.Y * sprite.Scale.Y / 2.0f;
			glassInstance.Position = new Vector2(0, sprite.Position.Y - halfHeight + 45);
			
			var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
			if (gameManager != null)
			{
				glassInstance.SetGlassType(gameManager.GetRandomGlassType());
			}
		}
		
		var clickArea = GetNodeOrNull<Area2D>("ClickArea");
		if (clickArea != null)
		{
			clickArea.InputPickable = true;
			clickArea.InputEvent += OnClickAreaInputEvent;
		}

		CallDeferred(nameof(SetDirectionTowardsDoor));
	}

	private void SetDirectionTowardsDoor()
	{
		if (!IsInsideTree()) return;
		var door = GetTree().GetFirstNodeInGroup("Door") as Node2D;
		if (door != null)
		{
			walkDirection = (door.GlobalPosition - GlobalPosition).Normalized();
		}
	}
	
	protected abstract void InitializeEntity();
	
	protected abstract void OnClicked();
	
	private void OnClickAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && 
			mouseEvent.Pressed && 
			mouseEvent.ButtonIndex == MouseButton.Left &&
			isAlive)
		{
			// 🔊 JOUER LE SON DE CLIC
			SfxManager.Instance?.PlayClickSound();
			OnClicked();
		}
	}
	
	public override void _InputEvent(Viewport viewport, InputEvent @event, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseEvent && 
			mouseEvent.Pressed && 
			mouseEvent.ButtonIndex == MouseButton.Left &&
			isAlive)
		{
			// 🔊 JOUER LE SON DE CLIC
			SfxManager.Instance?.PlayClickSound();
			OnClicked();
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!isAlive) return;

		ZIndex = Math.Min((int)GlobalPosition.Y, 99);

		if (spawnTimer < spawnDelay)
		{
			spawnTimer += (float)delta;
			return;
		}
		
		ProcessEntity(delta);
		
		switch (currentState)
		{
			case EntityState.Walking:
				Velocity = walkDirection * walkSpeed;
				MoveAndSlide();
				break;
		}
	}
	
	protected virtual void ProcessEntity(double delta) { }
	
	protected void Die()
	{
		isAlive = false;
		currentState = EntityState.Hiding;
		
		// 🔊 JOUER LE SON DE MORT
		SfxManager.Instance?.PlayDeathSound();
		
		var gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		if (gameManager != null)
		{
			gameManager.AddKill();
		}

		CollisionLayer = 0;
		CollisionMask = 0;
		
		var clickArea = GetNodeOrNull<Area2D>("ClickArea");
		if (clickArea != null)
		{
			clickArea.Monitoring = false;
			clickArea.Monitorable = false;
			clickArea.InputPickable = false;
		}

		Node2D trash = GetTree().GetFirstNodeInGroup("Trash") as Node2D;
		if (trash == null)
		{
			trash = GetTree().Root.FindChild("Trash", true, false) as Node2D;
		}

		Vector2 targetPos = new Vector2(64, 602);
		if (trash != null)
		{
			targetPos = trash.GlobalPosition;
		}

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenProperty(this, "global_position", targetPos, 0.5f)
			 .SetTrans(Tween.TransitionType.Quad)
			 .SetEase(Tween.EaseType.In);
		tween.TweenProperty(this, "scale", Vector2.Zero, 0.5f);
		
		tween.SetParallel(false);
		tween.Chain().TweenCallback(Callable.From(QueueFree));
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

	public void SetSpeed(float speed)
	{
		walkSpeed = speed;
	}

	public float GetSpeed()
	{
		return walkSpeed;
	}
}
