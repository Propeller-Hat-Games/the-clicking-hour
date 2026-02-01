using Godot;
using System;

public enum GlassType
{
	GrandVert,
	GrandBleu,
	GrandRouge,
	GrandJaune,
	VinRouge,
	VinJaune,
	VinVert,
	VinBleu,
	MoyenJaune,
	MoyenRouge,
	MoyenVert,
	MoyenBleu,
	PetitBleu,
	PetitVert,
	PetitJaune,
	PetitRouge,
	Special
}

public partial class Glass : Node2D {
	[Export] private Godot.Collections.Array<Sprite2D> sprites = new();
	private GlassType currentType;

	public Godot.Collections.Array<Sprite2D> GetSprites() {
		return sprites;
	}

	public void SetGlassType(GlassType type)
	{
		currentType = type;
		for (int i = 0; i < sprites.Count; i++)
		{
			if (sprites[i] != null)
			{
				sprites[i].Visible = i == ((int)type);
			}
		}
	}
	
	public GlassType GetGlassType() => currentType;

	public float AppearOffset { get; set; } = 0f;

	public void Appear()
	{
		// Logic moved to Entity.cs for frame-syncing
		AppearOffset = 120f;
	}

	public void Disappear()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate:a", 0.0f, 0.5f);
	}
}
