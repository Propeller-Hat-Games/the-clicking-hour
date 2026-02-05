using Godot;
using System;
using System.Threading.Tasks;

public enum EntityState
{
    Walking,
    Hiding,
    Stunned,
}

/// <summary>
/// Base class for all interactive entities in the game.
/// Handles movement, animation, and life cycle.
/// </summary>
public abstract partial class Entity : CharacterBody2D
{
    [Export]
    public float walkSpeed { get; set; } = 300f;

    [Export]
    protected float spawnDelay { get; set; } = 1f;

    [Export]
    protected Sprite2D glass { get; set; }

    public string GlassType { get; set; } = "";

    [Export]
    protected AnimatedSprite2D sprite { get; set; }

    [Export]
    private Area2D clickArea { get; set; }

    protected EntityState CurrentState { get; set; } = EntityState.Walking;
    protected Vector2 WalkDirection { get; set; } = Vector2.Right;
    protected int Hearts { get; set; } = 1;
    public bool IsAlive => Hearts > 0;
    protected bool IsDisappearing { get; set; } = false;
    protected string AnimPrefix { get; set; } = "normal";
    public bool HeadingToDoor { get; set; } = true;

    private float _spawnTimer = 0f;
    protected Vector2 _glassInitialPos;
    private Node2D _door;
    private SfxManager _sfxManager;
    private Node2D _trash;
    private float _targetYOffset;

    /// <summary>
    /// Initializes the entity, sets up references and initial state.
    /// </summary>
    public override void _Ready()
    {
        _sfxManager = GetNode<SfxManager>("/root/SfxManager");
        _door = GetTree().GetFirstNodeInGroup("Door") as Node2D;
        _targetYOffset = (float)GD.RandRange(-20.0, 20.0);
        MotionMode = MotionModeEnum.Floating;
        
        // Cache Trash node efficiently
        _trash = GetTree().GetFirstNodeInGroup("Trash") as Node2D;
        if (_trash == null)
        {
            _trash = GetTree().Root.FindChild("Trash", true, false) as Node2D;
        }

        InitializeEntity();
        UpdateAnimation();

        if (glass != null)
        {
            _glassInitialPos = glass.Position;
            AnimateGlassSpawn();
        }
    }

    /// <summary>
    /// Initializes specific entity properties. Must be implemented by subclasses.
    /// </summary>
    protected abstract void InitializeEntity();

    /// <summary>
    /// Called when the entity is clicked. Must be implemented by subclasses.
    /// </summary>
    protected abstract void OnClicked();

    /// <summary>
    /// Updates the glass sprite texture and type.
    /// </summary>
    public void UpdateGlassType(string type, Sprite2D spriteTemplate)
    {
        GlassType = type;
        if (glass != null && spriteTemplate != null)
        {
            glass.Texture = spriteTemplate.Texture;
        }
    }

    /// <summary>
    /// Animates the glass bobbing while the entity is walking.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    private void AnimateGlassWalking(double delta)
    {
        if (glass == null) return;

        if (CurrentState == EntityState.Walking && sprite != null)
        {
            // Sync glass bobbing with walking frames: 2 pixels offset on every other frame
            float yOffset = (sprite.Frame % 2 != 0) ? -1.0f : 1.0f;
            glass.Position = _glassInitialPos + new Vector2(0, yOffset);
        }
        else if (CurrentState != EntityState.Hiding)
        {
            glass.Position = glass.Position.Lerp(_glassInitialPos, (float)delta * 10.0f);
        }
    }

    /// <summary>
    /// Animates the glass spawning effect.
    /// </summary>
    private void AnimateGlassSpawn()
    {
        if (glass == null) return;

        glass.Position = _glassInitialPos + new Vector2(0, 50);
        glass.Modulate = new Color(glass.Modulate, 1.0f);

        float duration = spawnDelay;

        if (sprite != null && sprite.SpriteFrames != null)
        {
            string animName = $"{AnimPrefix}_jump";
            if (sprite.SpriteFrames.HasAnimation(animName))
            {
                int frameCount = sprite.SpriteFrames.GetFrameCount(animName);
            }
        }

        var tween = CreateTween();
        tween.TweenProperty(glass, "position", _glassInitialPos, duration)
            .SetTrans(Tween.TransitionType.Quart)
            .SetEase(Tween.EaseType.Out);
    }

    /// <summary>
    /// Animates the glass disappearance when the entity leaves or is removed.
    /// </summary>
    private void AnimateGlassDisappearance()
    {
        if (glass == null) return;

        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(glass, "position", _glassInitialPos + new Vector2(0, 50), 0.5f)
            .SetTrans(Tween.TransitionType.Linear);
        tween.TweenProperty(glass, "modulate:a", 0.0f, 0.5f)
            .SetTrans(Tween.TransitionType.Linear);
    }

    /// <summary>
    /// Handles a click attempt on this entity.
    /// </summary>
    /// <param name="game">The current game manager instance.</param>
    public void TryClick(GameManager game)
    {
        if (Hearts > 0)
        {
            Hearts--;
            _sfxManager.PlayClickSound();
            if (Hearts <= 0)
            {
                Die();
                game.EntitiesKilled++;
            }
            else
            {
                OnClicked();
            }
        }
    }

    /// <summary>
    /// Handles physics processing, movement, and animation updates.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    public override void _PhysicsProcess(double delta)
    {
        if (Hearts <= 0) return;

        UpdateAnimation();

        if (_spawnTimer < spawnDelay)
        {
            _spawnTimer += (float)delta;
            return;
        }

        ProcessEntity(delta);
        AnimateGlassWalking(delta);

        switch (CurrentState)
        {
            case EntityState.Walking:
                if (_door != null && HeadingToDoor)
                {
                    Vector2 doorPos = _door.GlobalPosition;
                    Vector2 targetPos = doorPos;
                    targetPos.Y += _targetYOffset;

                    // Improved pathfinding to avoid "Wall Left" and "Wall Right" flanking the door
                    // Entities aim for their "lane" (Y + offset) at a point 150px before the door
                    float distToDoorX = doorPos.X - GlobalPosition.X;
                    
                    if (distToDoorX > 20)
                    {
                        targetPos.X = doorPos.X - 20;
                    }
                    else
                    {
                        // Once close enough and aligned, aim BEYOND the door to ensure crossing it
                        targetPos.X = doorPos.X + 1000;
                    }

                    WalkDirection = (targetPos - GlobalPosition).Normalized();
                }
                Velocity = WalkDirection * walkSpeed;
                MoveAndSlide();
                break;
            case EntityState.Stunned:
                Velocity = Vector2.Zero;
                break;
        }
    }

    /// <summary>
    /// Disables interactions and collisions when the entity enters the door.
    /// </summary>
    public void SetEnteredDoor()
    {
        HeadingToDoor = false;
        ZIndex = -1;

        // Disable clicking and collisions safely
        SetDeferred(PropertyName.CollisionLayer, 0);
        SetDeferred(PropertyName.CollisionMask, 0);
        if (clickArea != null)
        {
            clickArea.SetDeferred(Area2D.PropertyName.Monitoring, false);
            clickArea.SetDeferred(Area2D.PropertyName.Monitorable, false);
            clickArea.InputPickable = false;
        }
    }

    /// <summary>
    /// Custom entity processing logic. To be overridden by subclasses.
    /// </summary>
    /// <param name="delta">Time since last frame.</param>
    protected virtual void ProcessEntity(double delta) { }

    /// <summary>
    /// Updates the entity's animation based on state and timers.
    /// </summary>
    protected virtual void UpdateAnimation()
    {
        if (sprite == null) return;

        if (_spawnTimer < spawnDelay)
        {
            // Play jump animation backwards for spawning (emerging)
            PlaySyncedAnimation($"{AnimPrefix}_jump", true, spawnDelay);
            return;
        }

        if (CurrentState == EntityState.Walking)
        {
            PlaySyncedAnimation($"{AnimPrefix}_walk");

            if (WalkDirection.X != 0)
            {
                sprite.FlipH = WalkDirection.X < 0;
            }
        }
        else if (CurrentState == EntityState.Stunned)
        {
            PlaySyncedAnimation($"{AnimPrefix}_hurt");
        }
    }

    /// <summary>
    /// Plays an animation with speed synchronization.
    /// </summary>
    /// <param name="animName">The name of the animation to play.</param>
    /// <param name="backwards">Whether to play the animation backwards.</param>
    /// <param name="duration">Total duration for the animation to play.</param>
    protected void PlaySyncedAnimation(string animName, bool backwards = false, float duration = -1f)
    {
        if (sprite == null || !sprite.SpriteFrames.HasAnimation(animName)) return;
        if (IsDisappearing && animName != "disapear") return;

        float speed = 1.0f;
        if (duration > 0)
        {
            float animDuration = (float)(sprite.SpriteFrames.GetFrameCount(animName) / sprite.SpriteFrames.GetAnimationSpeed(animName));
            speed = animDuration / duration;
        }

        float finalSpeed = backwards ? -speed : speed;

        // Only play if not already playing that animation, OR if we need to enforce speed/direction
        if (sprite.Animation != animName)
        {
            sprite.Play(animName, finalSpeed, backwards);
        }
        else if (!Mathf.IsEqualApprox(sprite.GetPlayingSpeed(), finalSpeed))
        {
            // If speed changed, update it. Play() handles this without resetting frame if anim is same.
            sprite.Play(animName, finalSpeed, backwards);
        }
    }

    /// <summary>
    /// Handles the entity's death, playing sound and animating it towards the trash.
    /// </summary>
    protected void Die()
    {
        CurrentState = EntityState.Hiding;

        _sfxManager.PlayDeathSound();

        SetDeferred(PropertyName.CollisionLayer, 0);
        SetDeferred(PropertyName.CollisionMask, 0);

        if (clickArea != null)
        {
            clickArea.SetDeferred(Area2D.PropertyName.Monitoring, false);
            clickArea.SetDeferred(Area2D.PropertyName.Monitorable, false);
            clickArea.InputPickable = false;
        }

        Vector2 targetPos = new Vector2(64, 602);
        if (_trash != null)
        {
            targetPos = _trash.GlobalPosition;
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

    /// <summary>
    /// Asynchronously handles the entity's disappearance animation.
    /// </summary>
    public async Task Disappear()
    {
        if (IsDisappearing) return;
        IsDisappearing = true;

        SetDeferred(PropertyName.CollisionLayer, 0);
        SetDeferred(PropertyName.CollisionMask, 0);
        Velocity = Vector2.Zero;
        SetPhysicsProcess(false);

        AnimateGlassDisappearance();

        if (sprite != null && sprite.SpriteFrames.HasAnimation("disapear"))
        {
            sprite.Play("disapear");

            // Local function to wrap SignalAwaiter into a Task
            async Task WaitForAnimation()
            {
                try
                {
                    await ToSignal(sprite, AnimatedSprite2D.SignalName.AnimationFinished);
                }
                catch (ObjectDisposedException) { }
            }

            // Safer wait: wait for signal OR a timeout
            await Task.WhenAny(
                WaitForAnimation(),
                Task.Delay(1000)
            );
        }

        QueueFree();
    }

    /// <summary>
    /// Returns the type of glass carried by this entity.
    /// </summary>
    public string GetGlassType()
    {
        return GlassType;
    }

    /// <summary>
    /// Sets the walking direction for this entity.
    /// </summary>
    /// <param name="direction">Normalized direction vector.</param>
    public void SetWalkDirection(Vector2 direction)
    {
        WalkDirection = direction.Normalized();
    }

    /// <summary>
    /// Sets the movement speed of the entity.
    /// </summary>
    /// <param name="speed">Speed in pixels per second.</param>
    public void SetSpeed(float speed)
    {
        walkSpeed = speed;
    }

    /// <summary>
    /// Gets the current movement speed of the entity.
    /// </summary>
    public float GetSpeed()
    {
        return walkSpeed;
    }
}
