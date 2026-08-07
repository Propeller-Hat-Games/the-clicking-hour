extends GlassInterface


func effect() -> void:
	GameEvents.heart_granted.emit()
