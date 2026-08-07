using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class Collider : Sprite
{
    // ============ Variables & References ==================================================================================================================
    
    public bool IsTrigger = false;
    public bool IsEnabled { get; set; } = true;
    public int thickness = 3;
    public Vector2 SizeMultiplier { get; set; } = Vector2.One;
    private Action<Collider, Collider> _OnTrigger;
    private Action<Collider, Collider> _OnCollision;
    public Sprite Parent { get; set; }

    // ======================================================================================================================================================
    
    public Collider() : base("Pixel") { }
    
    public Rectangle GetBounds() // Calculates and returns the collision rectangle based on the sprite's position and scale
    {
        if (Parent == null)
            return Rectangle.Empty;

        Rectangle parentBounds = Parent.destRect;

        int width = Math.Max(
            1,
            (int)(parentBounds.Width * SizeMultiplier.X));

        int height = Math.Max(
            1,
            (int)(parentBounds.Height * SizeMultiplier.Y));
        
        return new Rectangle(
            parentBounds.Center.X - width / 2,
            parentBounds.Center.Y - height / 2,
            width,
            height);
    }

    public bool IsIntersecting(Collider other) // Checks if this collider's bounds overlap with another collider's bounds
    {
        if (other == null)
            return false;

        if (!IsEnabled || !other.IsEnabled)
            return false;

        if (Parent == null || other.Parent == null)
            return false;
        return GetBounds().Intersects(other.GetBounds());
    }

    public void Notify(Collider other) // Triggers the appropriate collision or trigger events when an intersection occurs
    {
        if (other == null)
            return;

        if (!IsEnabled || !other.IsEnabled)
            return;
        
        if (IsTrigger || other.IsTrigger)
            _OnTrigger?.Invoke(this, other);
        else
            _OnCollision?.Invoke(this, other);
    }

    public override void Draw(SpriteBatch _spriteBatch) // Renders the collider outline for debugging purposes (only when in debug mode)
    {
        if(!IsEnabled) return;
        
#if DEBUG
        Rectangle bounds = GetBounds();

        int outlineThickness = Math.Min(
            thickness,
            Math.Min(bounds.Width, bounds.Height));

        Color outlineColor = Color.Green;

        // Top
        _spriteBatch.Draw(
            texture,
            new Rectangle(
                bounds.X,
                bounds.Y,
                bounds.Width,
                outlineThickness),
            outlineColor);

        // Left
        _spriteBatch.Draw(
            texture,
            new Rectangle(
                bounds.X,
                bounds.Y,
                outlineThickness,
                bounds.Height),
            outlineColor);

        // Right
        _spriteBatch.Draw(
            texture,
            new Rectangle(
                bounds.Right - outlineThickness,
                bounds.Y,
                outlineThickness,
                bounds.Height),
            outlineColor);

        // Bottom
        _spriteBatch.Draw(
            texture,
            new Rectangle(
                bounds.X,
                bounds.Bottom - outlineThickness,
                bounds.Width,
                outlineThickness),
            outlineColor);
#endif
    }

    public void RegisterOnTrigger(Action<Collider, Collider> action) // Subscribes an action to the trigger event
    {
        _OnTrigger += action;
    }

    public void RegisterOnCollision(Action<Collider, Collider> action) // Subscribes an action to the collision event
    {
        _OnCollision += action;
    }
    
    public void UnregisterOnTrigger(Action<Collider, Collider> action) // Unsubscribes an action to the trigger event
    {
        _OnTrigger -= action;
    }

    public void UnregisterOnCollision(Action<Collider, Collider> action) // Unsubscribes an action to the collision event
    {
        _OnCollision -= action;
    }
}