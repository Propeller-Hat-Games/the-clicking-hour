shader_type canvas_item;

uniform vec2 mouse_position;
uniform float radius : hint_range(50.0, 500.0) = 150.0;
uniform float darkness : hint_range(0.0, 1.0) = 0.95;

void fragment() {
    // Calcul de la distance entre le pixel actuel (en coordonnées écran) et la souris
    float dist = distance(FRAGCOORD.xy, mouse_position);
    
    // Smoothstep pour créer un bord doux autour de la lumière
    // 0.0 à l'intérieur du cercle (transparent), 1.0 à l'extérieur (sombre)
    float alpha = smoothstep(radius, radius + 100.0, dist);
    
    // Application de la couleur noire avec l'opacité calculée
    COLOR = vec4(0.0, 0.0, 0.0, alpha * darkness);
}