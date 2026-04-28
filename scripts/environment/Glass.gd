extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary = {}
@export var probabilityCurve: Curve
var amountOfApparitionsPerType : Dictionary = {}
var sumOfApparitions : int = 0

func _ready() -> void:
	for type in sprites.keys():
		amountOfApparitionsPerType[type] = 0

## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


## Returns a random glass key from the available sprites.
func get_random_glass() -> String:
	var keys = sprites.keys()
	
	var returnType = keys[-1]
	for type in keys.slice(0,-1):
		var quot : float = (1.0+amountOfApparitionsPerType[type])/(keys.size()+sumOfApparitions)
		var probability : float = probabilityCurve.sample(quot)
		if (randf() <= probability) :
			returnType = type
			break
	
	amountOfApparitionsPerType[returnType] += 1
	sumOfApparitions += 1
	return returnType
