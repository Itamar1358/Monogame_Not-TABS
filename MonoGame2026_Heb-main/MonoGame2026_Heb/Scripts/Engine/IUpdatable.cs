using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public interface IUpdatable
{
    void Start();
    void Update(GameTime gameTime);
}