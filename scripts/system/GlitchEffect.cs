using Godot;
using System;

public partial class GlitchEffect : CanvasLayer
{
    private ColorRect colorRect;
    private ShaderMaterial material;
    private Tween tween;

    public override void _Ready()
    {
        colorRect = GetNode<ColorRect>("ColorRect");
        material = (ShaderMaterial)colorRect.Material;
        // Start with no effect
        material.SetShaderParameter("strength", 0.0f);
        material.SetShaderParameter("desaturation", 0.0f);
    }

    public void SetDesaturation(float value)
    {
        if (material != null)
        {
            material.SetShaderParameter("desaturation", Mathf.Clamp(value, 0.0f, 1.0f));
        }
    }

    public void TriggerGlitch(float duration = 0.5f, float intensity = 0.05f)
    {
        GD.Print($"TriggerGlitch called: duration={duration}, intensity={intensity}");
        if (tween != null && tween.IsRunning())
        {
            tween.Kill();
        }

        tween = CreateTween();
        
        // Spike up
        tween.TweenMethod(Callable.From<float>(val => {
            material.SetShaderParameter("strength", val);
            // GD.Print($"Glitch strength: {val}");
        }), 0.0f, intensity, duration * 0.2f);
        // Fade out
        tween.TweenMethod(Callable.From<float>(val => {
            material.SetShaderParameter("strength", val);
        }), intensity, 0.0f, duration * 0.8f);
        
        tween.Finished += () => GD.Print("Glitch tween finished");
    }
}
