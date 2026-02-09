extends Entity

## Entity that hides in the ground when clicked and re-emerges after a duration.
## Requires multiple clicks to defeat.

const HIDE_DURATION: float = 3.0
const DIG_ANIM_DURATION: float = 0.3

func initialize_entity() -> void:
	anim_prefix = "dig"
	hearts = 3

func _on_clicked() -> void:
	if current_state == EntityState.HIDING or is_disappearing:
		return

	current_state = EntityState.HIDING

	# Hide in ground (Jump animation played normally = go down)
	var anim_name = anim_prefix + "_jump"
	play_synced_animation(anim_name, false, DIG_ANIM_DURATION)

	# Animate glass down
	if glass != null:
		var tween = create_tween()
		tween.tween_property(glass, "position", _glass_initial_pos + Vector2(0, 75), DIG_ANIM_DURATION).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_IN)

	await get_tree().create_timer(DIG_ANIM_DURATION).timeout
	if not is_inside_tree() or is_disappearing:
		return

	# Stay hidden
	await get_tree().create_timer(HIDE_DURATION - (2.0 * DIG_ANIM_DURATION)).timeout
	if not is_inside_tree() or is_disappearing:
		return

	# Emerge (Jump animation played backwards = go up)
	SfxManager.play_entity_emergence_sound()
	play_synced_animation(anim_name, true, DIG_ANIM_DURATION)

	# Animate glass up
	if glass != null:
		var tween = create_tween()
		tween.tween_property(glass, "position", _glass_initial_pos, DIG_ANIM_DURATION).set_trans(Tween.TRANS_QUART).set_ease(Tween.EASE_OUT)

	await get_tree().create_timer(DIG_ANIM_DURATION).timeout
	if not is_inside_tree() or is_disappearing:
		return

	current_state = EntityState.WALKING

func _update_animation() -> void:
	# If hidden, prevent base UpdateAnimation from interfering
	if current_state != EntityState.HIDING:
		super._update_animation()