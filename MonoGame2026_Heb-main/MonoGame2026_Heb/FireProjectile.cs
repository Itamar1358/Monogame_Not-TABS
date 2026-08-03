using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class FireProjectile : Projectile
{
    private int damage;

    public FireProjectile()
        : base("Fireball",new Vector2(0.6f, 0.5f))
    {
        RotationOffset = 180;
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
            owner: owner,
            target: target,
            startPosition: startPosition,
            movementSpeed: movementSpeed,
            projectileScale: 0.9f);
    }

    protected override void ApplyEffect(Unit hitUnit)
    {
        hitUnit.TakeDamage(damage);
        Console.WriteLine($"wizard hit {hitUnit} with {damage} damage");
    }
}