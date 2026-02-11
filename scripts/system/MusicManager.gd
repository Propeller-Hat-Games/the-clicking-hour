extends AudioStreamPlayer

## Manages background music playback and playlists.

var _menu_music: AudioStream
var _normal_playlist: Array[AudioStream] = []
var _night_playlist: Array[AudioStream] = []
var _rng: RandomNumberGenerator = RandomNumberGenerator.new()

var _fade_tween: Tween
var _pause_tween: Tween

@export var _fade_duration: float = 2.0

var _music_bus_index: int
var _low_pass_effect_index: int = -1
var _low_pass_effect: AudioEffectLowPassFilter

func _ready() -> void:
	process_mode = Node.PROCESS_MODE_ALWAYS
	_setup_music_bus()

	# Menu music (static)
	_menu_music = load("res://assets/musics/background/Jet27Drink.mp3")
	
	# 🌞 Normal Playlist
	_normal_playlist.append(load("res://assets/musics/background/GroovyDrink.mp3"))
	_normal_playlist.append(load("res://assets/musics/background/SeasideDrink.mp3"))
	_normal_playlist.append(load("res://assets/musics/background/SeriousDrink.mp3"))
	
	print("[MUSIC] Normal playlist loaded: ", _normal_playlist.size(), " tracks")

	# 🌙 Night Mode Playlist
	_night_playlist.append(load("res://assets/musics/backgroundDarkMode/DrunkGroovy.mp3"))
	_night_playlist.append(load("res://assets/musics/backgroundDarkMode/DrunkSeaside.mp3"))
	_night_playlist.append(load("res://assets/musics/backgroundDarkMode/DrunkSerious.mp3"))

	print("[MUSIC] Night playlist loaded: ", _night_playlist.size(), " tracks")

func _setup_music_bus() -> void:
	var existing_bus = AudioServer.get_bus_index("Music")
	if existing_bus != -1:
		_music_bus_index = existing_bus
	else:
		_music_bus_index = AudioServer.get_bus_count()
		AudioServer.add_bus(_music_bus_index)
		AudioServer.set_bus_name(_music_bus_index, "Music")
		AudioServer.set_bus_send(_music_bus_index, "Master")
	
	bus = "Music"

	# Check if low pass already exists
	for i in range(AudioServer.get_bus_effect_count(_music_bus_index)):
		var effect = AudioServer.get_bus_effect(_music_bus_index, i)
		if effect is AudioEffectLowPassFilter:
			_low_pass_effect = effect
			_low_pass_effect_index = i
			return

	_low_pass_effect = AudioEffectLowPassFilter.new()
	_low_pass_effect.cutoff_hz = 20000
	AudioServer.add_bus_effect(_music_bus_index, _low_pass_effect)
	_low_pass_effect_index = AudioServer.get_bus_effect_count(_music_bus_index) - 1
	AudioServer.set_bus_effect_enabled(_music_bus_index, _low_pass_effect_index, false)
	
	SettingsManager.apply_music_volume()

## Toggles a muffled (low-pass) effect on the music, typically used when the game is paused.
func set_pause_effect(paused: bool) -> void:
	if _low_pass_effect == null or _low_pass_effect_index == -1:
		return

	if _pause_tween != null and _pause_tween.is_valid():
		_pause_tween.kill()

	_pause_tween = create_tween()
	var target_cutoff = 800.0 if paused else 20000.0
	var target_volume = -6.0 if paused else 0.0

	AudioServer.set_bus_effect_enabled(_music_bus_index, _low_pass_effect_index, true)

	_pause_tween.parallel().tween_property(_low_pass_effect, "cutoff_hz", target_cutoff, 0.5).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)
	_pause_tween.parallel().tween_property(self, "volume_db", target_volume, 0.5).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)

	if not paused:
		_pause_tween.finished.connect(func(): AudioServer.set_bus_effect_enabled(_music_bus_index, _low_pass_effect_index, false))

## Plays the main menu music with a fade effect.
func play_menu_music() -> void:
	await fade_to(_menu_music)
	print("[MUSIC] MENU")

## Plays a random track from the game playlist (normal or night mode) with a fade effect.
func play_game_music(is_night_mode: bool) -> void:
	var playlist = _night_playlist if is_night_mode else _normal_playlist
	var next_stream = playlist[_rng.randi() % playlist.size()]
	
	await fade_to(next_stream)
	
	if is_night_mode:
		print("[MUSIC] NIGHT")
	else:
		print("[MUSIC] NORMAL")

## Smoothly fades out the current music and stops it.
func fade_out() -> void:
	if not playing:
		return

	if _fade_tween != null and _fade_tween.is_valid():
		_fade_tween.kill()

	_fade_tween = create_tween()
	_fade_tween.tween_property(self, "volume_db", -80.0, _fade_duration).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	_fade_tween.tween_callback(func():
		stop()
		volume_db = 0 # Reset volume for next play if not using fade_to
	)

	print("[MUSIC] FADE OUT")

	await _fade_tween.finished

## Smoothly fades out the current music and fades in the new stream.
func fade_to(next_stream: AudioStream) -> void:
	if stream == next_stream and playing:
		return

	if _fade_tween != null and _fade_tween.is_valid():
		_fade_tween.kill()

	_fade_tween = create_tween()

	# Fade out
	if playing:
		_fade_tween.tween_property(self, "volume_db", -80.0, _fade_duration / 2.0).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN)
	else:
		volume_db = -80.0

	# Change stream
	_fade_tween.tween_callback(func():
		stream = next_stream
		if next_stream != null:
			play()
		else:
			stop()
	)

	# Fade in
	if next_stream != null:
		_fade_tween.tween_property(self, "volume_db", 0.0, _fade_duration / 2.0).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_OUT)

	await _fade_tween.finished
