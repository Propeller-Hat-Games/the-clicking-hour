class_name GameManagerInterface
extends Node

## Base class for game managers, providing a reference to the main GameManager.

var game: GameManager


func init(p_game: GameManager) -> void:
	game = p_game
