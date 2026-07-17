class_name DiscordRPCManager
extends GameManagerInterface

## Manages Discord Rich Presence integration.

var _game_start_timestamp: int = 0
var _pause_start_timestamp: int = 0
var _is_paused: bool = false
var _discord_rpc = null


func init(p_game: GameManager) -> void:
	super.init(p_game)
	process_mode = Node.PROCESS_MODE_ALWAYS
	if !Engine.has_singleton("DiscordRPC"):
		return

	_discord_rpc = Engine.get_singleton("DiscordRPC")
	_discord_rpc.app_id = 1493718940147257365
	_discord_rpc.large_image = "icon"
	get_tree().create_timer(1.0).timeout.connect(set_main_menu)

	GameEvents.wave_started.connect(set_playing)
	GameEvents.game_over.connect(set_game_over)
	GameEvents.game_started.connect(func(): set_playing(0, false))


func _process(_delta: float) -> void:
	if _discord_rpc == null:
		return
	_discord_rpc.run_callbacks()


func set_main_menu() -> void:
	if _discord_rpc == null:
		return
	_game_start_timestamp = 0
	_is_paused = false
	_discord_rpc.details = tr(&"RPC_MAIN_MENU")
	_discord_rpc.state = ""
	_discord_rpc.start_timestamp = 0
	_discord_rpc.end_timestamp = 0
	_discord_rpc.refresh()


func set_settings_menu() -> void:
	if _discord_rpc == null:
		return
	_discord_rpc.details = tr(&"RPC_SETTINGS")
	_discord_rpc.refresh()


func set_credits_menu() -> void:
	if _discord_rpc == null:
		return
	_discord_rpc.details = tr(&"RPC_CREDITS")
	_discord_rpc.refresh()


func set_playing(wave: int, is_night_mode: bool) -> void:
	if _discord_rpc == null:
		return
	_discord_rpc.details = tr(&"RPC_GAME")
	var mode_text := " (%s)" % tr(&"RPC_NIGHT_STATUS") if is_night_mode else ""
	_discord_rpc.state = tr(&"RPC_WAVE_STATUS") % wave + mode_text

	if _game_start_timestamp == 0:
		_game_start_timestamp = int(Time.get_unix_time_from_system())

	_discord_rpc.start_timestamp = _game_start_timestamp
	_discord_rpc.end_timestamp = 0
	_discord_rpc.refresh()


func set_paused(is_paused: bool) -> void:
	if _discord_rpc == null:
		return
	if is_paused:
		if not _is_paused:
			_pause_start_timestamp = int(Time.get_unix_time_from_system())
			_is_paused = true
		_discord_rpc.details = tr(&"RPC_PAUSED")
		_discord_rpc.start_timestamp = 0
		_discord_rpc.refresh()
	else:
		if _is_paused:
			var pause_duration := int(Time.get_unix_time_from_system()) - _pause_start_timestamp
			_game_start_timestamp += pause_duration
			_is_paused = false
		set_playing(game.current_wave, game.is_night_mode)


func set_game_over(wave: int) -> void:
	if _discord_rpc == null:
		return
	_discord_rpc.details = tr(&"RPC_GAME_OVER_TITLE")
	_discord_rpc.state = tr(&"RPC_GAME_OVER_DESCRIPTION") % wave

	_game_start_timestamp = 0
	_is_paused = false
	_discord_rpc.refresh()
