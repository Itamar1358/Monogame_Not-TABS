using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class Sprite : IUpdatable, IDrawable
{
    // ============ Variables & References ==================================================================================================================
    
    public Transform tm = new Transform();
    public Texture2D texture;
    public Spritesheet spritesheet;
    public Color color = Color.White;
    public int sortingOrder = 0;
    public SpriteEffects effects = SpriteEffects.None;
    protected Rectangle? sourceRect = null;
    public Rectangle destRect;
    private Vector2 origin = Vector2.Zero;

    // ======================================================================================================================================================

    public Sprite(string spriteName) // (Constructor): Initializes the sprite with a texture identifier
    {
        ChangeSprite(spriteName);
    }

    public void ChangeSprite(string spriteName) // Updates the sprite texture identifier
    {
        spritesheet = SpriteManager.GetSprite(spriteName);
        texture = spritesheet.texture;
        sourceRect = spritesheet[0,0];
        UpdateDestRect();
    }

    public virtual void Start() // Initializes standard sprite components
    { }

    private void UpdateDestRect() // Updates the rendering destination rectangle based on the current transform scale and position
    {
        destRect = GetDestRect(sourceRect);
    }

    private void UpdateOrigin() // Updates the pivot point of the sprite based on its texture dimensions
    {
        origin = new Vector2(sourceRect.Value.Width * 0.5f, sourceRect.Value.Height * 0.5f);
    }

    public virtual void Update(GameTime gameTime) // Recalculates the rendering properties each frame
    {
        UpdateOrigin();
        UpdateDestRect();
    }

    protected Rectangle GetDestRect(Rectangle? srcRect) // Helper method to retrieve the destination rectangle
    {
        if (srcRect == null) return new Rectangle();
        
        int width = (int)(srcRect.Value.Width * tm.scale.X);
        int height = (int)(srcRect.Value.Height * tm.scale.Y);
        int pos_x = (int)(tm.position.X - origin.X * tm.scale.X);
        int pos_y = (int)(tm.position.Y - origin.Y * tm.scale.Y);
        
        return new Rectangle(pos_x, pos_y, width, height);
    }

    public virtual void Draw(SpriteBatch spriteBatch) // Renders the static texture to the screen
    {
        spriteBatch.Draw(texture, tm.position, sourceRect, color, MathHelper.ToRadians(tm.rotation), origin, tm.scale, effects, 0f);
    }
}