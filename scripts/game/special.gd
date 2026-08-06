extends GlassInterface


func effect() -> void:
	GameEvents.health_glass_dead.emit()
