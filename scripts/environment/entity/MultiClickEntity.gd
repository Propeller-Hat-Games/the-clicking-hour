extends Entity

## Entity that requires multiple clicks to defeat (3 to 5).
## Gets stunned briefly when clicked.

var _stun_timer: float = 0.0
const STUN_DURATION: float = 0.5

func initialize_entity() -> void:
	anim_prefix = "multiclick"
	hearts = randi_range(3, 5)

func can_be_clicked() -> bool:
	return hearts > 0 and not is_disappearing

func _on_clicked() -> void:
	current_state = EntityState.STUNNED
	_stun_timer = STUN_DURATION

func process_entity(delta: float) -> void:
	if current_state == EntityState.STUNNED:
		_stun_timer -= delta
		if _stun_timer <= 0:
			current_state = EntityState.WALKING
