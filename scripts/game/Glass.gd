class_name Glass
extends Node2D

## Manages a collection of glass sprites.

@export var sprites: Dictionary[String, GlassInterface] = {}
# Affects the changes in probabilities, higher influence means repetition is less likely
@export var influence_curve: Curve
var probabilities: Dictionary[String, float] = {}


## Returns the dictionary of glass sprites.
func get_sprites() -> Dictionary:
	return sprites


func _ready() -> void:
	GameEvents.entity_dead.connect(apply_effect.bind("dead_effect"))
	GameEvents.entity_clicked.connect(apply_effect.bind("clicked_effect"))
	GameEvents.entity_in_bin.connect(apply_effect.bind("bin_entered_effect"))
	GameEvents.entity_entered_door.connect(apply_effect.bind("door_entered_effect"))
	reset_probabilities()


## Resolves and returns the GlassInterface node for a given key.
func get_glass_interface(key: String) -> GlassInterface:
	if not sprites.has(key):
		return null
	var val = sprites[key]
	if val is GlassInterface:
		return val
	if val is NodePath:
		return get_node_or_null(val) as GlassInterface
	return null


## Returns glass keys that are eligible to appear in wave criteria.
func get_criteria_glass_types() -> Array[String]:
	var criteria_keys: Array[String] = []
	for key in sprites.keys():
		var gi := get_glass_interface(key)
		if gi == null or gi.can_appear_in_criteria:
			criteria_keys.append(key)
	return criteria_keys


## Returns standard (non-special) glass keys.
func get_standard_glass_types() -> Array[String]:
	var standard_keys: Array[String] = []
	for key in sprites.keys():
		var gi := get_glass_interface(key)
		if gi == null or not gi.is_special:
			standard_keys.append(key)
	return standard_keys


## Returns special glass keys.
func get_special_glass_types() -> Array[String]:
	var special_keys: Array[String] = []
	for key in sprites.keys():
		var gi := get_glass_interface(key)
		if gi != null and gi.is_special:
			special_keys.append(key)
	return special_keys


## Every standard glass has the same chance of being selected initially
func reset_probabilities() -> void:
	var keys: Array[String] = get_standard_glass_types()
	if keys.is_empty():
		return
	var ratio: float = 1.0 / (keys.size())
	probabilities.clear()
	for type in keys:
		probabilities[type] = ratio


## Returns a random glass key from the available sprites.
## Evaluates special glasses outside tutorial wave, then standard glasses.
func get_random_glass(current_wave: int) -> String:
	if current_wave > 0:
		var special_keys := get_special_glass_types()
		var total_special_chance: float = 0.0
		for key in special_keys:
			var gi := get_glass_interface(key)
			if gi != null:
				total_special_chance += gi.spawn_chance

		if total_special_chance > 0.0:
			var roll := randf()
			if roll < total_special_chance:
				var cumulative: float = 0.0
				for key in special_keys:
					var gi := get_glass_interface(key)
					if gi != null:
						cumulative += gi.spawn_chance
						if roll < cumulative:
							return key
				return special_keys[0]

	var keys: Array[String] = get_standard_glass_types()
	if keys.is_empty():
		var all_keys: Array[String] = sprites.keys()
		return all_keys[0] if not all_keys.is_empty() else ""

	var size := keys.size()
	# Failsafe in case of an eventual error to avoid NULL access on dictionary
	var return_key := keys[0]
	var probability_sum: float = 0.0

	var random_float: float = randf()
	for key in keys:
		probability_sum += probabilities.get(key, 0.0)
		if random_float <= probability_sum:
			return_key = key
			break

	# The two values used in the clamp function are the values present in the
	# Min domain and Max domain of the curve
	var probability_influence: float = 0.0
	if influence_curve != null:
		probability_influence = influence_curve.sample(clamp(current_wave, 0, 20))

	if size > 1:
		# Dont count chosen key, as we distribute probability to the others
		var current_p: float = probabilities.get(return_key, 0.0)
		var ratio: float = current_p / (size - 1) * probability_influence

		# Probabilities may go negative or above 1, it isn't an issue as the sum of all probabilities
		# will remain equal to 1
		for key in keys:
			if key == return_key:
				probabilities[key] = probabilities.get(key, 0.0) - ratio
			else:
				probabilities[key] = probabilities.get(key, 0.0) + ratio / (size - 1)

	return return_key


func apply_effect(entity: Entity, type_of_effect: String) -> void:
	var type := entity.glass_type
	var gi := get_glass_interface(type)
	if gi != null:
		gi.call(type_of_effect)
