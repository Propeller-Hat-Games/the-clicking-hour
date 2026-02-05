using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager
{
    private int Hearts { get; set; }
    
    [Export]
    private Node2D HeartContainer;
    [Export]
    private PackedScene HeartScene;
    
    private List<Node2D> _heartNodes = new List<Node2D>();

    /// <summary>
    /// Updates the visual heart display based on current life.
    /// </summary>
    public void UpdateHearts()
    {
        foreach (var heart in _heartNodes)
        {
            if (IsInstanceValid(heart))
            {
                heart.QueueFree();
            }
        }
        _heartNodes.Clear();

        float spacing = 96;

        for (int i = 0; i < Hearts; i++)
        {
            var heart = HeartScene.Instantiate<Node2D>();
            heart.Position = new Vector2(i * spacing, 0);
            HeartContainer.AddChild(heart);
            _heartNodes.Add(heart);
        }

        UpdateDesaturation(Mathf.Clamp((1.0f - (Hearts / 3.0f)) * 0.5f, 0.0f, 0.5f));
    }

    /// <summary>
    /// Starts an asynchronous loop that animates hearts with a jumping effect.
    /// </summary>
    public async void StartHeartAnimationLoop()
    {
        while (IsInsideTree())
        {
            // If the game is paused, wait until it's unpaused before starting a new wave
            if (GetTree().Paused)
            {
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                continue;
            }

            var currentHearts = new List<Node2D>(_heartNodes);
            foreach (var heart in currentHearts)
            {
                if (IsInstanceValid(heart))
                {
                    Tween tween = CreateTween();
                    // Jump up
                    tween.TweenProperty(heart, "position:y", -15f, 0.15f)
                        .SetTrans(Tween.TransitionType.Sine)
                        .SetEase(Tween.EaseType.Out);
                    // Fall down
                    tween.TweenProperty(heart, "position:y", 0f, 0.4f)
                        .SetTrans(Tween.TransitionType.Bounce)
                        .SetEase(Tween.EaseType.Out);
                }
                
                // Delay between each heart
                if (!IsInsideTree()) return;
                await ToSignal(GetTree().CreateTimer(0.2f), Timer.SignalName.Timeout);

                // If paused during the sequence, we might want to wait here too if needed, 
                // but standard timers and tweens already stop when paused.
            }
            
            // Wait before next wave of animation
            if (!IsInsideTree()) return;
            await ToSignal(GetTree().CreateTimer(3.0f), Timer.SignalName.Timeout);
        }
    }

    /// <summary>
    /// Reduces player health, triggers visual effects, and checks for game over.
    /// </summary>
    public async void LooseHeart()
    {
        Hearts--;
        TriggerGlitchEffect();
        SFX.PlayTakeDamageSound();
        UpdateHearts();
        if (Hearts <= 0)
        {
            await EndGame();
        }
    }
}
