class_name CreditMenu
extends GeneralMenu

## Displays the game credits.

signal close_requested


func _ready() -> void:
	super._ready()


## Signals that the close button was pressed.
func _on_close_button_pressed() -> void:
	close()
	close_requested.emit()
