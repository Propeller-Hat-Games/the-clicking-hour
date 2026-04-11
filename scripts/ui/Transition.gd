extends GeneralMenu

## Handles the transition screen between stages.

signal next_stage_requested

var _title_label: Label


func _ready() -> void:
	super._ready()
	_title_label = get_node("CanvasLayer/Window/Title")


## Sets the text to display which wave was completed.
func set_completed_wave(wave_number: int) -> void:
	if _title_label:
		_title_label.text = tr("STAGE_CLEARED") % wave_number


## Closes the transition window and signals for the next stage.
func close_window() -> void:
	next_stage_requested.emit()
	queue_free()
