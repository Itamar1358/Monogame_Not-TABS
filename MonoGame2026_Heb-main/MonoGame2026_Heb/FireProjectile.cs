using System;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class FireProjectile : Projectile
{
    private int damage;

    public FireProjectile()
        : base("FireProjectile")
    {
    }

    public void InitializeFireProjectile(
        Unit owner,
        Unit target,
        Vector2 startPosition,
        float movementSpeed,
        int projectileDamage)
    {
        damage = Math.Max(0, projectileDamage);

        InitializeProjectile(
            owner,
            target,
            startPosition,
            movementSpeed);
    }

    protected override void ApplyEffect(Unit hitUnit)
    {
        hitUnit.TakeDamage(damage);
    }
}