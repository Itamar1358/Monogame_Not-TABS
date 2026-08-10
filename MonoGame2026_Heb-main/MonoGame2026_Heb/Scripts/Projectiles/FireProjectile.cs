using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class FireProjectile : Projectile
{
    // ============ Variables & References ==================================================================================================================
    
    private int damage;

    // ======================================================================================================================================================
    
    public FireProjectile() : base("Fireball",new Vector2(0.6f, 0.5f)) // (Constructor): Initializes the fireball sprite and collider
    {
        RotationOffset = 180;
    }

    public void InitializeFireProjectile(Unit owner, Unit target, Vector2 startPosition, float movementSpeed, int projectileDamage) // Sets up the fireball's specific damage, speed, and target
    {
        damage = Math.Max(0, projectileDamage);
        InitializeProjectile(owner: owner, target: target, startPosition: startPosition, movementSpeed: movementSpeed, projectileScale: 0.9f);
    }

    protected override void ApplyEffect(Unit hitUnit) // (Override): Deals raw damage to the hit unit
    {
        hitUnit.TakeDamage(damage);
        Console.WriteLine($"wizard hit {hitUnit} with {damage} damage");
    }
}