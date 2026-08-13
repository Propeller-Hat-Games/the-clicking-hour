extends GlassInterface


func dead_effect() -> void:
	pass


func door_entered_effect() -> void:
	pass


func bin_entered_effect() -> void:
	GameEvents.heart_granted.emit()


func clicked_effect() -> void:
	pass
