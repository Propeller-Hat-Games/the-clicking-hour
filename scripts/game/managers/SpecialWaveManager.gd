class_name SpecialWaveManager
extends GameManagerInterface

# SW = SPECIAL_WAVE
const SW_NUMBER: int = 20  # at which wave is there a special effect
const SW_OFFSET: float = 20.0  # the offset from where the text comes from
const SW_PAUSE: float = 10.0  # the number of seconds the text displays on screen


func handle_special_wave(owner: Node) -> void:
	if game.current_wave == SW_NUMBER:
		game.background.set_special_background(true)
		MusicManager.play_special_music()
		await play_special_wave_ui_animation(owner)
	else:
		game.background.set_special_background(false)
		MusicManager.play_game_music(game.is_night_mode)
		game.special_wave_ui.visible = false


func play_special_wave_ui_animation(owner: Node) -> void:
	var ui: Control = game.special_wave_ui
	if ui == null:
		return

	ui.visible = true

	var final_pos: Vector2 = ui.position
	var start_pos: Vector2 = final_pos + Vector2(-SW_OFFSET, 0)

	ui.position = start_pos
	ui.modulate.a = 0.0

	var tween := owner.create_tween()

	tween.parallel().tween_property(ui, "modulate:a", 1.0, 0.5)
	tween.parallel().tween_property(ui, "position", final_pos, 0.5)

	tween.tween_interval(SW_PAUSE)

	tween.tween_property(ui, "modulate:a", 0.0, 0.5)
	tween.parallel().tween_property(ui, "position", final_pos + Vector2(SW_OFFSET, 0), 0.5)

	await tween.finished

	if is_instance_valid(ui):
		ui.visible = false
		ui.position = final_pos
