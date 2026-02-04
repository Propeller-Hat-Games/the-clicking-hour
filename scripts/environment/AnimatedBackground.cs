using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Handles the scrolling background animation with parallax effect.
/// </summary>
public partial class AnimatedBackground : Node2D
{
    [Export] public float SpeedBack { get; set; } = 20f;
    [Export] public float SpeedFront { get; set; } = 40f;
    [Export] public float ScreenWidth { get; set; } = 1152f;

    private Sprite2D[] _back;
    private Sprite2D[] _front;

    private AnimatedSprite2D _sprite;

    /// <summary>
    /// Initializes the background animation, starting the animated sprite and layers.
    /// </summary>
    public override void _Ready()
    {
        // AnimatedSprite2D
        _sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
        _sprite?.Play();

        // Background layers
        _back = SafeGetSprites("Sprite2D_Back", "Sprite2D_Back2");
        _front = SafeGetSprites("Sprite2D_Front", "Sprite2D_Front2");
    }

    /// <summary>
    /// Updates the scrolling position for each layer every frame.
    /// </summary>
    public override void _Process(double delta)
    {
        ScrollLayer(_back, SpeedBack, delta);
        ScrollLayer(_front, SpeedFront, delta);

        // Intentionally left without UpdateAnimatedSprite(false) to allow external control if needed
        // or re-enable if it was intended to enforce default state.
        // For now, assume night mode is controlled via signals or other managers calling UpdateAnimatedSprite.
    }

    /// <summary>
    /// Scrolls a set of sprites at a specific speed, wrapping them around the screen.
    /// </summary>
    /// <param name="layer">Array of sprites in the layer.</param>
    /// <param name="speed">Scrolling speed in pixels per second.</param>
    /// <param name="delta">Time since last frame.</param>
    private void ScrollLayer(Sprite2D[] layer, float speed, double delta)
    {
        if (layer == null) return;

        foreach (var sprite in layer)
        {
            if (sprite == null) continue;

            Vector2 pos = sprite.Position;
            pos.X -= speed * (float)delta;

            // Horizontal Loop
            if (pos.X <= -ScreenWidth)
                pos.X += 2 * ScreenWidth;

            sprite.Position = pos;
        }
    }

    /// <summary>
    /// Updates the background animation based on day/night cycle.
    /// </summary>
    /// <param name="isNightMode">True for night animation, False for default.</param>
    public void UpdateAnimatedSprite(bool isNightMode)
    {
        if (_sprite == null) return;

        string targetAnim = isNightMode ? "night" : "default";
        if (_sprite.Animation != targetAnim)
            _sprite.Play(targetAnim);
    }

    /// <summary>
    /// Utility method to safely get multiple Sprite2D nodes by name.
    /// </summary>
    /// <param name="nodeNames">Names of the nodes to retrieve.</param>
    /// <returns>An array of Sprite2D nodes that were found.</returns>
    private Sprite2D[] SafeGetSprites(params string[] nodeNames)
    {
        var list = new List<Sprite2D>();
        foreach (var name in nodeNames)
        {
            var node = GetNodeOrNull<Sprite2D>(name);
            if (node != null)
                list.Add(node);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }
}
