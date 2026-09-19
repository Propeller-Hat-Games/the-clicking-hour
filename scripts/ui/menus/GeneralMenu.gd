class_name GeneralMenu
extends Control

## Base class for menus, providing common functionality like floating window animation.

@export var background_border: TextureRect
@export var rotation_speed: float = 3.0

var _is_closing: bool = false
var _button_tweens: Dictionary[BaseButton, Tween] = {}


func _ready() -> void:
	var window: Control = get_node_or_null("CanvasLayer/Window")
	if window != null:
		# Set initial state for fade-in
		window.modulate.a = 0.0
		var fade_in_tween := create_tween()
		(
			fade_in_tween
			. tween_property(window, "modulate:a", 1.0, 0.3)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_OUT)
		)

		# Floating animation on a separate Tween
		var tween: Tween = create_tween().set_loops()
		(
			tween
			. tween_property(window, "position:y", window.position.y - 20.0, 2.0)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_IN_OUT)
		)
		(
			tween
			. tween_property(window, "position:y", window.position.y, 2.0)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_IN_OUT)
		)

		var background: TextureRect = background_border
		if background == null:
			background = window.get_node_or_null("BackgroundBorder")
		if background == null:
			background = window.get_node_or_null("background_border")

		if background != null and absf(rotation_speed) > 0.001:
			background.rotation = 0.0
			var duration: float = TAU / absf(rotation_speed)
			var rot_sign: float = signf(rotation_speed)

			if background.texture is GradientTexture2D:
				var grad_tex: GradientTexture2D = (
					background.texture.duplicate() as GradientTexture2D
				)
				grad_tex.fill = GradientTexture2D.FILL_CONIC
				grad_tex.fill_from = Vector2(0.5, 0.5)
				background.texture = grad_tex

				var rotation_tween := background.create_tween().set_loops()
				rotation_tween.tween_method(
					func(angle: float):
						grad_tex.fill_to = Vector2(0.5 + 0.5 * cos(angle), 0.5 + 0.5 * sin(angle)),
					0.0,
					TAU * rot_sign,
					duration
				)
			else:
				background.pivot_offset_ratio = Vector2(0.5, 0.5)
				var rotation_tween := background.create_tween().set_loops()
				(
					rotation_tween
					. tween_property(background, "rotation", TAU * rot_sign, duration)
					. from(0.0)
				)

		_setup_button_effects(self)


## Recursively applies hover and click micro-animations to all child buttons.
func _setup_button_effects(node: Node) -> void:
	for child in node.get_children():
		if child is BaseButton:
			child.pivot_offset = child.size / 2.0
			child.resized.connect(func(): child.pivot_offset = child.size / 2.0)
			child.mouse_entered.connect(_on_button_hover.bind(child, true))
			child.mouse_exited.connect(_on_button_hover.bind(child, false))
			child.button_down.connect(_on_button_press.bind(child, true))
			child.button_up.connect(_on_button_press.bind(child, false))
			child.tree_exiting.connect(func(): _button_tweens.erase(child))
		_setup_button_effects(child)


func _animate_button_scale(
	btn: BaseButton,
	target_scale: Vector2,
	duration: float,
	trans_type: Tween.TransitionType,
	ease_type: Tween.EaseType
) -> void:
	if _button_tweens.has(btn):
		var active_tween: Tween = _button_tweens[btn]
		if active_tween != null and active_tween.is_valid():
			active_tween.kill()
	var tween := btn.create_tween()
	_button_tweens[btn] = tween
	tween.tween_property(btn, "scale", target_scale, duration).set_trans(trans_type).set_ease(
		ease_type
	)


func _on_button_hover(btn: BaseButton, hovered: bool) -> void:
	if btn.disabled:
		return
	var target_scale := Vector2(1.05, 1.05) if hovered else Vector2.ONE
	_animate_button_scale(btn, target_scale, 0.15, Tween.TRANS_BACK, Tween.EASE_OUT)


func _on_button_press(btn: BaseButton, pressed: bool) -> void:
	var target_scale := (
		Vector2.ONE
		if btn.disabled
		else (
			Vector2(0.95, 0.95)
			if pressed
			else (Vector2(1.05, 1.05) if btn.is_hovered() else Vector2.ONE)
		)
	)
	_animate_button_scale(btn, target_scale, 0.1, Tween.TRANS_QUAD, Tween.EASE_OUT)


## Closes the menu by freeing the node.
func close() -> void:
	if _is_closing:
		return
	_is_closing = true

	var window: Control = get_node_or_null("CanvasLayer/Window")
	if window != null:
		var canvas_layer = get_node_or_null("CanvasLayer")
		if canvas_layer != null:
			var blocker := Control.new()
			blocker.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
			blocker.mouse_filter = Control.MOUSE_FILTER_STOP
			canvas_layer.add_child(blocker)

		var fade_out_tween := create_tween()
		(
			fade_out_tween
			. tween_property(window, "modulate:a", 0.0, 0.25)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_IN)
		)
		fade_out_tween.finished.connect(_on_close_animation_finished)
	else:
		_on_close_animation_finished()


func _on_close_animation_finished() -> void:
	queue_free()
