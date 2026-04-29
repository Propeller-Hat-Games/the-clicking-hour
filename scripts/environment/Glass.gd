extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary = {}
# Affects the changes in probabilities, higher power means repetition is less likely
@export var probability_power: int = 1
var probabilities: Dictionary = {}


## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


#func _ready() -> void:
#for type in sprites:
#lastest_amount_of_apparitions_per_type[type] = 0


func _ready() -> void:
	var keys = sprites.keys()
	var ratio: float = 1.0 / (keys.size())
	for type in sprites.keys():
		probabilities[type] = ratio


## Returns a random glass key from the available sprites.
#func get_random_glass() -> String:
#var keys = sprites.keys()
#var return_type: String
#
#return_type = keys[-1]
#for type in keys.slice(0, -1):
#var quot: float = (
#(1.0 + lastest_amount_of_apparitions_per_type[type])
#/ (keys.size() + last_apparitions.size())
#)
#var probability: float = probability_curve.sample(quot)
#if randf() <= probability:
#return_type = type
#break
#
#if number_of_apparitions >= max_number_of_apparitions:
#var temp_type = last_apparitions.pop_front()
#lastest_amount_of_apparitions_per_type[temp_type] -= 1
#last_apparitions.push_back(return_type)
#lastest_amount_of_apparitions_per_type[return_type] += 1
#number_of_apparitions += 1
#
#return return_type


func get_random_glass() -> String:
	print(probabilities)

	var keys = sprites.keys()
	var size = keys.size()
	# Failsafe in case of an eventual error to not get NULL access on dictionary
	var return_key = keys[0]
	var probability_sum: float = 0.0

	var random_float: float = randf()
	for key in keys:
		probability_sum += probabilities[key]
		if random_float <= probability_sum:
			return_key = key
			break

	# Dont count chosen key, as we distribute probability to the others
	var ratio: float = probabilities[return_key] / (size - 1)

	for key in keys:
		if key == return_key:
			probabilities[key] -= ratio * probability_power
		else:
			probabilities[key] += ratio / (size - 1) * probability_power

	return return_key
