class_name AnimatedBackground
extends Node2D

## Handles the scrolling background animation with parallax effect.

@export var speed_back: float = 20.0
@export var speed_front: float = 40.0
@export var screen_width: float = 1152.0

var _back: Array[Sprite2D] = []
var _front: Array[Sprite2D] = []
var _sprite_night: AnimatedSprite2D = null
var _fade_tween: Tween = null

@onready var _sprite: AnimatedSprite2D = $AnimatedSprite2D


func _ready() -> void:
	# AnimatedSprite2D
	if _sprite:
		_sprite.play(&"default")

		# Create the night overlay dynamically
		_sprite_night = _sprite.duplicate() as AnimatedSprite2D
		add_child(_sprite_night)
		move_child(_sprite_night, 1)  # Draw right on top of the day sky
		_sprite_night.play(&"night")
		_sprite_night.modulate.a = 0.0

	# Background layers
	_back = _safe_get_sprites([&"Sprite2D_Back", &"Sprite2D_Back2"])
	_front = _safe_get_sprites([&"Sprite2D_Front", &"Sprite2D_Front2"])


func _process(delta: float) -> void:
	_scroll_layer(_back, speed_back, delta)
	_scroll_layer(_front, speed_front, delta)


## Scrolls a set of sprites at a specific speed, wrapping them around the screen.
func _scroll_layer(layer: Array[Sprite2D], speed: float, delta: float) -> void:
	for spr in layer:
		if spr == null:
			continue

		var pos := spr.position
		pos.x -= speed * delta

		# Horizontal Loop
		if pos.x <= -screen_width:
			pos.x += 2.0 * screen_width

		spr.position = pos


## Updates the background animation based on day/night cycle.
func update_animated_sprite(is_night_mode: bool, duration: float = 0.0) -> void:
	if _sprite_night == null:
		return

	if _fade_tween != null:
		_fade_tween.kill()
		_fade_tween = null

	var target_alpha := 1.0 if is_night_mode else 0.0

	if duration <= 0.0:
		_sprite_night.modulate.a = target_alpha
	else:
		_fade_tween = create_tween()
		(
			_fade_tween
			. tween_property(_sprite_night, "modulate:a", target_alpha, duration)
			. set_trans(Tween.TRANS_SINE)
			. set_ease(Tween.EASE_IN_OUT)
		)


## Utility method to safely get multiple Sprite2D nodes by name.
func _safe_get_sprites(node_names: Array[String]) -> Array[Sprite2D]:
	var result: Array[Sprite2D] = []
	for node_name in node_names:
		var node := get_node_or_null(node_name)
		if node is Sprite2D:
			result.append(node as Sprite2D)
	return result
