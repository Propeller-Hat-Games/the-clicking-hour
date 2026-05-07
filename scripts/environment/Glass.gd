class_name Glass
extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary[String, Variant] = {}
# Affects the changes in probabilities, higher influence means repetition is less likely
@export var influence_curve: Curve
var probabilities: Dictionary[String, float] = {}


## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


func _ready() -> void:
	var keys: Array[String] = sprites.keys()
	var ratio: float = 1.0 / (keys.size())
	for type in sprites.keys():
		probabilities[type] = ratio


## Returns a random glass key from the available sprites.
func get_random_glass(current_wave: int) -> String:
	var keys: Array[String] = sprites.keys()
	var size := keys.size()
	# Failsafe in case of an eventual error to avoid NULL access on dictionary
	var return_key := keys[0]
	var probability_sum: float = 0.0

	var random_float: float = randf()
	for key in keys:
		probability_sum += probabilities[key]
		if random_float <= probability_sum:
			return_key = key
			break

	# The two values used in the clamp function are the values present in the
	# Min domain and Max domain of the curve
	var probability_influence: float = influence_curve.sample(clamp(current_wave, 0, 20))

	# Dont count chosen key, as we distribute probability to the others
	var ratio: float = probabilities[return_key] / (size - 1) * probability_influence

	# Probabilities may go negative or above 1, it isn't an issue as the sum of all probabilities
	# will remain equal to 1
	for key in keys:
		if key == return_key:
			probabilities[key] -= ratio
		else:
			probabilities[key] += ratio / (size - 1)

	return return_key
