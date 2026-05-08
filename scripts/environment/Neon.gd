class_name Neon
extends Node2D

## Controls a Neon light that flickers between On and Off states.

@export var neon_on_sprite: Sprite2D
@export var neon_off_sprite: Sprite2D
@export var light: PointLight2D


func _ready() -> void:
	while is_inside_tree():
		neon_on_sprite.visible = true
		neon_off_sprite.visible = false
		light.visible = true

		await get_tree().create_timer(randf_range(1.0, 5.0), false).timeout
		if not is_inside_tree():
			return

		neon_on_sprite.visible = false
		neon_off_sprite.visible = true
		light.visible = false

		await get_tree().create_timer(randf_range(0.5, 1.5), false).timeout
