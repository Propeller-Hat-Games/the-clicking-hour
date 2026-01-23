using Godot;
using System;

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
		animPrefix = "dig";
	}
	
	protected override void OnClicked()
	{
		if (currentState == EntityState.Hiding || spawnTimer < spawnDelay)
			return; // Ne peut pas cliquer quand cachée ou en train d'apparaître
			
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
			
			if (sprite != null)
			{
				sprite.Play($"{animPrefix}_jump");
			}
		}
	}
	
	protected override void UpdateAnimation()
	{
		if (sprite == null) return;
		
		// L'animation d'apparition initiale est gérée par la classe de base Entity.cs
		if (spawnTimer < spawnDelay)
		{
			base.UpdateAnimation();
			return;
		}

		if (currentState == EntityState.Walking)
		{
			base.UpdateAnimation();
		}
		else if (currentState == EntityState.Hiding)
		{
			string animName = $"{animPrefix}_jump";
			if (sprite.SpriteFrames.HasAnimation(animName))
			{
				if (sprite.Animation != animName) sprite.Play(animName);

				// Animation de descente/montée via les frames
				if (hideTimer < 0.3f)
				{
					// Descendre : jouer l'animation normalement
					float progress = hideTimer / 0.3f;
					int frameCount = sprite.SpriteFrames.GetFrameCount(animName);
					sprite.Frame = (int)(progress * (frameCount - 1));
					sprite.Pause();
				}
				else if (hideTimer >= HIDE_DURATION - 0.3f && hideTimer < HIDE_DURATION)
				{
					// Remonter : jouer l'animation à l'envers
					float progress = (hideTimer - (HIDE_DURATION - 0.3f)) / 0.3f;
					int frameCount = sprite.SpriteFrames.GetFrameCount(animName);
					sprite.Frame = (int)((1f - progress) * (frameCount - 1));
					sprite.Pause();
				}
				else
				{
					// Reste caché à la dernière frame
					int frameCount = sprite.SpriteFrames.GetFrameCount(animName);
					sprite.Frame = frameCount - 1;
					sprite.Pause();
				}
				UpdateGlassPosition();
			}
		}
	}

	protected override void ProcessEntity(double delta)
	{
		if (!isAlive || spawnTimer < spawnDelay) return;

		if (currentState == EntityState.Hiding)
		{
			hideTimer += (float)delta;
			
			if (hideTimer >= HIDE_DURATION)
			{
				// Revenir à l'état Walking
				currentState = EntityState.Walking;
			}
		}
	}
}
