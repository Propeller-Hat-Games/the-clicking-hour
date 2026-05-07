class_name Wall
extends Node2D

## Wall script that plays the attached AnimatedSprite2D and synchronizes
## the Light color with the animation frames.

@export var sprite: AnimatedSprite2D
@export var light: PointLight2D


func _ready() -> void:
	sprite.play(&"default")
	sprite.frame_changed.connect(_on_frame_changed)
	_update_light_color()


## Triggered when the sprite's animation frame changes.
func _on_frame_changed() -> void:
	_update_light_color()


## Updates the Light's color based on the current Sprite frame.
func _update_light_color() -> void:
	if light == null:
		return

	match sprite.frame:
		0:
			light.color = Color.RED
		1:
			light.color = Color.GREEN
		2:
			light.color = Color.BLUE
		3:
			light.color = Color.YELLOW
