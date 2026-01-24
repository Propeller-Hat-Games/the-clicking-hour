using Godot;
using System;

public partial class OptionsMenu : Control
{
    private HSlider _masterSlider;
    private HSlider _sfxSlider;
    private CheckBox _glitchToggle;
    private CheckBox _globalEffectsToggle;
    private Button _backButton;

    [Signal]
    public delegate void CloseRequestedEventHandler();

    public override void _Ready()
    {
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

    private void OnMasterSliderValueChanged(double value)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.MasterVolume = (float)value;
    }

    private void OnSfxSliderValueChanged(double value)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.SfxVolume = (float)value;
    }

    private void OnGlitchToggled(bool toggledOn)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.GlitchEnabled = toggledOn;
    }

    private void OnGlobalEffectsToggled(bool toggledOn)
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.GlobalEffectsEnabled = toggledOn;
    }

    private void OnBackPressed()
    {
        var settings = GetNode<SettingsManager>("/root/SettingsManager");
        settings.SaveSettings();
        EmitSignal(SignalName.CloseRequested);
    }
}
