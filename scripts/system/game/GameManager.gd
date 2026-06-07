class_name GameManager
extends Node2D

## Central manager for the game loop, linking all systems and state.

# --- EXPORTS ---
@export_group("Difficulty settings")
@export var delay_curve: Curve
@export var glass_count_curve: Curve
@export var entity_speed_curve: Curve
@export var waves_to_add_a_glass_type: float

@export_group("References")
@export var door: Door
@export var main_menu: MainMenu
@export var trash: Node2D
@export var glitch_effect: GlitchEffect
@export var settings_button: Button
@export var pause_menu_scene: PackedScene
@export var spawn_area: SpawnArea
@export var unboarding: Sprite2D
@export var board: Board
@export var heart_container: Node2D
@export var heart_scene: PackedScene
@export var background: AnimatedBackground
@export var black_canvas: CanvasModulate
@export var debug_panel: DebugPanel

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
var glass_manager: GlassManager
var entities_manager: EntitiesManager
var wave_manager: WaveManager
var conditions_manager: ConditionsManager
var hearts_manager: HeartsManager
var vfx_manager: VFXManager
var discord_rpc_manager: DiscordRPCManager

var _rng: RandomNumberGenerator = RandomNumberGenerator.new()

# --- INITIALIZATION ---


func _ready() -> void:
	_rng.randomize()
	print("[GAME] GameManager _ready")
	print("[GAME] unboarding: ", unboarding)
	_setup_managers()

	y_sort_enabled = true
	_screen_fade_in()

	main_menu.play_button_pressed.connect(_on_play_button_pressed)
	trash.add_to_group(&"Trash")
	trash.z_index = 100

	vfx_manager.update_effects_visibility()

	if glitch_effect:
		glitch_effect.set_desaturation(0.0)

	settings_button.pressed.connect(_on_settings_button_pressed)
	settings_button.disabled = true

	MusicManager.play_menu_music()
	hearts_manager.start_heart_animation_loop()


func _setup_managers() -> void:
	glass_manager = GlassManager.new()
	entities_manager = EntitiesManager.new()
	wave_manager = WaveManager.new()
	conditions_manager = ConditionsManager.new()
	hearts_manager = HeartsManager.new()
	vfx_manager = VFXManager.new()
	discord_rpc_manager = DiscordRPCManager.new()

	var managers: Array[GameManagerInterface] = [
		glass_manager,
		entities_manager,
		wave_manager,
		conditions_manager,
		hearts_manager,
		vfx_manager,
		discord_rpc_manager
	]

	for mgr in managers:
		mgr.name = mgr.get_script().get_path().get_file().get_basename()
		add_child(mgr)
		mgr.init(self)

	conditions_manager.load_conditions_manager()
	vfx_manager.load_vfx()
	entities_manager.load_entities()
	debug_panel.init(self)


func _screen_fade_in() -> void:
	var fade_layer := CanvasLayer.new()
	fade_layer.layer = 128
	var fade_rect := ColorRect.new()
	fade_rect.color = Color.BLACK
	fade_rect.modulate.a = 1.0
	fade_rect.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	fade_layer.add_child(fade_rect)
	add_child(fade_layer)

	var fade_tween := create_tween()
	fade_tween.tween_property(fade_rect, "modulate:a", 0.0, 1.5)
	fade_tween.finished.connect(func(): fade_layer.queue_free())


# --- CALLBACKS ---


func _on_play_button_pressed() -> void:
	main_menu.close()
	settings_button.disabled = false
	await main_menu.tree_exited
	wave_manager.start_game()


func _on_settings_button_pressed() -> void:
	if settings_button.disabled:
		return
	var pause_menu: PauseMenu = pause_menu_scene.instantiate()
	add_child(pause_menu)
	get_tree().paused = true
	MusicManager.set_pause_effect(true)
	discord_rpc_manager.set_paused(true)


func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed(&"interact"):
		if unboarding != null and unboarding.visible and unboarding.modulate.a > 0.0:
			if not unboarding.get_meta("is_fading_out", false):
				unboarding.set_meta("is_fading_out", true)
				hide_unboarding()
				get_viewport().set_input_as_handled()
			return
		_handle_entity_click(event.position)


func _handle_entity_click(_mouse_position: Vector2) -> void:
	var space_state := get_world_2d().direct_space_state
	var query := PhysicsPointQueryParameters2D.new()
	query.position = get_global_mouse_position()
	query.collide_with_areas = true
	query.collide_with_bodies = true

	var results := space_state.intersect_point(query)

	var top_entity: Entity = null
	var max_z: int = -2147483648

	for result in results:
		var collider: Object = result.collider
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


func show_unboarding() -> void:
	if unboarding == null:
		return
	unboarding.set_meta("is_fading_out", false)
	unboarding.modulate.a = 0.0
	unboarding.visible = true
	var tween := create_tween()
	tween.tween_property(unboarding, "modulate:a", 1.0, 0.4).set_trans(Tween.TRANS_SINE).set_ease(
		Tween.EASE_OUT
	)


func hide_unboarding() -> void:
	if unboarding == null:
		return
	var tween := create_tween()
	tween.tween_property(unboarding, "modulate:a", 0.0, 0.3).set_trans(Tween.TRANS_SINE).set_ease(
		Tween.EASE_IN
	)
	tween.finished.connect(
		func():
			unboarding.visible = false
			wave_manager.unboarding_closed = true
	)
