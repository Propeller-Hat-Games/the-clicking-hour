class_name CreditMenu
extends GeneralMenu

## Displays the game credits.

signal close_requested


func _ready() -> void:
	super._ready()

	# Update credits list with translated roles
	var credits_label: Label = $CanvasLayer/Window/VDiv/CreditsList
	if credits_label:
		var credits_text := "Ferdinand Del Re (Flamasar) - " + tr(&"ROLE_ART") + "\n"
		credits_text += "Gaspard Ternoy - " + tr(&"ROLE_MUSIC") + "\n"
		credits_text += "Baptiste May - " + tr(&"ROLE_CODE") + "\n"
		credits_text += "Lucas Guglielmetti - " + tr(&"ROLE_CODE") + "\n"
		credits_text += "Hugo Louis Joseph - " + tr(&"ROLE_CODE") + "\n"
		credits_text += "Kamil Charbenaga - " + tr(&"ROLE_CODE") + "\n"
		credits_text += "Clément Thery - " + tr(&"ROLE_CODE")
		credits_label.text = credits_text

	var close_button: Button = $CanvasLayer/Window/VDiv/CloseButton
	if close_button:
		close_button.text = tr(&"OP_RETURN")


## Signals that the close button was pressed.
func _on_close_button_pressed() -> void:
	close_requested.emit()
