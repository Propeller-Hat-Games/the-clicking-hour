using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Entity that teleports to a random location in its spawn area when clicked.
/// Requires 3 clicks to defeat.
/// </summary>
public partial class TeleportEntity : Entity
{
    /// <summary>
    /// Initializes the entity with 'tp' animation prefix and 3 hearts.
    /// </summary>
    protected override void InitializeEntity()
    {
        AnimPrefix = "tp";
        Hearts = 3;
    }

    /// <summary>
    /// Handles the click event by teleporting the entity to a random location within the spawn area.
    /// </summary>
    protected override async void OnClicked()
    {
        if (CurrentState != EntityState.Walking || IsDisappearing) return;

        CurrentState = EntityState.Hiding; // Use Hiding state to disable movement/animation updates

        // Disappear animation (Jump)
        string anim = $"{AnimPrefix}_jump";
        PlaySyncedAnimation(anim, false, 0.2f);

        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        if (IsDisappearing) return;

        // Teleport to random position in spawn area
        var parent = GetParent();
        if (parent is SpawnArea spawnArea && spawnArea.Area.Shape is RectangleShape2D rectShape)
        {
            Vector2 size = rectShape.Size;
            float randomX = (float)GD.RandRange(-size.X / 2, size.X / 2);
            float randomY = (float)GD.RandRange(-size.Y / 2, size.Y / 2);

            Position = new Vector2(randomX, randomY);
        }

        // Re-appear animation (Inverse Jump)
        PlaySyncedAnimation(anim, true, 0.2f);
        await ToSignal(GetTree().CreateTimer(0.2f), SceneTreeTimer.SignalName.Timeout);
        if (IsDisappearing) return;

        CurrentState = EntityState.Walking;
    }
}