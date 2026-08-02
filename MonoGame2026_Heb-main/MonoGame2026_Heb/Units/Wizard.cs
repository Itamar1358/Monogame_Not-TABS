using System;
using Microsoft.Xna.Framework;
namespace MonoGame2026_Heb;

public class Wizard : Unit
{
    private const float ProjectileSpeed = 30f;
    
    public Wizard()
        : base(
            spriteName: "Wizard",
            maxHealth: 50,
            damage: 70,
            cost: 100,
            movementSpeed: 60f,
            attackRange: 200f,
            attackCooldown: 1f,
            unitScale: 0.3f)
    {
        RotationOffset = 0f;
    }
    
    protected override void PerformAttack(Unit target)
    {
        FireProjectile projectile =
            SceneManager.Create<FireProjectile>();

        projectile.InitializeFireProjectile(owner: this,
            target: target,
            startPosition: GetProjectileSpawnPosition(),
            movementSpeed: ProjectileSpeed,
            projectileDamage: Damage);
    }
    
    private Vector2 GetProjectileSpawnPosition()
    {
        return tm.position;
    }
    
    protected override void OnStateChanged(UnitState newState)
    {
        switch (newState)
        {
            case UnitState.Idle:
                // idle animation will be added later
                break;

            case UnitState.Walking:
                // walking animation will be added later
                break;

            case UnitState.Attacking:
                // attack animation will be added later
                break;

            case UnitState.Dead:
                // death animation will be added later
                break;
        }
    }
}