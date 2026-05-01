extends PanelContainer

var game: GameManager
var change_wave: int = 0


func init(p_game: GameManager) -> void:
	game = p_game


func _ready() -> void:
	if not OS.is_debug_build():
		hide()
		set_process(false)
		return

	$Content.visible = false
	reset_size()


func _process(_delta: float) -> void:
	if not game:
		return

	$Content/Body/Informations/CurrentWave.text = "Current wave : %d" % game.current_wave
	$Content/Body/Informations/NightMode.text = (
		"Night Mode : %s" % ("ON" if game.is_night_mode else "OFF")
	)
	$Content/Body/Informations/Lives.text = "Lives : %d" % game.hearts
	$Content/Body/Informations/Kills.text = "Kills : %d" % game.entities_killed
	$Content/Body/Informations/Passed.text = "Passed : %d" % game.glass_passed


func _on_open_button_pressed() -> void:
	if game:
		change_wave = game.current_wave
		_update_wave_input()

	$Content.visible = true
	$OpenButton.visible = false
	reset_size()


func _on_close_button_pressed() -> void:
	$Content.visible = false
	$OpenButton.visible = true
	reset_size()


func _update_wave_input() -> void:
	if change_wave < 0:
		change_wave = 0
	$Content/Body/Manage/WaveGroup/Wave.text = str(change_wave)


func _on_wave_less_pressed() -> void:
	change_wave -= 1
	_update_wave_input()


func _on_wave_more_pressed() -> void:
	change_wave += 1
	_update_wave_input()


func _on_wave_text_changed(new_text: String) -> void:
	change_wave = new_text.to_int()
	_update_wave_input()


func _on_reset_button_pressed() -> void:
	if not game:
		return

	game.is_spawning = false
	game.spawn_area.kill_every_entities()

	game.wave_manager.start_wave(game.current_wave, game.is_night_mode)


func _on_update_button_pressed() -> void:
	if not game:
		return

	game.is_spawning = false
	game.spawn_area.kill_every_entities()

	var night_mode = $Content/Body/Manage/NightModeGroup/CheckButton.button_pressed
	game.wave_manager.start_wave(change_wave, night_mode)
