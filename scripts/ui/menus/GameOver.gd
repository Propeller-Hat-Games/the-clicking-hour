class_name GameOver
extends GeneralMenu

## Displayed when the game is lost. Shows stats and restart button.

@onready var _survive_value: Label = $CanvasLayer/Window/Div/SurviveValue
@onready var _waiters_value: Label = $CanvasLayer/Window/Div/WaitersValue
@onready var _glasses_value: Label = $CanvasLayer/Window/Div/GlassesValue


## Updates the game over screen with the session statistics.
## [param waves] Number of waves survived.
## [param kills] Total entities killed.
## [param passed] Total glasses/items delivered/passed.
func set_waves_survived(waves: int, kills: int, passed: int) -> void:
	_survive_value.text = str(waves)
	_waiters_value.text = str(kills)
	_glasses_value.text = str(passed)


## Reloads the current scene to restart the game.
func _on_button_pressed() -> void:
	get_tree().reload_current_scene()
