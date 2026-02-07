using Godot;

/// <summary>
/// Entity that requires multiple clicks to defeat (3 to 5).
/// Gets stunned briefly when clicked.
/// </summary>
public partial class MultiClickEntity : Entity
{
	private float StunTimer { get; set; } = 0f;
	private const float STUN_DURATION = 0.5f;

	/// <summary>
	/// Initializes the entity with a random number of hearts between 3 and 5.
	/// </summary>
	protected override void InitializeEntity()
	{
		AnimPrefix = "multiclick";
		Hearts = (int)GD.RandRange(3, 5);
	}

	/// <summary>
	/// Sets the entity state to stunned when clicked.
	/// </summary>
	protected override void OnClicked()
	{
		CurrentState = EntityState.Stunned;
		StunTimer = STUN_DURATION;
	}

	/// <summary>
	/// Processes the stun timer and returns the entity to walking state when finished.
	/// </summary>
	/// <param name="delta">Time since last frame.</param>
	protected override void ProcessEntity(double delta)
	{
		if (CurrentState == EntityState.Stunned)
		{
			StunTimer -= (float)delta;
			if (StunTimer <= 0)
			{
				CurrentState = EntityState.Walking;
			}
		}
	}
}
