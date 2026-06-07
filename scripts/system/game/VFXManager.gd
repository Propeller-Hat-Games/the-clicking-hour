class_name VFXManager
extends GameManagerInterface

## Handles cursor, VHS effect, and night mode visuals.

var vhs_layer: CanvasLayer
var _is_first_run: bool = true
var _night_mode_tween: Tween = null


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
	if not _is_first_run and game.black_canvas:
		var currently_night := game.black_canvas.visible and game.black_canvas.color.r < 0.5
		if game.is_night_mode == currently_night:
			return

	update_cursor()

	if _is_first_run:
		_is_first_run = false
		if game.background:
			game.background.update_animated_sprite(game.is_night_mode)
		if game.black_canvas:
			game.black_canvas.visible = game.is_night_mode
			game.black_canvas.color = (
				Color(0.039, 0.039, 0.039, 1.0) if game.is_night_mode else Color(1.0, 1.0, 1.0, 1.0)
			)
		if game.cursor_light:
			game.cursor_light.visible = game.is_night_mode
			game.cursor_light.energy = 0.75 if game.is_night_mode else 0.0
		for node in get_tree().get_nodes_in_group(&"Lights"):
			if node is PointLight2D:
				node.energy = 1.0 if game.is_night_mode else 0.5
		return

	if _night_mode_tween != null:
		_night_mode_tween.kill()
		_night_mode_tween = null

	var duration := 2.0
	_night_mode_tween = create_tween()
	_night_mode_tween.set_parallel(true)

	if game.background:
		game.background.update_animated_sprite(game.is_night_mode)

	if game.black_canvas:
		if not game.black_canvas.visible:
			game.black_canvas.color = Color(1.0, 1.0, 1.0, 1.0)
		game.black_canvas.visible = true
		var target_color := (
			Color(0.039, 0.039, 0.039, 1.0) if game.is_night_mode else Color(1.0, 1.0, 1.0, 1.0)
		)
		(
			_night_mode_tween
			. tween_property(game.black_canvas, "color", target_color, duration)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_IN_OUT)
		)

	if game.cursor_light:
		if game.is_night_mode:
			if not game.cursor_light.visible:
				game.cursor_light.energy = 0.0
			game.cursor_light.visible = true
			(
				_night_mode_tween
				. tween_property(game.cursor_light, "energy", 0.75, duration)
				. set_trans(Tween.TRANS_SINE)
				. set_ease(Tween.EASE_IN_OUT)
			)
		else:
			(
				_night_mode_tween
				. tween_property(game.cursor_light, "energy", 0.0, duration)
				. set_trans(Tween.TRANS_SINE)
				. set_ease(Tween.EASE_IN_OUT)
			)

	for node in get_tree().get_nodes_in_group(&"Lights"):
		if node is PointLight2D:
			var target_energy := 1.0 if game.is_night_mode else 0.5
			(
				_night_mode_tween
				. tween_property(node, "energy", target_energy, duration)
				. set_trans(Tween.TRANS_SINE)
				. set_ease(Tween.EASE_IN_OUT)
			)

	_night_mode_tween.chain().set_parallel(false).tween_callback(
		func():
			if game.black_canvas and not game.is_night_mode:
				game.black_canvas.visible = false
			if game.cursor_light and not game.is_night_mode:
				game.cursor_light.visible = false
			_night_mode_tween = null
	)


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
