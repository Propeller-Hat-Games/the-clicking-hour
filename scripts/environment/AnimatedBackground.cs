using Godot;
using System;

public partial class AnimatedBackground : Node2D
{
	private AnimatedSprite2D _sprite;
	private GameManager _gameManager;

	public override void _Ready()
	{
		_sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		_gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
		
		if (_sprite != null)
		{
			_sprite.Play();
		}
	}

	public override void _Process(double delta)
	{
		if (_sprite == null || _gameManager == null) return;

		string targetAnim = _gameManager.IsNightMode ? "night" : "default";
		if (_sprite.Animation != targetAnim)
		{
			_sprite.Play(targetAnim);
		}
	}
}
