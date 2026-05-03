class_name GameManager
extends Node2D

## Central manager for the game loop, linking all systems and state.

# --- EXPORTS ---
@export var delay_curve: Curve

@export_group("References")
@export var door: Node2D
@export var main_menu: Control
@export var trash: Node2D
@export var glitch_effect: Node  # GlitchEffect script
@export var settings_button: Button
@export var pause_menu_scene: PackedScene
@export var spawn_area: Node  # SpawnArea script
@export var unboarding: Sprite2D
@export var board: Node  # Board script
@export var heart_container: Node2D
@export var heart_scene: PackedScene
@export var background: Node  # AnimatedBackground script
@export var black_canvas: CanvasModulate

@export_group("Assets")
@export var cursor_normal: Texture2D
@export var cursor_night: Texture2D
@export var cursor_light: PointLight2D

# --- STATE ---
var current_wave: int = 0
var is_night_mode: bool = false
var entities_killed: int = 0
var glass_passed: int = 0
var hearts: int = 3
var is_spawning: bool = false

# --- MANAGERS ---
var glass_manager: Node
var entities_manager: Node
var wave_manager: Node
var conditions_manager: Node
var hearts_manager: Node
var vfx_manager: Node
var discord_rpc_manager: Node

var _rng: RandomNumberGenerator = RandomNumberGenerator.new()

# --- INITIALIZATION ---


func _ready() -> void:
	print("[GAME] GameManager _ready")
	print("[GAME] unboarding: ", unboarding)
	_setup_managers()

	y_sort_enabled = true
	_screen_fade_in()

	main_menu.play_button_pressed.connect(_on_play_button_pressed)
	trash.add_to_group("Trash")
	trash.z_index = 100

	SettingsManager.settings_changed.connect(vfx_manager.update_effects_visibility)
	vfx_manager.update_effects_visibility()

	if glitch_effect:
		glitch_effect.set_desaturation(0.0)

	settings_button.pressed.connect(_on_settings_button_pressed)
	settings_button.disabled = true

	MusicManager.play_menu_music()
	hearts_manager.start_heart_animation_loop()


func _setup_managers() -> void:
	glass_manager = _add_manager(preload("res://scripts/system/game/GlassManager.gd"))
	entities_manager = _add_manager(preload("res://scripts/system/game/EntitiesManager.gd"))
	wave_manager = _add_manager(preload("res://scripts/system/game/WaveManager.gd"))
	conditions_manager = _add_manager(preload("res://scripts/system/game/ConditionsManager.gd"))
	hearts_manager = _add_manager(preload("res://scripts/system/game/HeartsManager.gd"))
	vfx_manager = _add_manager(preload("res://scripts/system/game/VFXManager.gd"))
	discord_rpc_manager = _add_manager(preload("res://scripts/system/game/DiscordRPCManager.gd"))

	conditions_manager.load_conditions_manager()
	vfx_manager.load_vfx()
	entities_manager.load_entities()


func _add_manager(script: GDScript) -> Node:
	var mgr = Node.new()
	mgr.set_script(script)
	mgr.name = script.get_path().get_file().get_basename()
	add_child(mgr)
	mgr.init(self)
	return mgr


func _screen_fade_in() -> void:
	var fade_layer = CanvasLayer.new()
	fade_layer.layer = 128
	var fade_rect = ColorRect.new()
	fade_rect.color = Color.BLACK
	fade_rect.modulate.a = 1.0
	fade_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	fade_layer.add_child(fade_rect)
	add_child(fade_layer)

	var fade_tween = create_tween()
	fade_tween.tween_property(fade_rect, "modulate:a", 0.0, 1.5)
	fade_tween.finished.connect(func(): fade_layer.queue_free())


# --- CALLBACKS ---


func _on_play_button_pressed() -> void:
	main_menu.close()
	settings_button.disabled = false
	wave_manager.start_game()


func _on_settings_button_pressed() -> void:
	if settings_button.disabled:
		return
	var pause_menu = pause_menu_scene.instantiate()
	add_child(pause_menu)
	get_tree().paused = true
	MusicManager.set_pause_effect(true)
	discord_rpc_manager.set_paused(true)


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		if unboarding != null and unboarding.visible:
			unboarding.visible = false
			wave_manager.unboarding_closed = true
			get_viewport().set_input_as_handled()
			return
		_handle_entity_click(event.position)


func _handle_entity_click(_mouse_position: Vector2) -> void:
	var space_state = get_world_2d().direct_space_state
	var query = PhysicsPointQueryParameters2D.new()
	query.position = get_global_mouse_position()
	query.collide_with_areas = true
	query.collide_with_bodies = true

	var results = space_state.intersect_point(query)

	var top_entity: Entity = null
	var max_z: int = -2147483648

	for result in results:
		var collider = result.collider
		var entity: Entity = null

		if collider is Entity:
			entity = collider
		elif collider.get_parent() is Entity:
			entity = collider.get_parent()

		if entity != null and entity.is_alive:
			if entity.z_index > max_z:
				max_z = entity.z_index
				top_entity = entity
			elif entity.z_index == max_z:
				if top_entity != null and entity.global_position.y > top_entity.global_position.y:
					top_entity = entity
				elif top_entity == null:
					top_entity = entity

	if top_entity != null:
		top_entity.try_click(self)
		get_viewport().set_input_as_handled()
