extends GeneralMenu

## Displayed when the game is lost. Shows stats and restart button.

## Updates the game over screen with the session statistics.
## [param waves] Number of waves survived.
## [param kills] Total entities killed.
## [param passed] Total glasses/items delivered/passed.
func set_waves_survived(waves: int, kills: int, passed: int) -> void:
	var label = get_node("CanvasLayer/Window/VDiv/Data") as Label
	var text = "Vous avez survécu pendant %d vague%s\n" % [waves, "s" if waves > 1 else ""]
	text += "Serveurs supprimés : %d\n" % kills
	text += "Verres livrés : %d" % passed
	label.text = text

## Reloads the current scene to restart the game.
func _on_button_pressed() -> void:
	get_tree().reload_current_scene()
