class_name EntitiesManager
extends GameManagerInterface

## Handles entity loading and door entry logic.

var entity_scenes: Array[PackedScene] = []


func load_entities() -> void:
	if game.door:
		game.door.entity_entered_door.connect(_on_entity_entered_door)
	_load_entity_scene("res://scenes/game/entity/entity.tscn")
	_load_entity_scene("res://scenes/game/entity/hiding_entity.tscn")
	_load_entity_scene("res://scenes/game/entity/multi_click_entity.tscn")
	_load_entity_scene("res://scenes/game/entity/teleport_entity.tscn")


func _load_entity_scene(path: String) -> void:
	entity_scenes.append(load(path))


func get_random_entity() -> PackedScene:
	if game.current_wave < 5:
		return entity_scenes[0]
	var p := game._rng.randi_range(0, 99)

	if p <= 75:
		return entity_scenes[0]
	if p <= 85:
		return entity_scenes[1]
	if p <= 95:
		return entity_scenes[2]
	return entity_scenes[3]


func _on_entity_entered_door(entity: Entity) -> void:
	if not game.is_spawning:
		return

	var type := entity.glass_type
	if type == "":
		return

	entity.set_entered_door()

	# Delay QueueFree to let it continue its path for a bit visually
	get_tree().create_timer(2.0).timeout.connect(
		func():
			if is_instance_valid(entity):
				entity.queue_free()
	)

	game.glass_passed += 1

	var is_correct := game.conditions_manager.try_enter_glass(type)
	GameEvents.glass_delivered.emit(type, is_correct)

	if is_correct:
		if game.conditions_manager.is_conditions_done():
			game.wave_manager.end_wave()
	else:
		game.hearts_manager.lose_heart()
