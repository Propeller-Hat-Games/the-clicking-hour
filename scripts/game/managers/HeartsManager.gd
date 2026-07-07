class_name HeartsManager
extends GameManagerInterface

## Handles player health and heart UI animation.

var heart_nodes: Array[Node2D] = []


func update_hearts() -> void:
	for heart in heart_nodes:
		if is_instance_valid(heart):
			heart.queue_free()
	heart_nodes.clear()

	var spacing := 96.0

	for i in range(game.hearts):
		var heart := game.heart_scene.instantiate()
		heart.position = Vector2(i * spacing, 0)
		game.heart_container.add_child(heart)
		heart_nodes.append(heart)

	game.vfx_manager.update_desaturation(clamp((1.0 - (game.hearts / 3.0)) * 0.5, 0.0, 0.5))


func start_heart_animation_loop() -> void:
	while is_inside_tree():
		if get_tree().paused:
			await get_tree().process_frame
			continue

		var current_hearts: Array[Node2D] = heart_nodes.duplicate()
		for heart in current_hearts:
			if is_instance_valid(heart):
				var tween := create_tween()
				(
					tween
					. tween_property(heart, "position:y", -15.0, 0.15)
					. set_trans(Tween.TRANS_SINE)
					. set_ease(Tween.EASE_OUT)
				)
				(
					tween
					. tween_property(heart, "position:y", 0.0, 0.4)
					. set_trans(Tween.TRANS_BOUNCE)
					. set_ease(Tween.EASE_OUT)
				)

			await get_tree().create_timer(0.2).timeout
			if not is_inside_tree():
				return

		await get_tree().create_timer(3.0).timeout
		if not is_inside_tree():
			return


func lose_heart() -> void:
	if game.current_wave > 0:
		game.hearts -= 1
	game.vfx_manager.trigger_glitch_effect()
	SfxManager.play_take_damage_sound()
	update_hearts()
	if game.hearts <= 0:
		game.wave_manager.end_game()
