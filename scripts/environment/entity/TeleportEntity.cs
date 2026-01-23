using Godot;

// Entité qui se téléporte en arrière quand cliquée
// Nécessite 3 clics au total
public partial class TeleportEntity : Entity
{
    
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
        // Téléporter vers une position aléatoire dans la zone d'apparition
        var parent = GetParent();
        if (parent is SpawnArea spawnArea)
        {
            Vector2 size = spawnArea.AreaSize();
            float randomX = (float)GD.RandRange(-size.X / 2, size.X / 2);
            float randomY = (float)GD.RandRange(-size.Y / 2, size.Y / 2);
            
            Position = new Vector2(randomX, randomY);
            
            // Recalculer la direction vers la porte
            var door = GetTree().GetFirstNodeInGroup("Door") as Node2D;
            if (door != null)
            {
                walkDirection = (door.GlobalPosition - GlobalPosition).Normalized();
            }
            
             GD.Print($"TeleportEntity téléportée aléatoirement à {Position}");
        }
        
        // Effet visuel de téléportation (optionnel)
        if (sprite != null)
        {
            // Flash visuel
            Tween tween = CreateTween();
            tween.TweenProperty(sprite, "modulate:a", 0.3f, 0.1);
            tween.TweenProperty(sprite, "modulate:a", 1.0f, 0.1);
        }
    }
}
