using System;
using Microsoft.Xna.Framework;
using MonoGame2026_Heb.MeleeWeapons;

namespace MonoGame2026_Heb;

public class Ogre : Unit
{
    // ============ Variables & References ==================================================================================================================
    
    private Club club;
    public const int BaseHealth = 700;
    public const int BaseDamage = 40;
    public const int BaseCost = 150; 
    public const float BaseMovementSpeed = 70f;
    public const float BaseAttackRange = 140f;
    public const float BaseAttackCooldown = 2f;
    public const float BaseUnitScale = 0.35f;
    
    // ======================================================================================================================================================

    // (Constructor): Sets base stats like health, damage, and speed
    public Ogre() : base(spriteName: "Ogre", maxHealth: BaseHealth, damage: BaseDamage, cost: BaseCost, movementSpeed: BaseMovementSpeed, attackRange: BaseAttackRange, attackCooldown: BaseAttackCooldown, unitScale: BaseUnitScale)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Ogre_Hurt", "Ogre_VeryHurt");
    }

    public override void Cleanup()
    {
        if (club != null)
        {
            SceneManager.Remove(club);
            club = null;
        }
        base.Cleanup();
    }

    protected override void InitializeEquipment() // Spawns and attaches a Club melee weapon to the ogre
    {
        if (club != null) return;
        club = SceneManager.Create<Club>();
        club.InitializeWeapon(weaponOwner: this, weaponDamage: Damage, weaponOffset: new Vector2(0f, -55f), weaponScale: 0.1f);
    }
    
    protected override void PerformAttack(Unit target) // Triggers the attached club's StartSwing() function
    {
        club.StartSwing();
        Console.WriteLine($"Ogre attacked {target} " + $"Health remaining: {target.CurrentHealth}");
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