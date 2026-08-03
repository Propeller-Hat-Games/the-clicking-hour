extends Sprite2D

func effect() -> void:
	# gameManager.gd is the parent of glass.gd's parent which is itself the parent of special.gd
	var gameManager = get_parent().get_parent()
	gameManager.hearts_manager.grant_heart()
