using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager
{
    [Export]
    private Board board;
    private List<string> _requiredGlassTypes = new List<string>();
    private List<int> _requiredGlassCounts = new List<int>();

    /// <summary>
    /// Initializes the conditions manager and loads glass sprites.
    /// </summary>
    public void LoadConditionsManager()
    {
        if (board == null)
        {
            GD.PrintErr("[CONDITIONS] Board export is not assigned in GameManager!");
            return;
        }

        board.ZIndex = 100;

        LoadGlass();
    }

    /// <summary>
    /// Updates the UI board with current required glass types and counts.
    /// </summary>
    public void UpdateBoard()
    {
        if (board == null) return;
        board.UpdateBoard(_requiredGlassTypes.Count, GetGlassSprites(_requiredGlassTypes), _requiredGlassCounts);
    }

    /// <summary>
    /// Generates new glass collection conditions for the current wave.
    /// </summary>
    public void GenerateConditions()
    {
        _requiredGlassTypes.Clear();
        _requiredGlassCounts.Clear();

        int glassTypeCount = 1 + (int) Math.Min(2, CurrentWave / 3);
        int glassCount = 1 + CurrentWave;

        for (int i = 0; i < glassTypeCount; i++)
        {
            string newType;
            do
            {
                newType = RandomGlassType();
            } while (_requiredGlassTypes.Contains(newType));

            _requiredGlassTypes.Add(newType);
            _requiredGlassCounts.Add(1);
        }

        for (int i = 0; i < glassCount; i++)
        {
            int index = Rng.Next(_requiredGlassTypes.Count);
            _requiredGlassCounts[index]++;
        }

        GD.Print("[CONDITIONS] Generated conditions:");
        for (int i = 0; i < glassTypeCount; i++)
        {
            GD.Print($"             - {_requiredGlassTypes[i]} : {_requiredGlassCounts[i]}");
        }

        UpdateBoard();
        if (board != null) board.Visible = true;
    }

    /// <summary>
    /// Attempts to count a glass type towards the current conditions.
    /// </summary>
    /// <param name="type">The type of glass to enter.</param>
    /// <returns>True if the glass was required and counted, false otherwise.</returns>
    public bool TryEnterGlass(string type)
    {
        int index = _requiredGlassTypes.IndexOf(type);
        if (index == -1) return false;
        if (_requiredGlassCounts[index] > 0)
        {
            _requiredGlassCounts[index]--;
            SFX.PlayCorrectGlassSound();
            UpdateBoard();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if all current wave conditions have been met.
    /// </summary>
    /// <returns>True if all required glasses have been collected.</returns>
    public bool IsConditionsDone()
    {
        foreach (var count in _requiredGlassCounts)
        {
            if (count > 0) return false;
        }
        return true;
    }
}
