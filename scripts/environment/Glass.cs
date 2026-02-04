using Godot;
using System;
using Godot.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages a collection of glass sprites.
/// </summary>
public partial class Glass : Node2D
{
    [Export]
    public Godot.Collections.Dictionary<string, Sprite2D> Sprites { get; set; } = new();

    /// <summary>
    /// Returns the dictionary of glass sprites.
    /// </summary>
    public Godot.Collections.Dictionary<string, Sprite2D> GetSprites()
    {
        return Sprites;
    }

    /// <summary>
    /// Returns a random glass key from the available sprites.
    /// </summary>
    public string GetRandomGlass()
    {
        var keys = new List<string>(Sprites.Keys);
        if (keys.Count == 0) return null;
        int randomIndex = GD.RandRange(0, keys.Count - 1);
        return keys[randomIndex];
    }
}
