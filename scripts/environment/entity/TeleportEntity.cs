using Godot;

// Entité qui se téléporte en arrière quand cliquée
// Nécessite 3 clics au total
public partial class TeleportEntity : Entity
{
    private float teleportDistance = 150f; // Distance de téléportation en pixels
    
    protected override void InitializeEntity()
    {
        clicksRemaining = 3;
    }
    
    protected override void OnClicked()
    {
        clicksRemaining--;
        GD.Print($"TeleportEntity cliquée, clics restants: {clicksRemaining}");
        
        if (clicksRemaining <= 0)
        {
            Die();
        }
        else
        {
            // Se téléporter en arrière
            Teleport();
        }
    }
    
    private void Teleport()
    {
        // Téléporter vers la gauche (en arrière)
        Vector2 teleportDirection = -walkDirection;
        Position += teleportDirection * teleportDistance;
        
        // Effet visuel de téléportation (optionnel)
        if (sprite != null)
        {
            // Flash visuel
            Tween tween = CreateTween();
            tween.TweenProperty(sprite, "modulate:a", 0.3f, 0.1);
            tween.TweenProperty(sprite, "modulate:a", 1.0f, 0.1);
        }
        
        GD.Print($"TeleportEntity téléportée en arrière de {teleportDistance} pixels");
    }
}
