extends Control

## Displays the splash screen with a logo and transition to the main menu.

@export var fade_duration: float = 1.5
@export var wait_duration: float = 1.0
@export var main_menu_scene_path: String = "res://scenes/game_manager.tscn"

var _sprite: AnimatedSprite2D
var _audio: AudioStreamPlayer
var _fade_color_rect: ColorRect
var _is_skipping: bool = false

func _ready() -> void:
	_sprite = get_node("AnimatedSprite2D")
	_audio = get_node("AudioStreamPlayer")
	
	# Add a dedicated fade overlay
	_fade_color_rect = ColorRect.new()
	_fade_color_rect.color = Color.BLACK
	_fade_color_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	_fade_color_rect.modulate.a = 0.0
	_fade_color_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(_fade_color_rect)

	_sprite.position = get_viewport_rect().size / 2.0
	_sprite.modulate.a = 0.0
	
	_sprite.play()
	_audio.play()
	
	await _fade_sprite(0.0, 1.0)
	if not is_inside_tree() or _is_skipping: return
	
	await get_tree().create_timer(wait_duration).timeout
	if not is_inside_tree() or _is_skipping: return
	
	_transition_to_menu()

func _input(event: InputEvent) -> void:
	if _is_skipping:
		return
		
	if (event is InputEventMouseButton and event.pressed) or (event is InputEventKey and event.pressed):
		_skip()

func _skip() -> void:
	_is_skipping = true
	_transition_to_menu(fade_duration / 3.0) # Faster fade when skipping

func _transition_to_menu(custom_fade_duration: float = fade_duration) -> void:
	# Fade out everything to black
	var fade_out_tween = create_tween().set_parallel()
	fade_out_tween.tween_property(_sprite, "modulate:a", 0.0, custom_fade_duration)
	fade_out_tween.tween_property(_audio, "volume_db", -80.0, custom_fade_duration)
	fade_out_tween.tween_property(_fade_color_rect, "modulate:a", 1.0, custom_fade_duration)
	
	await fade_out_tween.finished
	if not is_inside_tree(): return
	
	# Wait one extra frame to be sure black is rendered
	await get_tree().process_frame
	if not is_inside_tree(): return
	
	get_tree().change_scene_to_file(main_menu_scene_path)

func _fade_sprite(from: float, to: float) -> void:
	var tween = create_tween()
	tween.tween_property(_sprite, "modulate:a", to, fade_duration).from(from)
	
	await tween.finished
