using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb.MeleeWeapons;

public class Club : MeleeWeapon
{
    // (Constructor): Initializes the club sprite.
    public Club() : base(spriteName: "Club", colliderSize: new Vector2(0.65f, 0.8f), swingDuration: 0.4f, swingAngle: 65f) { }
}