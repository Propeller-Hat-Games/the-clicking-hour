class_name DiscordRPCManager
extends GameManagerInterface

## Manages Discord Rich Presence integration.
## On Android, this module is disabled as the Discord RPC addon is not available.

var _game_start_timestamp: int = 0
var _pause_start_timestamp: int = 0
var _is_paused: bool = false

## Whether Discord RPC is available on the current platform.
var _enabled: bool = false


func init(p_game: GameManager) -> void:
	super.init(p_game)
	if OS.has_feature("android"):
		return
	_enabled = true
	process_mode = Node.PROCESS_MODE_ALWAYS
	DiscordRPC.app_id = 1493718940147257365
	DiscordRPC.large_image = "icon"
	get_tree().create_timer(1.0).timeout.connect(set_main_menu)

	GameEvents.wave_started.connect(set_playing)
	GameEvents.game_over.connect(set_game_over)
	GameEvents.game_started.connect(func(): set_playing(0, false))


func _process(_delta: float) -> void:
	if not _enabled:
		return
	DiscordRPC.run_callbacks()


func set_main_menu() -> void:
	if not _enabled:
		return
	_game_start_timestamp = 0
	_is_paused = false
	DiscordRPC.details = tr(&"RPC_MAIN_MENU")
	DiscordRPC.state = ""
	DiscordRPC.start_timestamp = 0
	DiscordRPC.end_timestamp = 0
	DiscordRPC.refresh()


func set_settings_menu() -> void:
	if not _enabled:
		return
	DiscordRPC.details = tr(&"RPC_SETTINGS")
	DiscordRPC.refresh()


func set_credits_menu() -> void:
	if not _enabled:
		return
	DiscordRPC.details = tr(&"RPC_CREDITS")
	DiscordRPC.refresh()


func set_playing(wave: int, is_night_mode: bool) -> void:
	if not _enabled:
		return
	DiscordRPC.details = tr(&"RPC_GAME")
	var mode_text := " (%s)" % tr(&"RPC_NIGHT_STATUS") if is_night_mode else ""
	DiscordRPC.state = tr(&"RPC_WAVE_STATUS") % wave + mode_text

	if _game_start_timestamp == 0:
		_game_start_timestamp = int(Time.get_unix_time_from_system())

	DiscordRPC.start_timestamp = _game_start_timestamp
	DiscordRPC.end_timestamp = 0
	DiscordRPC.refresh()


func set_paused(is_paused: bool) -> void:
	if not _enabled:
		return
	if is_paused:
		if not _is_paused:
			_pause_start_timestamp = int(Time.get_unix_time_from_system())
			_is_paused = true
		DiscordRPC.details = tr(&"RPC_PAUSED")
		DiscordRPC.start_timestamp = 0
		DiscordRPC.refresh()
	else:
		if _is_paused:
			var pause_duration := int(Time.get_unix_time_from_system()) - _pause_start_timestamp
			_game_start_timestamp += pause_duration
			_is_paused = false
		set_playing(game.current_wave, game.is_night_mode)


func set_game_over(wave: int) -> void:
	if not _enabled:
		return
	DiscordRPC.details = tr(&"RPC_GAME_OVER_TITLE")
	DiscordRPC.state = tr(&"RPC_GAME_OVER_DESCRIPTION") % wave
	DiscordRPC.start_timestamp = _game_start_timestamp
	DiscordRPC.end_timestamp = int(Time.get_unix_time_from_system())
	_game_start_timestamp = 0
	_is_paused = false
	DiscordRPC.refresh()
