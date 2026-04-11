extends Area2D

## Represents a door in the environment that entities can enter.
## Handles the visual state (Open/Closed) and detects entity entry.

signal entity_entered_door(entity: Entity)

@export var open_texture: Texture2D
@export var closed_texture: Texture2D
@export var sprite: Sprite2D


func _ready() -> void:
	add_to_group("Door")
	body_entered.connect(_on_body_entered)


## Changes the door sprite to the open texture and plays the open sound.
func open(sfx: Node) -> void:
	sprite.texture = open_texture
	sfx.play_door_open_sound()


## Changes the door sprite to the closed texture and plays the close sound.
func close(sfx: Node) -> void:
	sprite.texture = closed_texture
	sfx.play_door_close_sound()


## Callback for when a body enters the door's detection area.
func _on_body_entered(body: Node2D) -> void:
	if body is Entity and body.is_alive:
		entity_entered_door.emit(body)
