extends GeneralMenu

## Displays the game credits.

signal close_requested

## Signals that the close button was pressed.
func _on_close_button_pressed() -> void:
	close_requested.emit()
