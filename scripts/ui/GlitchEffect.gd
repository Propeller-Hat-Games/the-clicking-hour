class_name GlitchEffect
extends CanvasLayer

## Controls the screen glitch shader effect.

var _tween: Tween

@onready var _material: ShaderMaterial = $ColorRect.material


func _ready() -> void:
	# Start with no effect
	_material.set_shader_parameter(&"strength", 0.0)
	_material.set_shader_parameter(&"aberration", 0.0)
	_material.set_shader_parameter(&"desaturation", 0.0)


## Sets the desaturation level of the screen.
func set_desaturation(value: float) -> void:
	if _material != null:
		_material.set_shader_parameter(&"desaturation", clamp(value, 0.0, 1.0))


## Triggers a glitch spike effect.
func trigger_glitch(duration: float = 0.5, intensity: float = 0.05) -> void:
	if not SettingsManager.glitch_enabled:
		return

	if _tween != null and _tween.is_running():
		_tween.kill()

	_tween = create_tween()

	# Spike up
	_tween.parallel().tween_method(
		func(val): _material.set_shader_parameter(&"strength", val), 0.0, intensity, duration * 0.2
	)
	_tween.parallel().tween_method(
		func(val): _material.set_shader_parameter(&"aberration", val * 0.1),
		0.0,
		intensity,
		duration * 0.2
	)

	# Fade out
	_tween.tween_method(
		func(val): _material.set_shader_parameter(&"strength", val), intensity, 0.0, duration * 0.8
	)
	_tween.parallel().tween_method(
		func(val): _material.set_shader_parameter(&"aberration", val * 0.1),
		intensity,
		0.0,
		duration * 0.8
	)


## Triggers a chromatic aberration effect.
func trigger_chromatic_aberration(duration: float = 0.5, intensity: float = 0.005) -> void:
	if not SettingsManager.glitch_enabled:
		return

	if _tween != null and _tween.is_running():
		_tween.kill()

	_tween = create_tween()

	_tween.tween_method(
		func(val): _material.set_shader_parameter(&"aberration", val),
		0.0,
		intensity,
		duration * 0.2
	)
	_tween.tween_method(
		func(val): _material.set_shader_parameter(&"aberration", val),
		intensity,
		0.0,
		duration * 0.8
	)
