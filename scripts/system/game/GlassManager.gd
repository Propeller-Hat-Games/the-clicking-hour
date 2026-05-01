extends Node

## Handles glass asset loading and random glass types.

var game: GameManager
var every_sprites: Dictionary = {}
var glass_node


func init(p_game: GameManager) -> void:
	game = p_game


func load_glass() -> void:
	glass_node = game.get_node_or_null("Glass")
	if glass_node == null:
		printerr("[GLASS] Glass node not found in GameManager!")
		return

	var raw_sprites = glass_node.get_sprites()
	every_sprites.clear()

	print("[GLASS] Loading and resolving sprites:")
	for key in raw_sprites.keys():
		var val = raw_sprites[key]
		var node: Sprite2D = null

		if val is Sprite2D:
			node = val
		elif val is NodePath:
			node = glass_node.get_node_or_null(val)

		if node == null:
			printerr("        - %s: Could not resolve to Sprite2D! (Value: %s)" % [key, str(val)])
		else:
			every_sprites[key] = node
			print("        - %s resolved to %s" % [key, node.name])


func random_glass_type(current_wave: int) -> String:
	return glass_node.get_random_glass(current_wave)


func n_random_glass_types(n: int) -> Array[String]:
	var types_array: Array[String] = []

	var sprites = every_sprites.keys()
	sprites.shuffle()

    # We add the sprites contained in a subpart of the sprites randomly sorted array
	types_array.append_array(sprites.slice(0, min(every_sprites.size(), n)))

	return types_array


func get_glass_sprite(type: String) -> Sprite2D:
	return every_sprites.get(type)


func get_glass_sprites(keys: Array[String]) -> Array[Sprite2D]:
	var res: Array[Sprite2D] = []
	for key in keys:
		if every_sprites.has(key):
			res.append(every_sprites[key])
	return res
