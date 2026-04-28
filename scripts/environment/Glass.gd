extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary = {}
@export var probabilityCurve: Curve
var amountOfApparitionsPerTypeInTheLastTen : Dictionary = {}
var lastTenApparitions : Array[String] = []
var amountOfApparitions : int = 0

## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


## Returns a random glass key from the available sprites.
func get_random_glass() -> String:
	var keys = sprites.keys()
	var returnType : String
	
	for type in sprites.keys():
		amountOfApparitionsPerTypeInTheLastTen[type] = 0
	for type in lastTenApparitions:
		amountOfApparitionsPerTypeInTheLastTen[type] += 1
	
	returnType = keys[-1]
	for type in keys.slice(0,-1):
		var quot : float = (1.0+amountOfApparitionsPerTypeInTheLastTen[type])/(keys.size()+lastTenApparitions.size())
		var probability : float = probabilityCurve.sample(quot)
		print(type ,"  ",quot, "  ",probability ,"  ",1.0+amountOfApparitionsPerTypeInTheLastTen[type],"  ",keys.size()+lastTenApparitions.size())
		if (randf() <= probability) :
			returnType = type
			break
	
	amountOfApparitions += 1
	if (amountOfApparitions >= 10):
		lastTenApparitions.pop_front()
	lastTenApparitions.push_back(returnType)
	
	return returnType
