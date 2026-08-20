class_name GlassInterface
extends Sprite2D

## Base class for all glass types and their gameplay effects.

@export var is_special: bool = false
@export var can_appear_in_criteria: bool = true
@export_range(0.0, 1.0, 0.01) var spawn_chance: float = 0.0


func dead_effect() -> void:
	pass


func door_entered_effect() -> void:
	pass


func bin_entered_effect() -> void:
	pass


func clicked_effect() -> void:
	pass
