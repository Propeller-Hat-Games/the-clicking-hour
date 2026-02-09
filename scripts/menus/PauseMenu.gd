extends GeneralMenu

## Handles the Pause Menu.

@export var options_menu_scene: PackedScene

var _options_menu_instance: Control

## Resumes the game and closes the pause menu.
func _on_resume_button_pressed() -> void:
	get_tree().paused = false
	MusicManager.set_pause_effect(false)
	queue_free()

## Opens the options menu from the pause screen.
func _on_options_button_pressed() -> void:
	if options_menu_scene != null:
		var options_menu = options_menu_scene.instantiate()
		_options_menu_instance = options_menu
		add_child(_options_menu_instance)
		options_menu.close_requested.connect(_on_options_menu_closed)

## Callback for when the options menu is closed within the pause screen.
func _on_options_menu_closed() -> void:
	if _options_menu_instance != null:
		_options_menu_instance.queue_free()
		_options_menu_instance = null

## Unpauses the game and reloads the current scene to return to the main menu.
func _on_main_menu_button_pressed() -> void:
	get_tree().paused = false
	MusicManager.set_pause_effect(false)
	get_tree().reload_current_scene()
