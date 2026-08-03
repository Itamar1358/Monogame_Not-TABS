using System;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class FireProjectile : Projectile
{
    private int damage;

    public FireProjectile()
        : base("Fireball")
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
            GetProjectileSpawnPosition(),
            movementSpeed,
            projectileDamage
            );
    }

    protected override void ApplyEffect(Unit hitUnit)
    {
        hitUnit.TakeDamage(damage);
    }
    private Vector2 GetProjectileSpawnPosition()
    {
        return tm.position;
    }
}