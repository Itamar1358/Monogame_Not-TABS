using System;
using Microsoft.Xna.Framework;
namespace MonoGame2026_Heb;

public class Wizard : Unit
{
    // ============ Variables & References ==================================================================================================================
    
    private const float ProjectileSpeed = 180f;
    public const int BaseHealth = 50;
    public const int BaseDamage = 70;
    public const int BaseCost = 100;
    public const float BaseMovementSpeed = 60f;
    public const float BaseAttackRange = 400f;
    public const float BaseAttackCooldown = 4f;
    public const float BaseUnitScale = 0.3f;
    
    // ========================================================================================================================================================
    
    // (Constructor): Sets base stats like health, damage, and speed
    public Wizard() : base(spriteName: "Wizard", maxHealth: BaseHealth, damage: BaseDamage, cost: BaseCost, movementSpeed: BaseMovementSpeed, attackRange: BaseAttackRange, attackCooldown: BaseAttackCooldown, unitScale: BaseUnitScale)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Wizard_Hurt", "Wizard_VeryHurt");
    }
    
    protected override void PerformAttack(Unit target) // Instantiates a FireProjectile and fires it at the target
    {
        FireProjectile projectile = SceneManager.Create<FireProjectile>();
        AudioManager.PlaySFX?.Invoke("FireballSFX");
        projectile.InitializeFireProjectile(owner: this, target: target, startPosition: GetProjectileSpawnPosition(), movementSpeed: ProjectileSpeed, projectileDamage: Damage);
    }
    
    protected override void OnStateChanged(UnitState newState) // Swaps between Idle, Walk, and Attack animations
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