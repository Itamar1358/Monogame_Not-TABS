using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class Spritesheet
{
    public int columns { get; set; }
    public int rows { get; set; }
    public Texture2D texture { get; set; }

    public Rectangle this[int x, int y]
    {
        get
        {
            int width = texture.Width / columns;
            int height = texture.Height / rows;

            int pos_x = width * x;
            int pos_y = height * y;
            
            return new Rectangle(
                pos_x,
                pos_y,
                width,
                height);
        }
    }
}