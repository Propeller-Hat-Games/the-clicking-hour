using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Entity that hides in the ground when clicked and re-emerges after a duration.
/// Requires multiple clicks to defeat.
/// </summary>
public partial class HidingEntity : Entity
{
    private const float HIDE_DURATION = 3f;
    private const float DIG_ANIM_DURATION = 0.3f;

    /// <summary>
    /// Initializes the entity with 'dig' animation prefix and 3 hearts.
    /// </summary>
    protected override void InitializeEntity()
    {
        AnimPrefix = "dig";
        Hearts = 3;
    }

    /// <summary>
    /// Handles the click event by hiding the entity in the ground and re-emerging after a delay.
    /// </summary>
    protected override async void OnClicked()
    {
        if (CurrentState == EntityState.Hiding || IsDisappearing)
            return; // Cannot click while hidden or appearing

        CurrentState = EntityState.Hiding;

        // Hide in ground (Jump animation played normally = go down)
        string animName = $"{AnimPrefix}_jump";
        PlaySyncedAnimation(animName, false, DIG_ANIM_DURATION);

        // Animate glass down
        if (glass != null)
        {
            var tween = CreateTween();
            tween.TweenProperty(glass, "position", _glassInitialPos + new Vector2(0, 75), DIG_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.In);
        }

        await ToSignal(GetTree().CreateTimer(DIG_ANIM_DURATION), SceneTreeTimer.SignalName.Timeout);
        if (!IsInsideTree() || IsDisappearing) return;

        // Stay hidden
        await ToSignal(GetTree().CreateTimer(HIDE_DURATION - (2 * DIG_ANIM_DURATION)), SceneTreeTimer.SignalName.Timeout);
        if (!IsInsideTree() || IsDisappearing) return;

        // Emerge (Jump animation played backwards = go up)
        GetNode<SfxManager>("/root/SfxManager").PlayEntityEmergenceSound();
        PlaySyncedAnimation(animName, true, DIG_ANIM_DURATION);

        // Animate glass up
        if (glass != null)
        {
            var tween = CreateTween();
            tween.TweenProperty(glass, "position", _glassInitialPos, DIG_ANIM_DURATION)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.Out);
        }

        await ToSignal(GetTree().CreateTimer(DIG_ANIM_DURATION), SceneTreeTimer.SignalName.Timeout);
        if (!IsInsideTree() || IsDisappearing) return;

        CurrentState = EntityState.Walking;
    }

    /// <summary>
    /// Updates the entity's animation, preventing base updates while hidden.
    /// </summary>
    protected override void UpdateAnimation()
    {
        // If hidden, prevent base UpdateAnimation from interfering
        if (CurrentState != EntityState.Hiding)
        {
            base.UpdateAnimation();
        }
    }
}