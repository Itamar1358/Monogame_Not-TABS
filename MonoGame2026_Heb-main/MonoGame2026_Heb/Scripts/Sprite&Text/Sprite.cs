using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class Sprite : IUpdatable, IDrawable
{
    public Transform tm = new Transform();
    public Texture2D texture;
    public Spritesheet spritesheet;
    public Color color = Color.White;
    public int sortingOrder = 0;
    public SpriteEffects effects = SpriteEffects.None;

    protected Rectangle? sourceRect = null;
    public Rectangle destRect;
    
    private Vector2 origin = Vector2.Zero;


    public Sprite(string spriteName)
    {
        ChangeSprite(spriteName);
    }

    public void ChangeSprite(string spriteName)
    {
        spritesheet = SpriteManager.GetSprite(spriteName);
        texture = spritesheet.texture;
        
        // should take the rect of the selected frame
        sourceRect = spritesheet[0,0];
        UpdateDestRect();
    }

    public virtual void Start()
    {
        
    }

    private void UpdateDestRect()
    {
        destRect = GetDestRect(sourceRect);
    }

    private void UpdateOrigin()
    {
        origin = new Vector2(sourceRect.Value.Width * 0.5f, sourceRect.Value.Height * 0.5f);
    }

    public virtual void Update(GameTime gameTime)
    {
        // origin calculation must happened AFTER the source being update
        // which is occur in the Animation.update()

        UpdateOrigin();
        UpdateDestRect();
    }

    protected Rectangle GetDestRect(Rectangle? srcRect)
    {
        // take into account the scale and origin into 
        // the final result of dest rectangle
        
        if (srcRect == null) return new Rectangle();
        
        int width = (int)(srcRect.Value.Width * tm.scale.X);
        int height = (int)(srcRect.Value.Height * tm.scale.Y);

        int pos_x = (int)(tm.position.X - origin.X * tm.scale.X);
        int pos_y = (int)(tm.position.Y - origin.Y * tm.scale.Y);
        
        return new Rectangle(
            pos_x,
            pos_y,
            width,
            height
            );
    }

    public virtual void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(
            texture, 
            tm.position,
            sourceRect,
            color,
            MathHelper.ToRadians(tm.rotation),
            origin,
            tm.scale,
            effects,
            sortingOrder
        );
    }
}