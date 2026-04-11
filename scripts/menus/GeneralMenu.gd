class_name GeneralMenu
extends Control

## Base class for menus, providing common functionality like floating window animation.

@export var background_border: TextureRect
@export var rotation_speed: float = 3.0


func _ready() -> void:
	var window = get_node_or_null("CanvasLayer/Window")
	if window != null:
		var tween = create_tween().set_loops()
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

		var background = background_border
		if background == null:
			background = window.get_node_or_null("BackgroundBorder")
		if background == null:
			background = window.get_node_or_null("background_border")

		if background != null and abs(rotation_speed) > 0.001:
			# 1. Create a container that will stay static and clip the rotating child
			var clip_container = Control.new()
			clip_container.name = "RotationClipContainer"
			clip_container.clip_contents = true

			# 2. Match the container's layout to the original background's layout
			clip_container.size = background.size
			clip_container.position = background.position
			clip_container.layout_mode = background.layout_mode
			clip_container.anchors_preset = background.anchors_preset
			clip_container.anchor_left = background.anchor_left
			clip_container.anchor_top = background.anchor_top
			clip_container.anchor_right = background.anchor_right
			clip_container.anchor_bottom = background.anchor_bottom
			clip_container.offset_left = background.offset_left
			clip_container.offset_top = background.offset_top
			clip_container.offset_right = background.offset_right
			clip_container.offset_bottom = background.offset_bottom
			clip_container.grow_horizontal = background.grow_horizontal
			clip_container.grow_vertical = background.grow_vertical

			# 3. Move the background into the container
			var parent = background.get_parent()
			var index = background.get_index()
			parent.remove_child(background)
			clip_container.add_child(background)
			parent.add_child(clip_container)
			parent.move_child(clip_container, index)

			# 4. Make the background square and large enough to cover the rectangle during rotation
			var max_dim = max(clip_container.size.x, clip_container.size.y) * 1.5
			background.size = Vector2(max_dim, max_dim)
			background.position = (clip_container.size - background.size) / 2.0
			background.pivot_offset = background.size / 2.0
			background.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
			background.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_COVERED

			# 5. Use a Tween object to manage the rotation logic
			var duration = TAU / abs(rotation_speed)
			var target_rotation = TAU * sign(rotation_speed)

			var rotation_tween = background.create_tween().set_loops()
			rotation_tween.tween_method(
				func(rot): background.rotation = rot, 0.0, target_rotation, duration
			)


## Closes the menu by freeing the node.
func close() -> void:
	queue_free()
