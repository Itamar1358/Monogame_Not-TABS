using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb.MeleeWeapons;

public class Sword : MeleeWeapon
{
    // (Constructor): Initializes the sword sprite
    public Sword() : base(spriteName: "Sword", colliderSize: new Vector2(0.35f, 0.8f), swingDuration: 0.3f, swingAngle: 75f) { }
}