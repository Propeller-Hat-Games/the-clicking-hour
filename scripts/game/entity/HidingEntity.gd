class_name HidingEntity
extends Entity

## Entity that hides in the ground when clicked and re-emerges after a duration.
## Requires multiple clicks to defeat.

const HIDE_DURATION: float = 3.0
const DIG_ANIM_DURATION: float = 0.3


func initialize_entity() -> void:
	anim_prefix = &"dig"
	hearts = 3


func _on_clicked() -> void:
	if current_state == EntityState.HIDING or is_disappearing:
		return

	current_state = EntityState.HIDING

	# Hide in ground (Jump animation played normally = go down)
	var anim_name := StringName(anim_prefix + "_jump")
	play_synced_animation(anim_name, false, DIG_ANIM_DURATION)

	var tween := create_tween()

	# Animate glass down
	if glass != null:
		(
			tween
			. tween_property(
				glass, "position", _glass_initial_pos + Vector2(0, 75), DIG_ANIM_DURATION
			)
			. set_trans(Tween.TRANS_QUART)
			. set_ease(Tween.EASE_IN)
		)
	else:
		tween.tween_interval(DIG_ANIM_DURATION)

	# Stay hidden
	tween.tween_interval(HIDE_DURATION - (2.0 * DIG_ANIM_DURATION))

	# Emerge (Jump animation played backwards = go up)
	tween.tween_callback(
		func():
			if is_disappearing:
				return
			SfxManager.play_entity_emergence_sound()
			play_synced_animation(anim_name, true, DIG_ANIM_DURATION)
	)

	# Animate glass up
	if glass != null:
		(
			tween
			. tween_property(glass, "position", _glass_initial_pos, DIG_ANIM_DURATION)
			. set_trans(Tween.TRANS_QUART)
			. set_ease(Tween.EASE_OUT)
		)
	else:
		tween.tween_interval(DIG_ANIM_DURATION)

	tween.tween_callback(
		func():
			if not is_disappearing:
				current_state = EntityState.WALKING
	)


func _update_animation() -> void:
	# If hidden, prevent base UpdateAnimation from interfering
	if current_state != EntityState.HIDING:
		super._update_animation()
