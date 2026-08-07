using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class Text : IUpdatable, IDrawable
{
    // ============ Variables & References ==================================================================================================================
    
    public Transform tm = new Transform();
    public SpriteFont font;
    public Color color = Color.White;
    public int sortingOrder = 0;
    public SpriteEffects effects = SpriteEffects.None;
    public string text = string.Empty;
    
    // ======================================================================================================================================================
    
    public virtual void Start() //  Initializes the text object
    { }

    public virtual void Update(GameTime gameTime) // Processes logic for the text object
    { }

    public void Draw(SpriteBatch spriteBatch) // Renders a string of text to the screen using a specified font
    {
        Vector2 textCenter = font.MeasureString(text) * 0.5f;
        spriteBatch.DrawString(font, text, tm.position, color, MathHelper.ToRadians(tm.rotation), textCenter, tm.scale, effects, sortingOrder);
    }
}