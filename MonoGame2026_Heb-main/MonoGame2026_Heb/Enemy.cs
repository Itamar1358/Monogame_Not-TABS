using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class Enemy : Animation
{
    public Collider collider { get; }
    
    public Enemy() : base("egret")
    {
        collider = SceneManager.Create<Collider>();
        collider.Parent = this;
        collider.IsTrigger = true;
    }
    
    public override void Start()
    {
        base.Start();
        
        tm.position = Game1._screenCenter;
        tm.position.Y -= 300;
        tm.scale = new Vector2(0.3f, 0.3f);
    }
}