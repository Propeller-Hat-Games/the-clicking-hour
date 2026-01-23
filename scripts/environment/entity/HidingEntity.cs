using Godot;

// Entité qui se cache dans le sol quand cliquée, ressort après 3 secondes
// Nécessite 3 clics au total
public partial class HidingEntity : Entity
{
	private float hideTimer = 0f;
	private const float HIDE_DURATION = 3f;
	private Vector2 originalPosition;
	private float hideDepth = 50f; // Distance vers le bas quand cachée
	
	protected override void InitializeEntity()
	{
		clicksRemaining = 3;
		originalPosition = Position;
	}
	
	protected override void OnClicked()
	{
		if (currentState == EntityState.Hiding)
			return; // Ne peut pas cliquer quand cachée
			
		clicksRemaining--;
		GD.Print($"HidingEntity cliquée, clics restants: {clicksRemaining}");
		
		if (clicksRemaining <= 0)
		{
			Die();
		}
		else
		{
			// Se cacher dans le sol
			currentState = EntityState.Hiding;
			hideTimer = 0f;
		}
	}
	
	protected override void ProcessEntity(double delta)
	{
		if (!isAlive) return;

		if (currentState == EntityState.Hiding)
		{
			hideTimer += (float)delta;
			
			// Animation de descente/montée
			if (hideTimer < 0.3f)
			{
				// Descendre dans le sol
				float progress = hideTimer / 0.3f;
				Position = new Vector2(Position.X, originalPosition.Y + hideDepth * progress);
			}
			else if (hideTimer >= HIDE_DURATION - 0.3f && hideTimer < HIDE_DURATION)
			{
				// Remonter du sol
				float progress = (hideTimer - (HIDE_DURATION - 0.3f)) / 0.3f;
				Position = new Vector2(Position.X, originalPosition.Y + hideDepth * (1f - progress));
			}
			
			if (hideTimer >= HIDE_DURATION)
			{
				// Revenir à l'état Walking
				currentState = EntityState.Walking;
				Position = new Vector2(Position.X, originalPosition.Y);
			}
		}
		else if (currentState == EntityState.Walking)
		{
			originalPosition = Position;
		}
	}
}
