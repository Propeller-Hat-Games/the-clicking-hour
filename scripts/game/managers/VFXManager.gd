class_name VFXManager
extends GameManagerInterface

## Handles cursor, VHS effect, and night mode visuals.

var vhs_layer: CanvasLayer
var cursor_layer: CanvasLayer = null
var cursor_day_sprite: Sprite2D = null
var cursor_night_sprite: Sprite2D = null
var _is_first_run: bool = true
var _night_mode_tween: Tween = null


func _process(_delta: float) -> void:
	var mouse_pos := get_viewport().get_mouse_position()
	if game.cursor_light:
		game.cursor_light.position = mouse_pos

	var is_visible := get_viewport().get_visible_rect().has_point(mouse_pos)
	if cursor_layer:
		cursor_layer.visible = is_visible

	if cursor_day_sprite:
		cursor_day_sprite.position = mouse_pos
	if cursor_night_sprite:
		cursor_night_sprite.position = mouse_pos


func load_vfx() -> void:
	_setup_software_cursor()
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


func _setup_software_cursor() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_HIDDEN

	cursor_layer = CanvasLayer.new()
	cursor_layer.layer = 1000  # Above VHS and all other UI
	game.add_child(cursor_layer)

	if OS.get_name() == "Android":
		return

	# Day cursor sprite
	cursor_day_sprite = Sprite2D.new()
	cursor_day_sprite.texture = game.cursor_normal
	cursor_day_sprite.centered = false
	cursor_day_sprite.scale = Vector2(3, 3)
	cursor_day_sprite.offset = -Vector2(10, 5)  # Hotspot offset (30, 15) / 3
	cursor_layer.add_child(cursor_day_sprite)

	# Night cursor sprite
	cursor_night_sprite = Sprite2D.new()
	cursor_night_sprite.texture = game.cursor_night
	cursor_night_sprite.centered = false
	cursor_night_sprite.scale = Vector2(3, 3)
	cursor_night_sprite.offset = -Vector2(10, 5)  # Hotspot offset (30, 15) / 3
	cursor_night_sprite.modulate.a = 0.0  # Start invisible
	cursor_layer.add_child(cursor_night_sprite)


func _exit_tree() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE


func update_night_mode() -> void:
	if not _is_first_run and game.black_canvas:
		var currently_night := game.black_canvas.visible and game.black_canvas.color.r < 0.5
		if game.is_night_mode == currently_night:
			return

	update_cursor()

	if _is_first_run:
		_is_first_run = false
		if game.background:
			game.background.update_animated_sprite(game.is_night_mode, 0.0)
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
		game.background.update_animated_sprite(game.is_night_mode, duration)

	var start_cursor := 0.0 if game.is_night_mode else 1.0
	var end_cursor := 1.0 if game.is_night_mode else 0.0
	(
		_night_mode_tween
		. tween_method(update_cursor_transition, start_cursor, end_cursor, duration)
		. set_trans(Tween.TRANS_SINE)
		. set_ease(Tween.EASE_IN_OUT)
	)

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
	update_cursor_transition(1.0 if game.is_night_mode else 0.0)


func update_cursor_transition(t: float) -> void:
	if cursor_night_sprite:
		cursor_night_sprite.modulate.a = t


func trigger_glitch_effect() -> void:
	if game.glitch_effect:
		game.glitch_effect.trigger_glitch(0.5, 0.15)


func update_desaturation(value: float) -> void:
	if game.glitch_effect:
		game.glitch_effect.set_desaturation(value)
