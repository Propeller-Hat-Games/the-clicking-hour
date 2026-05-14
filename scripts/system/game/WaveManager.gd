class_name WaveManager
extends GameManagerInterface

## Handles wave progression and the spawning loop.

var unboarding_closed: bool = false


func start_game() -> void:
	game.current_wave = 0
	game.hearts = 3
	game.hearts_manager.update_hearts()
	game.entities_killed = 0
	game.glass_passed = 0

	GameEvents.game_started.emit()

	MusicManager.fade_out()

	if not SettingsManager.has_seen_onboarding:
		if game.unboarding != null:
			game.unboarding.visible = true
			unboarding_closed = false
			while not unboarding_closed:
				await get_tree().process_frame
		SettingsManager.has_seen_onboarding = true
		game.debug_panel.update_has_seen_unboarding()
		tutorial_wave()
	else:
		next_wave()


func tutorial_wave() -> void:
	game.conditions_manager.generate_conditions_tutorial()
	start_wave(0, 0)


func next_wave() -> void:
	game.conditions_manager.generate_conditions()
	var next_wave_index := game.current_wave + 1
	var next_night_mode := (
		next_wave_index > 2 and not game.is_night_mode and game._rng.randf() < 0.3
	)
	start_wave(next_wave_index, next_night_mode)


func start_wave(wave_index: int, night_mode: bool) -> void:
	game.current_wave = wave_index
	game.is_night_mode = night_mode

	game.settings_button.disabled = false

	await get_tree().create_timer(1.0, false).timeout
	if not is_inside_tree():
		return

	if game.door:
		game.door.open()
	print("[WAVE] Wave %d started!" % game.current_wave)
	if game.is_night_mode:
		print("[WAVE] This wave is night mode!")

	MusicManager.play_game_music(game.is_night_mode)
	GameEvents.wave_started.emit(game.current_wave, game.is_night_mode)

	await get_tree().create_timer(1.0, false).timeout
	if not is_inside_tree():
		return

	game.is_spawning = true

	while game.is_spawning:
		if game.spawn_area:
			game.spawn_area.spawn_entity(game)
			# The two numbers are respectively the min and max domain of the curve
			# We clamp the wave value to stay inside the curve domain
		var delay := (
			game.delay_curve.sample(clamp(game.current_wave, 0, 50)) * game._rng.randf_range(1.5, 3)
		)

		await get_tree().create_timer(delay, false).timeout
		if not is_inside_tree():
			return
		if not game.is_spawning:
			break


func end_wave() -> void:
	if not game.is_spawning:
		return
	game.is_spawning = false

	print("[WAVE] Wave %d finished!" % game.current_wave)
	GameEvents.wave_finished.emit(game.current_wave)

	if game.door:
		game.door.close()
	if game.spawn_area:
		game.spawn_area.kill_every_entities()
	await MusicManager.fade_out()
	if not is_inside_tree():
		return

	await get_tree().create_timer(0.5, false).timeout
	if not is_inside_tree():
		return

	game.settings_button.disabled = true

	SfxManager.play_jingle_sound()

	var transition_scene: PackedScene = load("res://scenes/ui/transition.tscn")
	var transition_instance: Transition = transition_scene.instantiate()
	game.add_child(transition_instance)
	transition_instance.set_completed_wave(game.current_wave)

	await get_tree().create_timer(5.0, false).timeout
	if not is_inside_tree():
		return

	transition_instance.close_window()
	if not is_inside_tree():
		return

	next_wave()


func end_game() -> void:
	game.is_spawning = false
	game.settings_button.disabled = true
	GameEvents.game_over.emit(game.current_wave - 1)
	if game.door:
		game.door.close()
	if game.spawn_area:
		game.spawn_area.kill_every_entities()
	await MusicManager.fade_out()

	var game_over_scene: PackedScene = load("res://scenes/ui/game_over.tscn")
	var game_over_instance: GameOver = game_over_scene.instantiate()
	game.add_child(game_over_instance)
	game_over_instance.set_waves_survived(
		game.current_wave - 1, game.entities_killed, game.glass_passed
	)
	await MusicManager.play_menu_music()
