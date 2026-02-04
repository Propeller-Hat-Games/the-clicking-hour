using Godot;
using System;

public partial class GameManager
{
    [Export]
    private GlitchEffect Glitch;
    [Export]
    private Texture2D CursorNormal;
    [Export]
    private Texture2D CursorNight;

    private CanvasLayer _vhsLayer;
    private CanvasLayer _nightModeLayer;
    private ColorRect _nightModeRect;
    [Export]
    private AnimatedBackground background;

    /// <summary>
    /// Updates the night mode shader with current mouse position if active.
    /// </summary>
    public override void _Process(double delta)
    {
        if (IsNightMode && _nightModeRect != null)
        {
            Vector2 mousePos = GetViewport().GetMousePosition();
            (_nightModeRect.Material as ShaderMaterial).SetShaderParameter("mouse_position", mousePos);
        }
    }

    /// <summary>
    /// Initializes all visual effects systems.
    /// </summary>
    public void LoadVFX()
    {
        SetupVHSEffect();
        SetupNightModeEffect();
        UpdateCursor();
    }

    /// <summary>
    /// Sets up the VHS screen overlay effect.
    /// </summary>
    private void SetupVHSEffect()
    {
        _vhsLayer = new CanvasLayer();
        _vhsLayer.Layer = 10; // Ensure it's on top of everything
        AddChild(_vhsLayer);

        var colorRect = new ColorRect();
        colorRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        colorRect.MouseFilter = Control.MouseFilterEnum.Ignore; // Allow clicks to pass through
        
        var shader = GD.Load<Shader>("res://assets/shaders/vhs.gdshader");
        var material = new ShaderMaterial();
        material.Shader = shader;
        colorRect.Material = material;
        _vhsLayer.AddChild(colorRect);
        GD.Print("[VFX] VHS Filter loaded!");
    }

    /// <summary>
    /// Sets up the flashlight/night mode screen overlay effect.
    /// </summary>
    private void SetupNightModeEffect()
    {
        _nightModeLayer = new CanvasLayer();
        _nightModeLayer.Layer = 5; // Under VHS (layer 10) but above game
        AddChild(_nightModeLayer);

        _nightModeRect = new ColorRect();
        _nightModeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _nightModeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        _nightModeRect.Visible = false; // Hidden by default
        
        var shader = GD.Load<Shader>("res://assets/shaders/flashlight.gdshader");
        var material = new ShaderMaterial();
        material.Shader = shader;
        _nightModeRect.Material = material;
        _nightModeLayer.AddChild(_nightModeRect);
        GD.Print("[VFX] Night Mode loaded!");
    }

    /// <summary>
    /// Updates the visibility of global effects based on user settings.
    /// </summary>
    public void UpdateEffectsVisibility()
    {
        bool enabled = Settings.GlobalEffectsEnabled;
        if (_vhsLayer != null) _vhsLayer.Visible = enabled;
        if (_nightModeLayer != null) _nightModeLayer.Visible = enabled;
    }

    /// <summary>
    /// Toggles night mode visual elements and updates background.
    /// </summary>
    public void UpdateNightMode()
    {
        if (_nightModeRect != null)
        {
            _nightModeRect.Visible = IsNightMode;
        }
        UpdateCursor();
        if (background != null)
        {
            background.UpdateAnimatedSprite(IsNightMode);
        }
    }

    /// <summary>
    /// Updates the custom mouse cursor texture based on current mode (normal/night).
    /// </summary>
    public void UpdateCursor()
    {
        Texture2D cursorTexture = IsNightMode ? CursorNight : CursorNormal;
        if (cursorTexture == null) return;

        Image img = cursorTexture.GetImage();
        img.Resize(img.GetWidth() * 3, img.GetHeight() * 3, Image.Interpolation.Nearest);
        ImageTexture scaledCursor = ImageTexture.CreateFromImage(img);
        // Hotspot at (10*3, 5*3) = (30, 15) because the visual tip is at (10, 5) in the original 32x32 sprite
        Input.SetCustomMouseCursor(scaledCursor, Input.CursorShape.Arrow, new Vector2(30, 15));
    }

    /// <summary>
    /// Triggers a brief screen glitch effect.
    /// </summary>
    public void TriggerGlitchEffect()
    {
        if (Glitch != null)
            Glitch.TriggerGlitch(0.5f, 0.15f);
    }

    /// <summary>
    /// Updates the screen desaturation level.
    /// </summary>
    /// <param name="value">The desaturation value (0.0 to 1.0).</param>
    public void UpdateDesaturation(float value)
    {
        if (Glitch != null)
            Glitch.SetDesaturation(value);
    }
}
