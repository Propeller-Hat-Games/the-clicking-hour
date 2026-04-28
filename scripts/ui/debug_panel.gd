extends PanelContainer

var game: GameManager


func init(p_game: GameManager) -> void:
	game = p_game


func _ready() -> void:
	if not OS.is_debug_build():
		hide()
		set_process(false)
		return

	$Container.visible = false
	reset_size()


func _process(_delta: float) -> void:
	if not game:
		return

	$Container/CurrentWave.text = "Current wave : %d" % game.current_wave
	$Container/NightMode.text = "Night Mode : %s" % ("ON" if game.is_night_mode else "OFF")
	$Container/Lives.text = "Lives : %d" % game.hearts
	$Container/Kills.text = "Kills : %d" % game.entities_killed
	$Container/Passed.text = "Passed : %d" % game.glass_passed


func _on_open_button_pressed() -> void:
	$Container.visible = true
	$OpenButton.visible = false
	reset_size()


func _on_close_button_pressed() -> void:
	$Container.visible = false
	$OpenButton.visible = true
	reset_size()
