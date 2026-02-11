extends GeneralMenu

## Manages game settings like audio volume and visual effects.

signal close_requested

@onready var _master_slider: HSlider = %MasterSlider
@onready var _music_slider: HSlider = %MusicSlider
@onready var _sfx_slider: HSlider = %SfxSlider
@onready var _glitch_toggle: CheckBox = %GlitchToggle
@onready var _global_effects_toggle: CheckBox = %GlobalEffectsToggle
@onready var _back_button: Button = %BackButton

func _ready() -> void:
	super._ready()
	
	_master_slider.value = SettingsManager.master_volume
	_music_slider.value = SettingsManager.music_volume
	_sfx_slider.value = SettingsManager.sfx_volume
	_glitch_toggle.button_pressed = SettingsManager.glitch_enabled
	_global_effects_toggle.button_pressed = SettingsManager.global_effects_enabled

	_master_slider.value_changed.connect(_on_master_slider_value_changed)
	_music_slider.value_changed.connect(_on_music_slider_value_changed)
	_sfx_slider.value_changed.connect(_on_sfx_slider_value_changed)
	_glitch_toggle.toggled.connect(_on_glitch_toggled)
	_global_effects_toggle.toggled.connect(_on_global_effects_toggled)
	_back_button.pressed.connect(_on_back_pressed)

## Updates the master volume setting.
func _on_master_slider_value_changed(value: float) -> void:
	SettingsManager.master_volume = value

## Updates the music volume setting.
func _on_music_slider_value_changed(value: float) -> void:
	SettingsManager.music_volume = value

## Updates the SFX volume setting.
func _on_sfx_slider_value_changed(value: float) -> void:
	SettingsManager.sfx_volume = value

## Toggles the glitch effect setting.
func _on_glitch_toggled(toggled_on: bool) -> void:
	SettingsManager.glitch_enabled = toggled_on

## Toggles all global visual effects settings.
func _on_global_effects_toggled(toggled_on: bool) -> void:
	SettingsManager.global_effects_enabled = toggled_on

## Saves settings and signals for the menu to close.
func _on_back_pressed() -> void:
	SettingsManager.save_settings()
	close_requested.emit()
