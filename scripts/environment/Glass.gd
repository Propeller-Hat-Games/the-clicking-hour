extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary = {}

## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


## Returns a random glass key from the available sprites.
func get_random_glass() -> String:
	var keys = sprites.keys()
	if keys.size() == 0:
		return ""
	var random_index = randi() % keys.size()
	return keys[random_index]
