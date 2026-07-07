class_name MainMenu
extends GeneralMenu

## Controls the Main Menu interactions.

signal play_button_pressed

@export var credit_menu_scene: PackedScene
@export var options_menu_scene: PackedScene

var _credit_menu_instance: Control
var _options_menu_instance: Control


func _ready() -> void:
	super._ready()
	$CanvasLayer/Window/VDiv/Version.text = (
		"v" + ProjectSettings.get_setting("application/config/version")
	)

	if OS.has_feature("web"):
		$CanvasLayer/Window/VDiv/Grid/QuitButton.disabled = true


## Signals that the play button was pressed.
func _on_play_button_pressed() -> void:
	play_button_pressed.emit()


## Opens the credit menu.
func _on_credits_button_pressed() -> void:
	if _credit_menu_instance != null:
		return
	if credit_menu_scene != null:
		var credit_menu: CreditMenu = credit_menu_scene.instantiate()
		_credit_menu_instance = credit_menu
		add_child(_credit_menu_instance)
		if get_parent() is GameManager:
			get_parent().discord_rpc_manager.set_credits_menu()
		credit_menu.close_requested.connect(_on_credit_menu_closed)
		credit_menu.tree_exited.connect(func(): _credit_menu_instance = null)


## Callback for when the credit menu is closed.
func _on_credit_menu_closed() -> void:
	if get_parent() is GameManager:
		get_parent().discord_rpc_manager.set_main_menu()


## Opens the options menu.
func _on_options_button_pressed() -> void:
	if _options_menu_instance != null:
		return
	var scene_to_instantiate := options_menu_scene

	if scene_to_instantiate == null:
		# Fallback if scene is not exported
		scene_to_instantiate = load("res://scenes/ui/menus/options_menu.tscn")

	if scene_to_instantiate != null:
		var options_menu: OptionsMenu = scene_to_instantiate.instantiate()
		_options_menu_instance = options_menu
		add_child(_options_menu_instance)
		if get_parent() is GameManager:
			get_parent().discord_rpc_manager.set_settings_menu()
		options_menu.close_requested.connect(_on_options_menu_closed)
		options_menu.tree_exited.connect(func(): _options_menu_instance = null)


## Callback for when the options menu is closed.
func _on_options_menu_closed() -> void:
	if get_parent() is GameManager:
		get_parent().discord_rpc_manager.set_main_menu()


## Quits the application.
func _on_quit_button_pressed() -> void:
	get_tree().quit()
