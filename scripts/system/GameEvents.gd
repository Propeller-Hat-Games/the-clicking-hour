extends Node

## Global Signal Bus for decoupling game systems.

# Wave events
@warning_ignore("unused_signal")
signal wave_started(number: int, is_night_mode: bool)
@warning_ignore("unused_signal")
signal wave_finished(number: int)

# Game state events
@warning_ignore("unused_signal")
signal game_started
@warning_ignore("unused_signal")
signal game_over(waves_survived: int)

# Interaction events
@warning_ignore("unused_signal")
signal entity_clicked(entity: Entity)
@warning_ignore("unused_signal")
signal glass_delivered(type: String, was_correct: bool)
