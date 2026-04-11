class_name Entity
extends CharacterBody2D

## Base class for all interactive entities in the game.
## Handles movement, animation, and life cycle.

enum EntityState {
	WALKING,
	HIDING,
	STUNNED,
}

@export var walk_speed: float = 10.0
@export var spawn_delay: float = 1.0
@export var glass: Sprite2D
@export var sprite: AnimatedSprite2D
@export var click_area: Area2D

var glass_type: String = ""
var current_state: EntityState = EntityState.WALKING
var walk_direction: Vector2 = Vector2.RIGHT
var hearts: int = 1
var is_alive: bool:
	get:
		return hearts > 0
var is_disappearing: bool = false
var anim_prefix: String = "normal"
var heading_to_door: bool = true

var _spawn_timer: float = 0.0
var _glass_initial_pos: Vector2
var _door: Node2D
var _trash: Node2D
var _target_y_offset: float


func _ready() -> void:
	_door = get_tree().get_first_node_in_group("Door") as Node2D
	_target_y_offset = randf_range(-20.0, 20.0)
	motion_mode = CharacterBody2D.MOTION_MODE_FLOATING

	# Cache Trash node efficiently
	_trash = get_tree().get_first_node_in_group("Trash") as Node2D
	if _trash == null:
		_trash = get_tree().root.find_child("Trash", true, false) as Node2D

	initialize_entity()
	_update_animation()

	if glass != null:
		_glass_initial_pos = glass.position
		_animate_glass_spawn()


## Initializes specific entity properties. Must be implemented by subclasses.
func initialize_entity() -> void:
	pass


## Called when the entity is clicked. Must be implemented by subclasses.
func _on_clicked() -> void:
	pass


## Updates the glass sprite texture and type.
func update_glass_type(type: String, template: Sprite2D) -> void:
	glass_type = type
	if glass != null and template != null:
		glass.texture = template.texture


## Animates the glass bobbing while the entity is walking.
func _animate_glass_walking(delta: float) -> void:
	if glass == null:
		return

	if current_state == EntityState.WALKING and sprite != null:
		# Sync glass bobbing with walking frames: 2 pixels offset on every other frame
		var y_offset = -1.0 if (sprite.frame % 2 != 0) else 1.0
		glass.position = _glass_initial_pos + Vector2(0, y_offset)
	elif current_state != EntityState.HIDING:
		glass.position = glass.position.lerp(_glass_initial_pos, delta * 10.0)


## Animates the glass spawning effect.
func _animate_glass_spawn() -> void:
	if glass == null:
		return

	glass.position = _glass_initial_pos + Vector2(0, 50)
	glass.modulate.a = 1.0

	var duration = spawn_delay

	var tween = create_tween()
	(
		tween
		. tween_property(glass, "position", _glass_initial_pos, duration)
		. set_trans(Tween.TRANS_QUART)
		. set_ease(Tween.EASE_OUT)
	)


## Animates the glass disappearance when the entity leaves or is removed.
func _animate_glass_disappearance() -> void:
	if glass == null:
		return

	var tween = create_tween()
	tween.set_parallel(true)
	tween.tween_property(glass, "position", _glass_initial_pos + Vector2(0, 50), 0.5).set_trans(
		Tween.TRANS_LINEAR
	)
	tween.tween_property(glass, "modulate:a", 0.0, 0.5).set_trans(Tween.TRANS_LINEAR)


## Returns true if the entity can currently be clicked.
func can_be_clicked() -> bool:
	if hearts <= 0 or is_disappearing:
		return false

	# Lock click if during spawn animation or hidden
	if _spawn_timer < spawn_delay or current_state == EntityState.HIDING:
		return false

	return true


## Handles a click attempt on this entity.
func try_click(game: Node) -> void:
	if can_be_clicked():
		hearts -= 1
		SfxManager.play_click_sound()
		if hearts <= 0:
			die()
			if "entities_killed" in game:
				game.entities_killed += 1
		else:
			_on_clicked()


func _physics_process(delta: float) -> void:
	if hearts <= 0:
		return

	_update_animation()

	if _spawn_timer < spawn_delay:
		_spawn_timer += delta
		return

	process_entity(delta)
	_animate_glass_walking(delta)

	match current_state:
		EntityState.WALKING:
			if _door != null and heading_to_door:
				var door_pos = _door.global_position
				var target_pos = door_pos
				target_pos.y += _target_y_offset

				# Improved pathfinding to avoid "Wall Left" and "Wall Right" flanking the door
				var dist_to_door_x = door_pos.x - global_position.x

				if dist_to_door_x > 20:
					target_pos.x = door_pos.x - 20
				else:
					# Once close enough and aligned, aim BEYOND the door to ensure crossing it
					target_pos.x = door_pos.x + 1000

				walk_direction = (target_pos - global_position).normalized()

			velocity = walk_direction * walk_speed
			move_and_slide()

		EntityState.STUNNED:
			velocity = Vector2.ZERO


## Disables interactions and collisions when the entity enters the door.
func set_entered_door() -> void:
	heading_to_door = false
	z_index = -1

	# Disable clicking and collisions safely
	set_deferred("collision_layer", 0)
	set_deferred("collision_mask", 0)
	if click_area != null:
		click_area.set_deferred("monitoring", false)
		click_area.set_deferred("monitorable", false)
		click_area.input_pickable = false


## Custom entity processing logic. To be overridden by subclasses.
func process_entity(_delta: float) -> void:
	pass


## Updates the entity's animation based on state and timers.
func _update_animation() -> void:
	if sprite == null:
		return

	if _spawn_timer < spawn_delay:
		# Play jump animation backwards for spawning (emerging)
		play_synced_animation(anim_prefix + "_jump", true, spawn_delay)
		return

	if current_state == EntityState.WALKING:
		play_synced_animation(anim_prefix + "_walk")

		if walk_direction.x != 0:
			sprite.flip_h = walk_direction.x < 0
	elif current_state == EntityState.STUNNED:
		play_synced_animation(anim_prefix + "_hurt")


## Plays an animation with speed synchronization.
func play_synced_animation(
	anim_name: String, backwards: bool = false, duration: float = -1.0
) -> void:
	if sprite == null or not sprite.sprite_frames.has_animation(anim_name):
		return
	if is_disappearing and anim_name != "disapear":
		return

	var speed = 1.0
	if duration > 0:
		var anim_duration = (
			sprite.sprite_frames.get_frame_count(anim_name)
			/ sprite.sprite_frames.get_animation_speed(anim_name)
		)
		speed = anim_duration / duration

	var final_speed = -speed if backwards else speed

	# Only play if not already playing that animation, OR if we need to enforce speed/direction
	if sprite.animation != anim_name:
		sprite.play(anim_name, final_speed, backwards)
	elif not is_equal_approx(sprite.get_playing_speed(), final_speed):
		# If speed changed, update it. play() handles this without resetting frame if anim is same.
		sprite.play(anim_name, final_speed, backwards)


## Handles the entity's death, playing sound and animating it towards the trash.
func die() -> void:
	current_state = EntityState.WALKING  # Just to ensure it's not stunned anymore for movement

	SfxManager.play_death_sound()

	set_deferred("collision_layer", 0)
	set_deferred("collision_mask", 0)

	if click_area != null:
		click_area.set_deferred("monitoring", false)
		click_area.set_deferred("monitorable", false)
		click_area.input_pickable = false

	var target_pos = Vector2(64, 602)
	if _trash != null:
		target_pos = _trash.global_position

	var tween = create_tween()
	tween.set_parallel(true)
	(
		tween
		. tween_property(self, "global_position", target_pos, 0.5)
		. set_trans(Tween.TRANS_QUAD)
		. set_ease(Tween.EASE_IN)
	)
	tween.tween_property(self, "scale", Vector2.ZERO, 0.5)

	tween.set_parallel(false)
	tween.chain().tween_callback(queue_free)


## Asynchronously handles the entity's disappearance animation.
func disappear() -> void:
	if is_disappearing:
		return
	is_disappearing = true

	set_deferred("collision_layer", 0)
	set_deferred("collision_mask", 0)
	velocity = Vector2.ZERO
	set_physics_process(false)

	_animate_glass_disappearance()

	if sprite != null and sprite.sprite_frames.has_animation("disapear"):
		sprite.play("disapear")
		await sprite.animation_finished
		if not is_inside_tree():
			return

	await get_tree().create_timer(0.1).timeout
	if not is_inside_tree():
		return
	queue_free()


## Returns the type of glass carried by this entity.
func get_glass_type() -> String:
	return glass_type


## Sets the walking direction for this entity.
func set_walk_direction(direction: Vector2) -> void:
	walk_direction = direction.normalized()


## Sets the movement speed of the entity.
func set_speed(speed: float) -> void:
	walk_speed = speed


## Gets the current movement speed of the entity.
func get_speed() -> float:
	return walk_speed
