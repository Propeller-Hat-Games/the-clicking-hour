using Godot;
using System;

public partial class AnimatedBackground : Node2D
{
	[Export] public float SpeedBack = 20f;
	[Export] public float SpeedFront = 40f;
	[Export] public float ScreenWidth = 1152f;

	private Sprite2D[] _back;
	private Sprite2D[] _front;

	private AnimatedSprite2D _sprite;
	private GameManager _gameManager;

	public override void _Ready()
	{
		// AnimatedSprite2D
		_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (_sprite != null)
			_sprite.Play();

		// GameManager
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");

		// Layers de fond
		_back = SafeGetSprites("Sprite2D_Back", "Sprite2D_Back2");
		_front = SafeGetSprites("Sprite2D_Front", "Sprite2D_Front2");
	}

	public override void _Process(double delta)
	{
		ScrollLayer(_back, SpeedBack, delta);
		ScrollLayer(_front, SpeedFront, delta);

		UpdateAnimatedSprite();
	}

	private void ScrollLayer(Sprite2D[] layer, float speed, double delta)
	{
		if (layer == null) return;

		foreach (var sprite in layer)
		{
			if (sprite == null) continue; // <- protection contre null

			Vector2 pos = sprite.Position;
			pos.X -= speed * (float)delta;

			// Loop horizontal
			if (pos.X <= -ScreenWidth)
				pos.X += 2 * ScreenWidth;

			sprite.Position = pos;
		}
	}

	private void UpdateAnimatedSprite()
	{
		if (_sprite == null || _gameManager == null) return;

		string targetAnim = _gameManager.IsNightMode ? "night" : "default";
		if (_sprite.Animation != targetAnim)
			_sprite.Play(targetAnim);
	}

	// Méthode utilitaire pour récupérer les sprites sans risquer de null
	private Sprite2D[] SafeGetSprites(params string[] nodeNames)
	{
		var list = new System.Collections.Generic.List<Sprite2D>();
		foreach (var name in nodeNames)
		{
			var node = GetNodeOrNull<Sprite2D>(name);
			if (node != null)
				list.Add(node);
		}
		return list.Count > 0 ? list.ToArray() : null;
	}
}
