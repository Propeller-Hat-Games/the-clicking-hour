extends GlassInterface


func effect() -> void:
	# gameManager.gd is the parent of glass.gd's parent which is itself the parent of special.gd
	var game_manager = get_parent().get_parent()
	game_manager.hearts_manager.grant_heart()
