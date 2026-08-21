class_name TeleportEntity
extends Entity

## Entity that teleports to a random location in its spawn area when clicked.
## Requires 3 clicks to defeat.


func initialize_entity() -> void:
	anim_prefix = &"tp"
	hearts = 3


func _on_clicked() -> void:
	if current_state != EntityState.WALKING or is_disappearing:
		return

	current_state = EntityState.HIDING  # Use Hiding state to disable movement/animation updates

	# Disappear animation (Jump)
	var anim := StringName(anim_prefix + "_jump")
	var anim_duration := 0.2
	play_synced_animation(anim, false, anim_duration)

	var tween := create_tween()

	# Animate glass down
	if glass != null:
		(
			tween
			. tween_property(glass, "position", _glass_initial_pos + Vector2(0, 50), anim_duration)
			. set_trans(Tween.TRANS_QUAD)
			. set_ease(Tween.EASE_OUT)
		)
	else:
		tween.tween_interval(anim_duration)

	# Teleport to random position in spawn area and play re-appear animation
	tween.tween_callback(
		func():
			if is_disappearing:
				return
			var parent := get_parent()
			if parent != null and parent.has_method(&"get_valid_random_position"):
				position = parent.get_valid_random_position(self)
			play_synced_animation(anim, true, anim_duration)
	)

	# Animate glass up
	if glass != null:
		(
			tween
			. tween_property(glass, "position", _glass_initial_pos, anim_duration)
			. set_trans(Tween.TRANS_QUAD)
			. set_ease(Tween.EASE_OUT)
		)
	else:
		tween.tween_interval(anim_duration)

	tween.tween_callback(
		func():
			if not is_disappearing:
				current_state = EntityState.WALKING
	)
