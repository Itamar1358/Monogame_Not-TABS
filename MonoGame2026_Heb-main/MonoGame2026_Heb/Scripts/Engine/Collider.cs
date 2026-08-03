using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class Collider : Sprite
{
    public bool IsTrigger = false;
    public bool IsEnabled { get; set; } = true;
    public int thickness = 3;
    
    public Vector2 SizeMultiplier { get; set; } = Vector2.One;


    private Action<Collider, Collider> _OnTrigger;
    private Action<Collider, Collider> _OnCollision;
    public Sprite Parent { get; set; }

    public Collider() : base("Pixel")
    {
    }
    
    public Rectangle GetBounds()
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

        // Keep the smaller collider centered on the sprite.
        return new Rectangle(
            parentBounds.Center.X - width / 2,
            parentBounds.Center.Y - height / 2,
            width,
            height);
    }

    public bool IsIntersecting(Collider other)
    {
        if (other == null)
            return false;

        if (!IsEnabled || !other.IsEnabled)
            return false;

        if (Parent == null || other.Parent == null)
            return false;
        return GetBounds().Intersects(other.GetBounds());
    }

    public void Notify(Collider other)
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

    public override void Draw(SpriteBatch _spriteBatch)
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

    public void RegisterOnTrigger(Action<Collider, Collider> action)
    {
        _OnTrigger += action;
    }

    public void RegisterOnCollision(Action<Collider, Collider> action)
    {
        _OnCollision += action;
    }
    
    public void UnregisterOnTrigger(Action<Collider, Collider> action)
    {
        _OnTrigger -= action;
    }

    public void UnregisterOnCollision(Action<Collider, Collider> action)
    {
        _OnCollision -= action;
    }
}