extends Node


func init(_game: GameManager) -> void:
	DiscordRPC.app_id = 1493718940147257365
	DiscordRPC.details = "In the main menu"
	DiscordRPC.large_image = "icon"
	DiscordRPC.start_timestamp = int(Time.get_unix_time_from_system())
	DiscordRPC.refresh()
