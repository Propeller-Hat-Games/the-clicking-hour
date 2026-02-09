extends Entity

## Entity that teleports to a random location in its spawn area when clicked.
## Requires 3 clicks to defeat.

func initialize_entity() -> void:
	anim_prefix = "tp"
	hearts = 3

func _on_clicked() -> void:
	if current_state != EntityState.WALKING or is_disappearing:
		return

	current_state = EntityState.HIDING # Use Hiding state to disable movement/animation updates

	# Disappear animation (Jump)
	var anim = anim_prefix + "_jump"
	var anim_duration = 0.2
	play_synced_animation(anim, false, anim_duration)

	# Animate glass down
	if glass != null:
		var tween = create_tween()
		tween.tween_property(glass, "position", _glass_initial_pos + Vector2(0, 50), anim_duration).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	await get_tree().create_timer(anim_duration).timeout
	if not is_inside_tree() or is_disappearing:
		return

	# Teleport to random position in spawn area
	var parent = get_parent()
	if parent.has_method("get_valid_random_position"):
		position = parent.get_valid_random_position(self)

	# Re-appear animation (Inverse Jump)
	play_synced_animation(anim, true, anim_duration)

	# Animate glass up
	if glass != null:
		var tween = create_tween()
		tween.tween_property(glass, "position", _glass_initial_pos, anim_duration).set_trans(Tween.TRANS_QUAD).set_ease(Tween.EASE_OUT)

	await get_tree().create_timer(anim_duration).timeout
	if not is_inside_tree() or is_disappearing:
		return

	current_state = EntityState.WALKING