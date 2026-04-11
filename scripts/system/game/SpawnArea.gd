extends Area2D

## Manages the spawning of entities within a defined area.

@export var area: CollisionShape2D

var _active_entities: Array[Entity] = []


## Generates a random position within the defined rectangular spawn area.
func get_random_position() -> Vector2:
	if area == null or not area.shape is RectangleShape2D:
		return Vector2.ZERO

	var rect_shape = area.shape as RectangleShape2D
	var size = rect_shape.size
	var x = randf_range(-size.x / 2.0, size.x / 2.0)
	var y = randf_range(-size.y / 2.0, size.y / 2.0)

	return area.position + Vector2(x, y)


## Checks if a position is valid (not too close to other active entities).
func is_position_valid(
	pos: Vector2, min_distance_squared: float, ignore_entity: Entity = null
) -> bool:
	for entity in _active_entities:
		if not is_instance_valid(entity) or entity == ignore_entity:
			continue
		if pos.distance_squared_to(entity.position) < min_distance_squared:
			return false
	return true


## Returns a valid random position within the spawn area, considering other active entities.
func get_valid_random_position(
	ignore_entity: Entity = null, min_dist: float = 250.0, max_attempts: int = 50
) -> Vector2:
	var pos = Vector2.ZERO
	for i in range(max_attempts):
		pos = get_random_position()
		var threshold = min_dist * (1.0 - float(i) / max_attempts)
		if is_position_valid(pos, threshold * threshold, ignore_entity):
			break
	return pos


## Instantiates and spawns a random entity within the area, applying current wave modifiers.
func spawn_entity(game: GameManager) -> void:
	var random_scene = game.entities_manager.get_random_entity()
	var entity = random_scene.instantiate() as Entity

	# Calculate speed first
	entity.set_speed(entity.get_speed() + 15 * exp(0.1 * game.current_wave))

	# Find a valid random position
	entity.position = get_valid_random_position()
	add_child(entity)

	var glass_type = game.glass_manager.random_glass_type()
	entity.update_glass_type(glass_type, game.glass_manager.get_glass_sprite(glass_type))

	entity.tree_exited.connect(func(): _on_entity_tree_exited(entity))
	_active_entities.append(entity)


## Callback for when an entity is removed from the scene tree, cleaning up the active entities list.
func _on_entity_tree_exited(entity: Entity) -> void:
	if _active_entities.has(entity):
		_active_entities.erase(entity)


## Triggers the disappearance and removal of all currently active entities.
func kill_every_entities() -> void:
	var entities_snapshot = _active_entities.duplicate()

	for entity in entities_snapshot:
		if is_instance_valid(entity) and entity.is_alive:
			if not entity.heading_to_door:
				continue
			entity.disappear()

	_active_entities.clear()
