using Godot;
using System;

/// <summary>
/// Base class for menus, providing common functionality like floating window animation.
/// </summary>
public partial class GeneralMenu : Control
{
    [Export] public TextureRect BackgroundBorder { get; set; }
    [Export] public float RotationSpeed { get; set; } = 3.0f;

    /// <summary>
    /// Initializes the menu and starts a floating animation for the window and rotation for the background.
    /// </summary>
    public override void _Ready()
    {
        var window = GetNodeOrNull<Control>("CanvasLayer/Window");
        if (window != null)
        {
            var tween = CreateTween().SetLoops();
            tween.TweenProperty(window, "position:y", window.Position.Y - 20f, 2.0f)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.InOut);
            tween.TweenProperty(window, "position:y", window.Position.Y, 2.0f)
                 .SetTrans(Tween.TransitionType.Sine)
                 .SetEase(Tween.EaseType.InOut);

            var background = BackgroundBorder;
            if (background == null)
            {
                background = window.GetNodeOrNull<TextureRect>("BackgroundBorder");
            }

            if (background != null && Mathf.Abs(RotationSpeed) > 0.001f)
            {
                // 1. Create a container that will stay static and clip the rotating child
                Control clipContainer = new Control();
                clipContainer.Name = "RotationClipContainer";
                clipContainer.ClipContents = true;
                
                // 2. Match the container's layout to the original background's layout
                clipContainer.Size = background.Size;
                clipContainer.Position = background.Position;
                clipContainer.LayoutMode = background.LayoutMode;
                clipContainer.AnchorsPreset = (int)background.AnchorsPreset;
                clipContainer.AnchorLeft = background.AnchorLeft;
                clipContainer.AnchorTop = background.AnchorTop;
                clipContainer.AnchorRight = background.AnchorRight;
                clipContainer.AnchorBottom = background.AnchorBottom;
                clipContainer.OffsetLeft = background.OffsetLeft;
                clipContainer.OffsetTop = background.OffsetTop;
                clipContainer.OffsetRight = background.OffsetRight;
                clipContainer.OffsetBottom = background.OffsetBottom;
                clipContainer.GrowHorizontal = background.GrowHorizontal;
                clipContainer.GrowVertical = background.GrowVertical;

                // 3. Move the background into the container
                Node parent = background.GetParent();
                int index = background.GetIndex();
                parent.RemoveChild(background);
                clipContainer.AddChild(background);
                parent.AddChild(clipContainer);
                parent.MoveChild(clipContainer, index);

                // 4. Make the background square and large enough to cover the rectangle during rotation
                float maxDim = Mathf.Max(clipContainer.Size.X, clipContainer.Size.Y) * 1.5f;
                background.Size = new Vector2(maxDim, maxDim);
                background.Position = (clipContainer.Size - background.Size) / 2f; 
                background.PivotOffset = background.Size / 2;
                background.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                background.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;

                // 5. Use a Tween object to manage the rotation logic
                // This replaces the _Process loop with a dedicated Godot animation object
                float duration = Mathf.Tau / Mathf.Abs(RotationSpeed);
                float targetRotation = Mathf.Tau * Mathf.Sign(RotationSpeed);

                var rotationTween = background.CreateTween().SetLoops();
                rotationTween.TweenMethod(Callable.From<float>(rot => background.Rotation = rot), 
                    0f, targetRotation, duration);
            }
        }
    }

    /// <summary>
    /// Closes the menu by freeing the node.
    /// </summary>
    public void Close()
    {
        QueueFree();
    }
}