shader_type canvas_item;

uniform float vignette_intensity = 0.4;
uniform float vignette_opacity : hint_range(0.0, 1.0) = 0.5;

// Scanlines
uniform float scanline_count : hint_range(0, 1080) = 50.0;
uniform float scanline_opacity : hint_range(0.0, 1.0) = 0.3; // Faible opacité pour que ce soit "léger"

// Noise / Grain
uniform float noise_opacity : hint_range(0.0, 1.0) = 0.1; // Très léger
uniform float noise_speed = 5.0;

// Chromatic Aberration
uniform float aberration_amount : hint_range(0.0, 10.0) = 0.5; // Décalage léger

uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;

float random(vec2 uv) {
    return fract(sin(dot(uv, vec2(12.9898, 78.233))) * 43758.5453123);
}

void fragment() {
    vec2 uv = SCREEN_UV;
    
    // Chromatic Aberration (décalage des canaux rouge et bleu)
    float shift = aberration_amount * 0.001;
    vec4 tex = texture(SCREEN_TEXTURE, uv);
    vec4 tex_r = texture(SCREEN_TEXTURE, uv - vec2(shift, 0.0));
    vec4 tex_b = texture(SCREEN_TEXTURE, uv + vec2(shift, 0.0));
    
    vec4 color = vec4(tex_r.r, tex.g, tex_b.b, tex.a);
    
    // Scanlines (lignes horizontales)
    float scanline = sin(uv.y * scanline_count * 3.14159 + TIME * 10.0);
    scanline = (scanline + 1.0) * 0.5; // Normaliser entre 0 et 1
    color.rgb = mix(color.rgb, color.rgb * 0.9, scanline * scanline_opacity);
    
    // Noise (grain)
    float noise = random(uv + mod(TIME * noise_speed, 100.0));
    color.rgb = mix(color.rgb, vec3(noise), noise_opacity);
    
    // Vignette (coins sombres)
    vec2 vignette_uv = uv * (1.0 - uv.yx);
    float vignette = vignette_uv.x * vignette_uv.y * 15.0;
    vignette = pow(vignette, vignette_intensity);
    vignette = clamp(vignette, 0.0, 1.0);
    color.rgb = mix(color.rgb, color.rgb * vignette, vignette_opacity);
    
    COLOR = color;
}
