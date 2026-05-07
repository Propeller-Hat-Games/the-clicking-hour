class_name ConditionsManager
extends GameManagerInterface

## Handles board updates and requirement checking.

var required_glass_types: Array[String] = []
var required_glass_counts: Array[int] = []


func load_conditions_manager() -> void:
	if game.board == null:
		printerr("[CONDITIONS] Board export is not assigned in GameManager!")
		return

	game.board.z_index = 100
	game.glass_manager.load_glass()


func update_board() -> void:
	if game.board == null:
		return
	game.board.update_board(
		required_glass_types.size(),
		game.glass_manager.get_glass_sprites(required_glass_types),
		required_glass_counts
	)


func generate_conditions() -> void:
	required_glass_types.clear()
	required_glass_counts.clear()

	var glass_type_count := 1 + int(min(2, game.current_wave / 3.0))
	var glass_count := 1 + game.current_wave - glass_type_count

	for new_type in game.glass_manager.n_random_glass_types(glass_type_count):
		required_glass_types.append(new_type)
		required_glass_counts.append(1)

	for i in range(glass_count):
		var index := game._rng.randi() % required_glass_types.size()
		required_glass_counts[index] += 1

	print("[CONDITIONS] Generated conditions:")
	for i in range(glass_type_count):
		print("             - %s : %d" % [required_glass_types[i], required_glass_counts[i]])

	update_board()
	if game.board != null:
		game.board.visible = true


func try_enter_glass(type: String) -> bool:
	var index := required_glass_types.find(type)
	if index == -1:
		return false
	if required_glass_counts[index] > 0:
		required_glass_counts[index] -= 1
		SfxManager.play_correct_glass_sound()
		update_board()
		return true
	return false


func is_conditions_done() -> bool:
	for count in required_glass_counts:
		if count > 0:
			return false
	return true
