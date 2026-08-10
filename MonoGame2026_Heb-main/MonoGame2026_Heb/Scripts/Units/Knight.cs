using System;
using Microsoft.Xna.Framework;
using MonoGame2026_Heb.MeleeWeapons;

namespace MonoGame2026_Heb;

public class Knight : Unit
{
    // ============ Variables & References ==================================================================================================================
    
    private Sword sword;
    public const int BaseHealth = 150;
    public const int BaseDamage = 35;
    public const int BaseCost = 100;
    public const float BaseMovementSpeed = 100f;
    public const float BaseAttackRange = 140f;
    public const float BaseAttackCooldown = 3.5f;
    public const float BaseUnitScale = 0.3f;
    
    // ======================================================================================================================================================

    // (Constructor): Sets base stats like health, damage, and speed
    public Knight() : base(spriteName: "Knight", maxHealth: BaseHealth, damage: BaseDamage, cost: BaseCost, movementSpeed: BaseMovementSpeed, attackRange: BaseAttackRange, attackCooldown: BaseAttackCooldown, unitScale: BaseUnitScale)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Knight_Hurt", "Knight_VeryHurt");
    }

    public override void Cleanup()
    {
        if (sword != null)
        {
            SceneManager.Remove(sword);
            sword = null;
        }
        base.Cleanup();
    }

    protected override void InitializeEquipment() // Spawns and attaches a Sword melee weapon to the knight
    {
        if (sword != null) return;
        sword = SceneManager.Create<Sword>();
        sword.InitializeWeapon(weaponOwner: this, weaponDamage: Damage, weaponOffset: new Vector2(0f, -55f), weaponScale: 0.1f);
    }
    
    protected override void PerformAttack(Unit target) // Triggers the attached sword's StartSwing() function
    {
        sword.StartSwing();
        Console.WriteLine($"Knight attacked {target}. " + $"Health remaining: {target.CurrentHealth}");
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