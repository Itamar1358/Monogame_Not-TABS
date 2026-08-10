using System;
using Microsoft.Xna.Framework;
    
namespace MonoGame2026_Heb;

public class HealerProjectile : Projectile
{
    // ============ Variables & References ==================================================================================================================
    
    private int damage;

    // ======================================================================================================================================================
    
    public HealerProjectile() : base("HealingProjectile",new Vector2(0.4f, 0.4f)) // (Constructor): Initializes the HealerProjectile sprite
    {
        RotationOffset = 90f;
    }

    public void InitializeHealerProjectile(Unit owner, Unit target, Vector2 startPosition, float movementSpeed, int projectileDamage) // Sets up the HealerProjectile's specific damage, speed, and target
    {
        damage = Math.Max(0, projectileDamage);
        InitializeProjectile(owner: owner, target: target, startPosition: startPosition, movementSpeed: movementSpeed, projectileScale: 0.4f);
    }

    protected override void ApplyEffect(Unit hitUnit) // (Override): Deals raw damage to the hit unit
    {
        hitUnit.TakeDamage(damage);
        Console.WriteLine($"Healer hit {hitUnit} with {damage} damage");
    }
}