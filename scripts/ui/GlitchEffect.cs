using Godot;
using System;

/// <summary>
/// Controls the screen glitch shader effect.
/// </summary>
public partial class GlitchEffect : CanvasLayer
{
	private ColorRect _colorRect;
	private ShaderMaterial _material;
	private Tween _tween;

	/// <summary>
	/// Initializes the glitch effect, caching the material and resetting parameters.
	/// </summary>
	public override void _Ready()
	{
		_colorRect = GetNode<ColorRect>("ColorRect");
		_material = (ShaderMaterial)_colorRect.Material;
		// Start with no effect
		_material.SetShaderParameter("strength", 0.0f);
		_material.SetShaderParameter("aberration", 0.0f);
		_material.SetShaderParameter("desaturation", 0.0f);
	}
	/// <summary>
	/// Sets the desaturation level of the screen.
	/// </summary>
	public void SetDesaturation(float value)
	{
		if (_material != null)
		{
			_material.SetShaderParameter("desaturation", Mathf.Clamp(value, 0.0f, 1.0f));
		}
	}

	/// <summary>
	/// Triggers a glitch spike effect.
	/// </summary>
	public void TriggerGlitch(float duration = 0.5f, float intensity = 0.05f)
	{
		var settings = GetNode<SettingsManager>("/root/SettingsManager");
		if (settings != null && !settings.GlitchEnabled) return;

		if (_tween != null && _tween.IsRunning())
		{
			_tween.Kill();
		}

		_tween = CreateTween();
		
		// Spike up
		_tween.Parallel().TweenMethod(Callable.From<float>(val => {
			_material.SetShaderParameter("strength", val);
		}), 0.0f, intensity, duration * 0.2f);
		
		_tween.Parallel().TweenMethod(Callable.From<float>(val => {
			_material.SetShaderParameter("aberration", val * 0.1f);
		}), 0.0f, intensity, duration * 0.2f);

		// Fade out
		_tween.TweenMethod(Callable.From<float>(val => {
			_material.SetShaderParameter("strength", val);
		}), intensity, 0.0f, duration * 0.8f);
		
		_tween.Parallel().TweenMethod(Callable.From<float>(val => {
			_material.SetShaderParameter("aberration", val * 0.1f);
		}), intensity, 0.0f, duration * 0.8f);
	}

	/// <summary>
	/// Triggers a chromatic aberration effect.
	/// </summary>
	public void TriggerChromaticAberration(float duration = 0.5f, float intensity = 0.005f)
	{
		var settings = GetNode<SettingsManager>("/root/SettingsManager");
		if (settings != null && !settings.GlitchEnabled) return;

		if (_tween != null && _tween.IsRunning())
		{
			_tween.Kill();
		}

		_tween = CreateTween();
		
		_tween.TweenMethod(Callable.From<float>(val => {
			_material.SetShaderParameter("aberration", val);
		}), 0.0f, intensity, duration * 0.2f);
		
		_tween.TweenMethod(Callable.From<float>(val => {
			_material.SetShaderParameter("aberration", val);
		}), intensity, 0.0f, duration * 0.8f);
	}
}
