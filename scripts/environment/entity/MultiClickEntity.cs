using Godot;

// Entité qui nécessite entre 3 et 5 clics
public partial class MultiClickEntity : Entity
{
	protected override void InitializeEntity()
	{
		animPrefix = "normal";
		clicksRemaining = (int)GD.RandRange(3, 5);
		GD.Print($"MultiClickEntity créée avec {clicksRemaining} clics requis");
	}
	
	protected override void OnClicked()
	{
		clicksRemaining--;
		GD.Print($"MultiClickEntity cliquée, clics restants: {clicksRemaining}");
		
		if (clicksRemaining <= 0)
		{
			Die();
		}
		else
		{
			SfxManager.Instance?.PlayClickSound();
		}
	}
}
