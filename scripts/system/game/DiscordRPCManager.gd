extends Node

## Manages Discord Rich Presence integration.

var _game: GameManager


func init(p_game: GameManager) -> void:
	_game = p_game
	DiscordRPC.app_id = 1493718940147257365
	DiscordRPC.large_image = "icon"
	DiscordRPC.start_timestamp = int(Time.get_unix_time_from_system())
	set_main_menu()


func set_main_menu() -> void:
	DiscordRPC.details = "Main Menu"
	DiscordRPC.state = "Getting ready to play"
	DiscordRPC.refresh()


func set_settings_menu() -> void:
	DiscordRPC.details = "Settings"
	DiscordRPC.state = "Tweaking the experience"
	DiscordRPC.refresh()


func set_credits_menu() -> void:
	DiscordRPC.details = "Credits"
	DiscordRPC.state = "Viewing the creators"
	DiscordRPC.refresh()


func set_playing(wave: int, is_night_mode: bool) -> void:
	DiscordRPC.details = "In the Club"
	var mode_text = " (Night Mode)" if is_night_mode else ""
	DiscordRPC.state = "Wave %d%s" % [wave, mode_text]
	DiscordRPC.refresh()


func set_paused(is_paused: bool) -> void:
	if is_paused:
		DiscordRPC.details = "Paused"
	else:
		DiscordRPC.details = "At the enter of the Club"
	DiscordRPC.refresh()


func set_game_over(wave: int) -> void:
	DiscordRPC.details = "Game Over"
	DiscordRPC.state = "Survived %d waves" % wave
	DiscordRPC.refresh()
