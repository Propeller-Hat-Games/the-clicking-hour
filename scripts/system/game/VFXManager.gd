class_name VFXManager
extends GameManagerInterface

## Handles cursor, VHS effect, and night mode visuals.

var vhs_layer: CanvasLayer


func _process(_delta: float) -> void:
	if game.cursor_light:
		game.cursor_light.position = get_viewport().get_mouse_position()


func load_vfx() -> void:
	_setup_vhs_effect()
	update_cursor()
	update_night_mode()

	GameEvents.wave_started.connect(func(_num: int, _night: bool): update_night_mode())
	SettingsManager.settings_changed.connect(update_effects_visibility)


func _setup_vhs_effect() -> void:
	vhs_layer = CanvasLayer.new()
	vhs_layer.layer = 255  # Ensure it's on top of everything
	game.add_child(vhs_layer)

	var color_rect := ColorRect.new()
	color_rect.set_anchors_preset(Control.PRESET_FULL_RECT)
	color_rect.mouse_filter = Control.MOUSE_FILTER_IGNORE

	var shader = load("res://assets/shaders/vhs.gdshader")
	var material := ShaderMaterial.new()
	material.shader = shader
	color_rect.material = material
	vhs_layer.add_child(color_rect)
	print("[VFX] VHS Filter loaded!")


func update_effects_visibility() -> void:
	var enabled := SettingsManager.global_effects_enabled
	if vhs_layer != null:
		vhs_layer.visible = enabled


func update_night_mode() -> void:
	update_cursor()
	if game.background:
		game.background.update_animated_sprite(game.is_night_mode)
	if game.black_canvas:
		game.black_canvas.visible = game.is_night_mode
	if game.cursor_light:
		game.cursor_light.visible = game.is_night_mode

	for node in get_tree().get_nodes_in_group(&"Lights"):
		if node is PointLight2D:
			node.energy = 1.0 if game.is_night_mode else 0.5


func update_cursor() -> void:
	var cursor_texture := game.cursor_night if game.is_night_mode else game.cursor_normal
	if cursor_texture == null:
		return

	var img := cursor_texture.get_image()
	img.resize(img.get_width() * 3, img.get_height() * 3, Image.INTERPOLATE_NEAREST)
	var scaled_cursor := ImageTexture.create_from_image(img)
	Input.set_custom_mouse_cursor(scaled_cursor, Input.CURSOR_ARROW, Vector2(30, 15))


func trigger_glitch_effect() -> void:
	if game.glitch_effect:
		game.glitch_effect.trigger_glitch(0.5, 0.15)


func update_desaturation(value: float) -> void:
	if game.glitch_effect:
		game.glitch_effect.set_desaturation(value)
