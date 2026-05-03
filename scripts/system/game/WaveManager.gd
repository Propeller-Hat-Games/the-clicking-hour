extends Node

## Handles wave progression and the spawning loop.

var game: GameManager
var unboarding_closed: bool = false


func init(p_game: GameManager) -> void:
	game = p_game


func start_game() -> void:
	game.current_wave = 0
	game.hearts = 3
	game.hearts_manager.update_hearts()
	game.entities_killed = 0
	game.glass_passed = 0

	game.discord_rpc_manager.set_playing(0, false)

	MusicManager.fade_out()

	if not SettingsManager.has_seen_onboarding:
		if game.unboarding != null:
			game.unboarding.visible = true
			unboarding_closed = false
			while not unboarding_closed:
				await get_tree().process_frame
		SettingsManager.has_seen_onboarding = true

	next_wave(game.current_wave)


func next_wave(wave_number: int) -> void:
	game.conditions_manager.generate_conditions()
	game.current_wave += 1

	game.is_night_mode = (
		game.current_wave > 2 and not game.is_night_mode and game._rng.randf() < 0.3
	)
	game.vfx_manager.update_night_mode()

	game.settings_button.disabled = false

	await get_tree().create_timer(1.0, false).timeout
	if not is_inside_tree():
		return

	if game.door:
		game.door.open(SfxManager)
	print("[WAVE] Wave %d started!" % game.current_wave)
	if game.is_night_mode:
		print("[WAVE] This wave is night mode!")

	MusicManager.play_game_music(game.is_night_mode)
	game.discord_rpc_manager.set_playing(game.current_wave, game.is_night_mode)

	await get_tree().create_timer(1.0, false).timeout
	if not is_inside_tree():
		return

	game.is_spawning = true

	while game.is_spawning:
		if game.spawn_area:
			game.spawn_area.spawn_entity(game)
			# The two numbers are respectively the min and max domain of the curve
			# We clamp the wave value to stay inside the curve domain
		var delay = (
			game.delay_curve.sample(clamp(wave_number, 0, 50)) * game._rng.randf_range(1.5, 3)
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

	if game.door:
		game.door.close(SfxManager)
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

	var transition_scene = load("res://scenes/ui/transition.tscn")
	var transition_instance = transition_scene.instantiate()
	game.add_child(transition_instance)
	transition_instance.set_completed_wave(game.current_wave)

	await get_tree().create_timer(5.0, false).timeout
	if not is_inside_tree():
		return

	transition_instance.close_window()
	if not is_inside_tree():
		return

	next_wave(game.current_wave)


func end_game() -> void:
	game.is_spawning = false
	game.settings_button.disabled = true
	if game.door:
		game.door.close(SfxManager)
	if game.spawn_area:
		game.spawn_area.kill_every_entities()
	await MusicManager.fade_out()

	var game_over_scene = load("res://scenes/ui/game_over.tscn")
	var game_over_instance = game_over_scene.instantiate()
	game.add_child(game_over_instance)
	game_over_instance.set_waves_survived(
		game.current_wave - 1, game.entities_killed, game.glass_passed
	)
	game.discord_rpc_manager.set_game_over(game.current_wave - 1)
	await MusicManager.play_menu_music()
