using Godot;

// Entité simple qui nécessite 1 seul clic
public partial class SimpleEntity : Entity
{
    protected override void InitializeEntity()
    {
        clicksRemaining = 1;
    }
    
    protected override void OnClicked()
    {
        clicksRemaining--;
        
        if (clicksRemaining <= 0)
        {
            Die();
        }
    }
}
