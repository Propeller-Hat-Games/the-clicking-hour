using Godot;
using System;

/// <summary>
/// Manages game settings like audio volume and visual effects.
/// </summary>
public partial class OptionsMenu : GeneralMenu
{
    private HSlider _masterSlider;
    private HSlider _sfxSlider;
    private CheckBox _glitchToggle;
    private CheckBox _globalEffectsToggle;
    private Button _backButton;

    [Signal]
    public delegate void CloseRequestedEventHandler();

    /// <summary>
    /// Initializes the options menu UI with current settings values.
    /// </summary>
    public override void _Ready()
    {
        base._Ready();
        _masterSlider = GetNode<HSlider>("%MasterSlider");
        _sfxSlider = GetNode<HSlider>("%SfxSlider");
        _glitchToggle = GetNode<CheckBox>("%GlitchToggle");
        _globalEffectsToggle = GetNode<CheckBox>("%GlobalEffectsToggle");
        _backButton = GetNode<Button>("%BackButton");

        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        
        _masterSlider.Value = settings.MasterVolume;
        _sfxSlider.Value = settings.SfxVolume;
        _glitchToggle.ButtonPressed = settings.GlitchEnabled;
        _globalEffectsToggle.ButtonPressed = settings.GlobalEffectsEnabled;

        _masterSlider.ValueChanged += OnMasterSliderValueChanged;
        _sfxSlider.ValueChanged += OnSfxSliderValueChanged;
        _glitchToggle.Toggled += OnGlitchToggled;
        _globalEffectsToggle.Toggled += OnGlobalEffectsToggled;
        _backButton.Pressed += OnBackPressed;
    }

    /// <summary>
    /// Updates the master volume setting.
    /// </summary>
    /// <param name="value">New linear volume value.</param>
    private void OnMasterSliderValueChanged(double value)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.MasterVolume = (float)value;
    }

    /// <summary>
    /// Updates the SFX volume setting.
    /// </summary>
    /// <param name="value">New linear volume value.</param>
    private void OnSfxSliderValueChanged(double value)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.SfxVolume = (float)value;
    }

    /// <summary>
    /// Toggles the glitch effect setting.
    /// </summary>
    /// <param name="toggledOn">True if enabled.</param>
    private void OnGlitchToggled(bool toggledOn)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.GlitchEnabled = toggledOn;
    }

    /// <summary>
    /// Toggles all global visual effects settings.
    /// </summary>
    /// <param name="toggledOn">True if enabled.</param>
    private void OnGlobalEffectsToggled(bool toggledOn)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.GlobalEffectsEnabled = toggledOn;
    }

    /// <summary>
    /// Saves settings and signals for the menu to close.
    /// </summary>
    private void OnBackPressed()
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.SaveSettings();
        EmitSignal(SignalName.CloseRequested);
    }
}
