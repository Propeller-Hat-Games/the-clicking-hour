shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float strength : hint_range(0.0, 0.1) = 0.05;
uniform float speed : hint_range(0.0, 10.0) = 5.0;
uniform float aberration : hint_range(0.0, 0.1) = 0.01;
uniform float frequency : hint_range(0.0, 100.0) = 10.0;

float random(float co) {
    return fract(sin(co * 12.9898) * 43758.5453);
}

void fragment() {
    vec2 uv = SCREEN_UV;
    
    float time = floor(TIME * speed);
    float glitch = step(1.0 - strength, random(time + floor(uv.y * frequency)));
    
    if (glitch > 0.0) {
        uv.x += (random(time) - 0.5) * strength;
    }
    
    vec4 color;
    color.r = texture(screen_texture, uv + vec2(aberration * glitch, 0.0)).r;
    color.g = texture(screen_texture, uv).g;
    color.b = texture(screen_texture, uv - vec2(aberration * glitch, 0.0)).b;
    color.a = 1.0;
    
    // Add a slight darkening during glitch
    color.rgb *= (1.0 - glitch * 0.2);
    
    COLOR = color;
}
