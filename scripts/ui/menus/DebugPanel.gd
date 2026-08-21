class_name DebugPanel
extends PanelContainer

var game: GameManager
var change_wave: int = 0


func init(p_game: GameManager) -> void:
	game = p_game
	_setup_spawn_options()


func _ready() -> void:
	if not OS.is_debug_build():
		hide()
		set_process(false)
		return

	$Content.visible = false
	reset_size()


func _process(_delta: float) -> void:
	if not game:
		return

	$Content/Body/Informations/CurrentWave.text = "Current wave : %d" % game.current_wave
	$Content/Body/Informations/NightMode.text = (
		"Night Mode : %s" % ("ON" if game.is_night_mode else "OFF")
	)
	$Content/Body/Informations/LivesGroup/Value.text = str(game.hearts)
	$Content/Body/Informations/Kills.text = "Kills : %d" % game.entities_killed
	$Content/Body/Informations/Passed.text = "Passed : %d" % game.glass_passed


func _on_open_button_pressed() -> void:
	if game:
		change_wave = game.current_wave
		_update_wave_input()

	$Content/Body/Informations/UnboadringGroup/CheckButton.button_pressed = (not (
		SettingsManager.has_seen_onboarding
	))
	_setup_spawn_options()
	$Content.visible = true
	$OpenButton.visible = false
	reset_size()


func _on_close_button_pressed() -> void:
	$Content.visible = false
	$OpenButton.visible = true
	reset_size()


func _update_wave_input() -> void:
	if change_wave < 0:
		change_wave = 0
	$Content/Body/Manage/WaveGroup/Wave.text = str(change_wave)


func _on_wave_less_pressed() -> void:
	change_wave -= 1
	_update_wave_input()


func _on_wave_more_pressed() -> void:
	change_wave += 1
	_update_wave_input()


func _on_wave_text_changed(new_text: String) -> void:
	change_wave = new_text.to_int()
	_update_wave_input()


func _on_reset_button_pressed() -> void:
	if not game:
		return

	game.is_spawning = false
	game.spawn_area.kill_every_entities()

	game.wave_manager.start_wave(game.current_wave, game.is_night_mode)


func _on_update_button_pressed() -> void:
	if not game:
		return

	game.is_spawning = false
	game.spawn_area.kill_every_entities()

	var night_mode: bool = $Content/Body/Manage/NightModeGroup/CheckButton.button_pressed
	game.wave_manager.start_wave(change_wave, night_mode)


func _on_lives_less_pressed() -> void:
	game.hearts -= 1
	game.hearts_manager.update_hearts()


func _on_lives_more_pressed() -> void:
	game.hearts += 1
	game.hearts_manager.update_hearts()


func update_has_seen_unboarding() -> void:
	$Content/Body/Informations/UnboadringGroup/CheckButton.button_pressed = (not (
		SettingsManager.has_seen_onboarding
	))


func _on_unboarding_button_toggled(toggled_on: bool) -> void:
	SettingsManager.has_seen_onboarding = not toggled_on


func _setup_spawn_options() -> void:
	var spawn_button: Button = $Content/Body/Spawn/Button
	var entity_input: OptionButton = $Content/Body/Spawn/Entity/Input
	var glass_input: OptionButton = $Content/Body/Spawn/Glass/Input

	if not game:
		spawn_button.disabled = true
		return

	if (
		entity_input.item_count <= 1
		and game.entities_manager
		and not game.entities_manager.entity_scenes.is_empty()
	):
		entity_input.clear()
		entity_input.add_item("Random")
		entity_input.set_item_metadata(0, null)
		for scene in game.entities_manager.entity_scenes:
			var entity_name := _get_entity_name(scene)
			var icon := _get_entity_icon(scene)
			var idx := entity_input.item_count
			if icon != null:
				entity_input.add_icon_item(icon, entity_name)
			else:
				entity_input.add_item(entity_name)
			entity_input.set_item_metadata(idx, scene)

	if (
		glass_input.item_count <= 1
		and game.glass_manager
		and not game.glass_manager.every_sprites.is_empty()
	):
		glass_input.clear()
		glass_input.add_item("Random")
		glass_input.set_item_metadata(0, "")
		var glass_keys: Array[String] = []
		glass_keys.assign(game.glass_manager.every_sprites.keys())
		glass_keys.sort()
		for key in glass_keys:
			var idx := glass_input.item_count
			var glass_sprite := game.glass_manager.get_glass_sprite(key)
			var icon: Texture2D = glass_sprite.texture if glass_sprite else null
			if icon != null:
				glass_input.add_icon_item(icon, key.capitalize())
			else:
				glass_input.add_item(key.capitalize())
			glass_input.set_item_metadata(idx, key)

	spawn_button.disabled = (
		game.spawn_area == null
		or game.entities_manager == null
		or game.glass_manager == null
		or entity_input.item_count == 0
		or glass_input.item_count == 0
	)


func _get_entity_name(scene: PackedScene) -> String:
	var base_name := scene.resource_path.get_file().get_basename()
	match base_name:
		"entity":
			return "Simple"
		"hiding_entity":
			return "Hiding"
		"multi_click_entity":
			return "MultiClick"
		"teleport_entity":
			return "Teleport"
		_:
			return base_name.capitalize()


func _get_entity_icon(scene: PackedScene) -> Texture2D:
	var base_name := scene.resource_path.get_file().get_basename()
	var anim_name: StringName
	match base_name:
		"entity":
			anim_name = &"normal_walk"
		"hiding_entity":
			anim_name = &"dig_walk"
		"multi_click_entity":
			anim_name = &"multiclick_walk"
		"teleport_entity":
			anim_name = &"tp_walk"
		_:
			anim_name = &"normal_walk"

	var temp: Node = scene.instantiate()
	var icon: Texture2D = null
	var sprite: AnimatedSprite2D = temp.get_node_or_null("Sprite2D")
	if sprite and sprite.sprite_frames and sprite.sprite_frames.has_animation(anim_name):
		icon = sprite.sprite_frames.get_frame_texture(anim_name, 0)
	temp.free()
	return icon


func _on_spawn_button_pressed() -> void:
	if not game or not game.spawn_area or not game.entities_manager or not game.glass_manager:
		return

	var entity_input: OptionButton = $Content/Body/Spawn/Entity/Input
	var glass_input: OptionButton = $Content/Body/Spawn/Glass/Input

	if entity_input.item_count == 0 or glass_input.item_count == 0:
		return

	var selected_entity_idx := entity_input.selected
	var entity_scene: PackedScene = null
	if selected_entity_idx >= 0:
		entity_scene = entity_input.get_item_metadata(selected_entity_idx)

	var selected_glass_idx := glass_input.selected
	var glass_type: String = ""
	if selected_glass_idx >= 0:
		glass_type = glass_input.get_item_metadata(selected_glass_idx)

	game.spawn_area.spawn_entity(game, entity_scene, glass_type)
