
using System.Xml.Linq;

namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;
public class HealingVFX : Animation
{
    private Unit target;

    private float duration = 0.7f;
    private float timer;

    public HealingVFX() : base("HealingCircle")
    {
    }

    public void Initialize(Unit targetUnit)
    {
        target = targetUnit;

        tm.position = target.tm.position;
        tm.scale = new Vector2(1.8f, 1.8f);

        sortingOrder = target.sortingOrder - 1;

        timer = duration;

        PlayAnimation(
            isLooping: true,
            samples: 10);
    }

    public override void Update(GameTime gameTime)
    {
        if (target == null)
        {
            SceneManager.Remove(this);
            return;
        }

        float deltaTime =
            (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Follow the healed unit.
        tm.position = target.tm.position;

        sortingOrder =
            target.sortingOrder - 1;

        timer -= deltaTime;

        if (timer <= 0f)
        {
            SceneManager.Remove(this);
            return;
        }

        base.Update(gameTime);
    }
}