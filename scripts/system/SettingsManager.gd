extends Node

## Manages application settings, persistence, and global state (like volume and effects).

signal settings_changed

var master_volume: float = 1.0:
	set(value):
		master_volume = value
		_apply_master_volume()
	get:
		return master_volume

var sfx_volume: float = 1.0:
	set(value):
		sfx_volume = value
	get:
		return sfx_volume

var glitch_enabled: bool = true:
	set(value):
		glitch_enabled = value
		settings_changed.emit()
	get:
		return glitch_enabled

var global_effects_enabled: bool = true:
	set(value):
		global_effects_enabled = value
		settings_changed.emit()
	get:
		return global_effects_enabled

var has_seen_onboarding: bool = false:
	set(value):
		has_seen_onboarding = value
		save_settings()
	get:
		return has_seen_onboarding

func _ready() -> void:
	load_settings()
	_apply_master_volume()

## Applies the current master volume setting to the AudioServer.
func _apply_master_volume() -> void:
	var master_bus_index = AudioServer.get_bus_index("Master")
	AudioServer.set_bus_volume_db(master_bus_index, linear_to_db(master_volume))

## Persists current settings to a configuration file.
func save_settings() -> void:
	var config = ConfigFile.new()
	config.set_value("audio", "master_volume", master_volume)
	config.set_value("audio", "sfx_volume", sfx_volume)
	config.set_value("effects", "glitch_enabled", glitch_enabled)
	config.set_value("effects", "global_effects_enabled", global_effects_enabled)
	config.set_value("gameplay", "has_seen_onboarding", has_seen_onboarding)
	config.save("user://settings.cfg")

## Loads settings from the configuration file if it exists.
func load_settings() -> void:
	var config = ConfigFile.new()
	var err = config.load("user://settings.cfg")
	if err == OK:
		master_volume = config.get_value("audio", "master_volume", 1.0)
		sfx_volume = config.get_value("audio", "sfx_volume", 1.0)
		glitch_enabled = config.get_value("effects", "glitch_enabled", true)
		global_effects_enabled = config.get_value("effects", "global_effects_enabled", true)
		has_seen_onboarding = config.get_value("gameplay", "has_seen_onboarding", false)
