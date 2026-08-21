shader_type canvas_item;

uniform float flash_value : hint_range(0.0, 1.0) = 0.0;
uniform vec4 flash_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);

void fragment() {
	vec4 texture_color = texture(TEXTURE, UV);
	COLOR = vec4(mix(texture_color.rgb, flash_color.rgb, flash_value), texture_color.a) * COLOR;
}
