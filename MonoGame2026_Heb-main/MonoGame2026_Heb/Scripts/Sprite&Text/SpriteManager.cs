using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb.Content;

public class SpriteManager
{
    // ============ Variables & References ==================================================================================================================
    
    static Dictionary<string, Spritesheet> sprites = new Dictionary<string, Spritesheet>();

    // ======================================================================================================================================================
    
    public static void AddSprite(string spriteName, string fileName, int columns = 1, int rows = 1) // Loads a sprite or spritesheet into memory
    {
        if (!sprites.ContainsKey(spriteName))
        {
            sprites[spriteName] = new Spritesheet();
            sprites[spriteName].texture = ResourcesManager<Texture2D>.LoadResource(spriteName, fileName);
            sprites[spriteName].columns = columns;
            sprites[spriteName].rows = rows;
        }
    }

    public static Spritesheet GetSprite(string spriteName) // Retrieves a loaded sprite by its string identifier
    {
        if (sprites.ContainsKey(spriteName)) return sprites[spriteName];
        return null;
    }
}