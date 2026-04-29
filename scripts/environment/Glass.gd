extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary = {}
@export var probability_curve: Curve
@export var max_number_of_apparitions: int
var lastest_amount_of_apparitions_per_type: Dictionary = {}
var last_apparitions: Array[String] = []
var number_of_apparitions: int = 0


## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


func _ready() -> void:
	for type in sprites:
		lastest_amount_of_apparitions_per_type[type] = 0


## Returns a random glass key from the available sprites.
func get_random_glass() -> String:
	var keys = sprites.keys()
	var return_type: String

	return_type = keys[-1]
	for type in keys.slice(0, -1):
		var quot: float = (
			(1.0 + lastest_amount_of_apparitions_per_type[type])
			/ (keys.size() + last_apparitions.size())
		)
		var probability: float = probability_curve.sample(quot)
		if randf() <= probability:
			return_type = type
			break

	if number_of_apparitions >= max_number_of_apparitions:
		var temp_type = last_apparitions.pop_front()
		lastest_amount_of_apparitions_per_type[temp_type] -= 1
	last_apparitions.push_back(return_type)
	lastest_amount_of_apparitions_per_type[return_type] += 1
	number_of_apparitions += 1

	return return_type
