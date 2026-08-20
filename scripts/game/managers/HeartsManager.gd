class_name HeartsManager
extends GameManagerInterface

## Handles player health and heart UI animation.
const MAX_HEARTS: int = 5
const SPACING: float = 96.0
var heart_nodes: Array[Node2D] = []


func _ready() -> void:
	GameEvents.heart_granted.connect(grant_heart)


func update_hearts() -> void:
	for heart in heart_nodes:
		if is_instance_valid(heart):
			heart.queue_free()
	heart_nodes.clear()

	for i in range(game.hearts):
		var heart := game.heart_scene.instantiate()
		heart.position = Vector2(i * SPACING, 0)
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


func grant_heart() -> void:
	if game.hearts < MAX_HEARTS:
		game.hearts += 1
		# Placeholder sfx
		SfxManager.play_health_granted_sound()

		heart_nodes = heart_nodes.filter(func(h: Node2D) -> bool: return is_instance_valid(h))

		var heart: Node2D
		if heart_nodes.size() == game.hearts - 1:
			heart = game.heart_scene.instantiate()
			heart.position = Vector2((game.hearts - 1) * SPACING, 0)
			heart.modulate.a = 0.0
			if heart.material is ShaderMaterial:
				(heart.material as ShaderMaterial).set_shader_parameter("flash_value", 1.0)
			game.heart_container.add_child(heart)
			heart_nodes.append(heart)
		else:
			update_hearts()
			if not heart_nodes.is_empty():
				heart = heart_nodes[-1]

		if heart != null and is_instance_valid(heart):
			animate_heart_gain(heart)

		game.vfx_manager.update_desaturation(clamp((1.0 - (game.hearts / 3.0)) * 0.5, 0.0, 0.5))


func animate_heart_gain(heart: Node2D) -> void:
	# 1. Start: Hide & Full White
	heart.modulate.a = 0.0
	if heart.material is ShaderMaterial:
		(heart.material as ShaderMaterial).set_shader_parameter("flash_value", 1.0)

	var tween := create_tween()
	# 2. Then: White fade in
	tween.tween_property(heart, "modulate:a", 1.0, 0.25).set_trans(Tween.TRANS_SINE).set_ease(
		Tween.EASE_OUT
	)
	# 3. Then: Fade to normal color
	if heart.material is ShaderMaterial:
		(
			tween
			. tween_property(heart, "material:shader_parameter/flash_value", 0.0, 0.35)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_OUT)
		)
