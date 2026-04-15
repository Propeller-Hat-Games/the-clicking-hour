extends Node

## Manages Discord Rich Presence integration.

var _game: GameManager
var _game_start_timestamp: int = 0
var _pause_start_timestamp: int = 0
var _is_paused: bool = false


func init(p_game: GameManager) -> void:
	_game = p_game
	process_mode = Node.PROCESS_MODE_ALWAYS
	DiscordRPC.app_id = 1493718940147257365
	DiscordRPC.large_image = "icon"
	get_tree().create_timer(1.0).timeout.connect(set_main_menu)


func _process(_delta: float) -> void:
	DiscordRPC.run_callbacks()


func set_main_menu() -> void:
	_game_start_timestamp = 0
	_is_paused = false
	DiscordRPC.details = "In the main menu"
	DiscordRPC.state = ""
	DiscordRPC.start_timestamp = 0
	DiscordRPC.end_timestamp = 0
	DiscordRPC.refresh()


func set_settings_menu() -> void:
	DiscordRPC.details = "In the settings"
	DiscordRPC.refresh()


func set_credits_menu() -> void:
	DiscordRPC.details = "In the credits"
	DiscordRPC.refresh()


func set_playing(wave: int, is_night_mode: bool) -> void:
	DiscordRPC.details = "In game"
	var mode_text = " (Night Mode)" if is_night_mode else ""
	DiscordRPC.state = "Wave %d%s" % [wave, mode_text]

	if _game_start_timestamp == 0:
		_game_start_timestamp = int(Time.get_unix_time_from_system())

	DiscordRPC.start_timestamp = _game_start_timestamp
	DiscordRPC.end_timestamp = 0
	DiscordRPC.refresh()


func set_paused(is_paused: bool) -> void:
	if is_paused:
		if not _is_paused:
			_pause_start_timestamp = int(Time.get_unix_time_from_system())
			_is_paused = true
		DiscordRPC.details = "Paused"
		DiscordRPC.start_timestamp = 0
		DiscordRPC.refresh()
	else:
		if _is_paused:
			var pause_duration = int(Time.get_unix_time_from_system()) - _pause_start_timestamp
			_game_start_timestamp += pause_duration
			_is_paused = false
		set_playing(_game.current_wave, _game.is_night_mode)


func set_game_over(wave: int) -> void:
	DiscordRPC.details = "Game Over"
	DiscordRPC.state = "Survived %d waves" % wave
	DiscordRPC.start_timestamp = _game_start_timestamp
	DiscordRPC.end_timestamp = int(Time.get_unix_time_from_system())
	_game_start_timestamp = 0
	_is_paused = false
	DiscordRPC.refresh()
