using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager
{
    private Godot.Collections.Dictionary<string, Sprite2D> _everySprites = new();

    /// <summary>
    /// Loads all glass sprites from the Glass node into a dictionary.
    /// </summary>
    public void LoadGlass()
    {
        Glass glass = GetNodeOrNull<Glass>("Glass");
        if (glass != null)
        {
            _everySprites = glass.GetSprites();
        }

        GD.Print("[GLASS] Loaded sprites:");
        foreach (var key in _everySprites.Keys)
        {
            GD.Print($"        - {key}");
        }
    }

    /// <summary>
    /// Returns a random glass type key from the loaded sprites.
    /// </summary>
    /// <returns>A string representing a random glass type.</returns>
    public string RandomGlassType()
    {
        var keys = new List<string>(_everySprites.Keys);
        if (keys.Count == 0) return "";
        return keys[Rng.Next(keys.Count)];
    }

    /// <summary>
    /// Gets the Sprite2D template for a specific glass type.
    /// </summary>
    /// <param name="type">The glass type key.</param>
    /// <returns>The corresponding Sprite2D, or null if not found.</returns>
    public Sprite2D GetGlassSprite(string type)
    {
        return _everySprites.ContainsKey(type) ? _everySprites[type] : null;
    }

    /// <summary>
    /// Returns a list of Sprite2D templates for the given glass type keys.
    /// </summary>
    /// <param name="keys">List of glass type keys.</param>
    /// <returns>A list of Sprite2D templates.</returns>
    public List<Sprite2D> GetGlassSprites(List<string> keys)
    {
        List<Sprite2D> res = new List<Sprite2D>();
        foreach (var key in keys)
        {
            if (_everySprites.ContainsKey(key))
            {
                res.Add(_everySprites[key]);
            }
        }
        return res;
    }
}
