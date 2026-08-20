extends GlassInterface


func _init() -> void:
	is_special = true
	can_appear_in_criteria = false
	spawn_chance = 0.01


func dead_effect() -> void:
	pass


func door_entered_effect() -> void:
	pass


func bin_entered_effect() -> void:
	GameEvents.heart_granted.emit()


func clicked_effect() -> void:
	pass
