extends Node

## Manages sound effects playback.

var _random = RandomNumberGenerator.new()

# Click sounds
var _clic1: AudioStream
var _clic2: AudioStream
var _clic3: AudioStream

# Death sound
var _death1: AudioStream

# Door sounds
var _door_open: AudioStream
var _door_close: AudioStream

# Entity sounds
var _entity_dig1: AudioStream
var _entity_emergence1: AudioStream

# Damage sounds
var _take_damage1: AudioStream

# Jingle sounds
var _jingle1: AudioStream
var _jingle2: AudioStream
var _jingle3: AudioStream

# Correct glass sounds
var _correct_glass1: AudioStream
var _correct_glass2: AudioStream
var _correct_glass3: AudioStream
var _correct_glass4: AudioStream

var _sfx_bus_index: int


func _ready() -> void:
	_setup_sfx_bus()
	# Load all sounds
	_clic1 = load("res://assets/sounds/Clic1.mp3")
	_clic2 = load("res://assets/sounds/Clic2.mp3")
	_clic3 = load("res://assets/sounds/Clic3.mp3")

	_death1 = load("res://assets/sounds/Death1.mp3")

	_door_open = load("res://assets/sounds/DoorOpen.mp3")
	_door_close = load("res://assets/sounds/DoorClose.mp3")

	_entity_dig1 = load("res://assets/sounds/EntityDig1.mp3")
	_entity_emergence1 = load("res://assets/sounds/EntityEmerge1.mp3")

	_take_damage1 = load("res://assets/sounds/TakeDamageBetter.mp3")

	_jingle1 = load("res://assets/sounds/Jingle1.mp3")
	_jingle2 = load("res://assets/sounds/Jingle2.mp3")
	_jingle3 = load("res://assets/sounds/Jingle3.mp3")

	_correct_glass1 = load("res://assets/sounds/Correct1.mp3")
	_correct_glass2 = load("res://assets/sounds/Correct2.mp3")
	_correct_glass3 = load("res://assets/sounds/Correct3.mp3")
	_correct_glass4 = load("res://assets/sounds/Correct4.mp3")

	print("[SFX] Sounds loaded!")


func _setup_sfx_bus() -> void:
	_sfx_bus_index = AudioServer.get_bus_index("SFX")

	if _sfx_bus_index == -1:
		_sfx_bus_index = AudioServer.get_bus_count()
		AudioServer.add_bus(_sfx_bus_index)
		AudioServer.set_bus_name(_sfx_bus_index, "SFX")
		AudioServer.set_bus_send(_sfx_bus_index, "Master")

	SettingsManager.apply_sfx_volume()


## Internal helper to play a sound stream with current SFX volume settings.
func _play_sound(sound: AudioStream, volume_scale: float = 1.0) -> void:
	if sound == null:
		return

	var player = AudioStreamPlayer.new()
	add_child(player)
	player.stream = sound
	player.bus = "SFX"
	player.volume_db = linear_to_db(volume_scale)
	player.play()

	# Remove player when sound finishes
	player.finished.connect(func(): player.queue_free())


## Plays a random click sound effect.
func play_click_sound() -> void:
	var sounds = [_clic1, _clic2, _clic3].filter(func(s): return s != null)
	if sounds.size() > 0:
		_play_sound(sounds[_random.randi() % sounds.size()])


## Plays the entity death sound effect.
func play_death_sound() -> void:
	_play_sound(_death1)


## Plays the door opening sound effect.
func play_door_open_sound() -> void:
	_play_sound(_door_open)


## Plays the door closing sound effect.
func play_door_close_sound() -> void:
	_play_sound(_door_close)


## Plays the entity digging sound effect.
func play_entity_dig_sound() -> void:
	_play_sound(_entity_dig1)


## Plays the entity emergence sound effect.
func play_entity_emergence_sound() -> void:
	_play_sound(_entity_emergence1)


## Plays the player take damage sound effect.
func play_take_damage_sound() -> void:
	_play_sound(_take_damage1)


## Plays a random jingle sound effect for wave completion.
func play_jingle_sound() -> void:
	var sounds = [_jingle1, _jingle2, _jingle3].filter(func(s): return s != null)
	if sounds.size() > 0:
		_play_sound(sounds[_random.randi() % sounds.size()])


## Plays a random correct glass sound effect.
func play_correct_glass_sound() -> void:
	var sounds = [_correct_glass1, _correct_glass2, _correct_glass3, _correct_glass4].filter(
		func(s): return s != null
	)
	if sounds.size() > 0:
		_play_sound(sounds[_random.randi() % sounds.size()], 3.0)
