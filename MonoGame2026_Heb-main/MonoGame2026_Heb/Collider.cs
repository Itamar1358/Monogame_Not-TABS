using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class Collider : Sprite
{
    public bool IsTrigger = false;
    public int thickness;

    private Action<Collider, Collider> _OnTrigger;
    private Action<Collider, Collider> _OnCollision;
    public Sprite Parent { get; set; }

    public Collider() : base("Pixel")
    {
    }

    public bool IsInterset(Collider other)
    {
        return Parent.destRect.Intersects(other.Parent.destRect);
    }

    public void Notify(Collider other)
    {
        if (IsTrigger || other.IsTrigger)
            _OnTrigger?.Invoke(this, other);
        else
            _OnCollision?.Invoke(this, other);
    }

    public override void Draw(SpriteBatch _spriteBatch)
    {
#if DEBUG
        // draw outline bounds
        
        color = Color.Green;
        thickness = 5;
        
        _spriteBatch.Draw(
            texture,
            new Rectangle(Parent.destRect.X, Parent.destRect.Y, Parent.destRect.Width, thickness), // top
            color);

        _spriteBatch.Draw(
            texture,
            new Rectangle(Parent.destRect.X, Parent.destRect.Y, thickness, Parent.destRect.Height), // left
            color);

        _spriteBatch.Draw(
            texture,
            new Rectangle(Parent.destRect.X + Parent.destRect.Width - thickness, Parent.destRect.Y, thickness, Parent.destRect.Height), // right
            color);

        _spriteBatch.Draw(
            texture,
            new Rectangle(Parent.destRect.X, Parent.destRect.Y + Parent.destRect.Height - thickness, Parent.destRect.Width, thickness), // bottom
            color);
        
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