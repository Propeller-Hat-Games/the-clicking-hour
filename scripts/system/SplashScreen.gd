class_name SplashScreen
extends Control

## Displays the animated splash screen sequence and transitions to the main menu.

@export_group("Timing")
@export var fade_duration: float = 1.0
@export var wait_duration: float = 14.5
@export var skip_fade_duration: float = 0.5

@export_group("Navigation")
@export var allow_skip: bool = true
@export var main_menu_scene_path: String = "res://scenes/game/game_manager.tscn"

@export_group("Audio")
@export var fade_audio_out: bool = true
@export var audio_fade_target_db: float = -80.0

var _fade_color_rect: ColorRect
var _is_skipping: bool = false
var _audio_unlocked: bool = false
var _splash_tween: Tween = null
var _transition_tween: Tween = null

@onready var _sprite: AnimatedSprite2D = $AnimatedSprite2D
@onready var _audio: AudioStreamPlayer = $AudioStreamPlayer


func _ready() -> void:
	get_viewport().use_hdr_2d = false

	# Dedicated fade overlay
	_fade_color_rect = ColorRect.new()
	_fade_color_rect.color = Color.BLACK
	_fade_color_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_fade_color_rect.modulate.a = 0.0
	_fade_color_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_fade_color_rect)

	_sprite.position = get_viewport_rect().size / 2.0
	_sprite.modulate.a = 0.0
	_sprite.play(&"default")

	if OS.get_name() != "Web":
		_try_unlock_audio()

	_splash_tween = create_tween()
	_splash_tween.tween_property(_sprite, "modulate:a", 1.0, fade_duration).from(0.0)
	_splash_tween.tween_interval(wait_duration)
	_splash_tween.tween_callback(_transition_to_menu.bind(fade_duration))


func _input(event: InputEvent) -> void:
	if _is_skipping:
		return

	if not _audio_unlocked:
		_try_unlock_audio()

	if allow_skip and (event.is_action_pressed(&"interact") or event.is_pressed()):
		_skip()


func _try_unlock_audio() -> void:
	if _audio_unlocked or _audio == null:
		return
	_audio.play()
	_audio_unlocked = true


func _skip() -> void:
	if _splash_tween != null and _splash_tween.is_valid():
		_splash_tween.kill()
	_is_skipping = true
	_transition_to_menu(skip_fade_duration)


func _transition_to_menu(duration: float) -> void:
	if _transition_tween != null and _transition_tween.is_valid():
		_transition_tween.kill()

	_transition_tween = create_tween().set_parallel()
	_transition_tween.tween_property(_sprite, "modulate:a", 0.0, duration)
	if fade_audio_out and _audio != null:
		_transition_tween.tween_property(_audio, "volume_db", audio_fade_target_db, duration)
	_transition_tween.tween_property(_fade_color_rect, "modulate:a", 1.0, duration)

	_transition_tween.chain().tween_await(get_tree().process_frame)
	_transition_tween.tween_callback(
		func():
			get_viewport().use_hdr_2d = ProjectSettings.get_setting_with_override(
				"rendering/viewport/hdr_2d"
			)
			get_tree().change_scene_to_file(main_menu_scene_path)
	)
