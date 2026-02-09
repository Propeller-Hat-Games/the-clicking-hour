extends Sprite2D

## Displays the current required glass orders.

@export var left_slot: Sprite2D
@export var middle_slot: Sprite2D
@export var right_slot: Sprite2D
@export var left_nb: Label
@export var middle_nb: Label
@export var right_nb: Label
@export var light: PointLight2D

var _amount_left: int = 0
var _amount_middle: int = 0
var _amount_right: int = 0

func _ready() -> void:
	if light != null:
		light.add_to_group("Lights")

	# Initial state: hide all slots and labels
	var slots = [left_slot, middle_slot, right_slot]
	var labels = [left_nb, middle_nb, right_nb]
	for slot in slots:
		if slot != null: slot.visible = false
	for label in labels:
		if label != null: label.visible = false

	# Setup floating animation with Tween
	var tween = create_tween().set_loops()
	tween.tween_property(self, "position:y", position.y - 5.0, 2.0).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)
	tween.tween_property(self, "position:y", position.y + 5.0, 2.0).set_trans(Tween.TRANS_SINE).set_ease(Tween.EASE_IN_OUT)

## Performs a flickering flash animation on a canvas item.
func _flash(item: CanvasItem, appearing: bool) -> void:
	if item == null or not is_instance_valid(item):
		return

	var opacities: Array[float]
	if appearing:
		opacities = [0.0, 0.25, 0.0, 0.5, 0.0, 0.75, 0.0, 1.0]
		item.visible = true
	else:
		opacities = [1.0, 0.0, 0.75, 0.0, 0.5, 0.0, 0.25, 0.0]

	for opacity in opacities:
		if not is_instance_valid(item):
			return
		item.modulate.a = opacity
		
		await get_tree().create_timer(0.05).timeout
		if not is_inside_tree():
			return

	if not is_instance_valid(item):
		return
	if not appearing:
		item.visible = false
	else:
		item.modulate.a = 1.0

## Updates the board slots with new glass textures and counts.
func update_board(slot_count: int, sprites: Array[Sprite2D], counts: Array[int]) -> void:
	var slots = [left_slot, middle_slot, right_slot]
	var labels = [left_nb, middle_nb, right_nb]
	var current_amounts = [_amount_left, _amount_middle, _amount_right]

	for i in range(slots.size()):
		if i < slot_count:
			# Ensure slots and sprites are valid before accessing
			if slots[i] != null and i < sprites.size() and sprites[i] != null:
				slots[i].texture = sprites[i].texture
				
				# Flash logic based on state change
				if current_amounts[i] == 0 and counts[i] > 0:
					_flash(slots[i], true)
				elif current_amounts[i] > 0 and counts[i] == 0:
					_flash(slots[i], false)

			# Ensure labels are valid before accessing
			if labels[i] != null and i < counts.size():
				labels[i].text = str(counts[i])

				if current_amounts[i] == 0 and counts[i] > 0:
					_flash(labels[i], true)
				elif current_amounts[i] > 0 and counts[i] == 0:
					_flash(labels[i], false)
		else:
			# Hide unused slots and labels
			if slots[i] != null: slots[i].visible = false
			if labels[i] != null: labels[i].visible = false

	_amount_left = counts[0] if slot_count > 0 else 0
	_amount_middle = counts[1] if slot_count > 1 else 0
	_amount_right = counts[2] if slot_count > 2 else 0
