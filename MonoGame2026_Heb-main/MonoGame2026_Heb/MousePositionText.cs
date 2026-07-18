using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGame2026_Heb;

public class MousePositionText : Text
{
    public override void Start()
    {

        tm.position = new Vector2(Game1._screenCenter.X, 50);
    }

    public override void Update(GameTime gameTime)
    {
       text = Mouse.GetState().Position.ToString();  
    }
}