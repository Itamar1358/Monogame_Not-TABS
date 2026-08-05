using System;
using Microsoft.Xna.Framework;
using MonoGame2026_Heb.MeleeWeapons;

namespace MonoGame2026_Heb;

public class Knight : Unit
{
    private Sword sword;
    public Knight()
        : base(
            spriteName: "Knight",
            maxHealth: 100,
            damage: 25,
            cost: 25,
            movementSpeed: 100f,
            attackRange: 140f,
            attackCooldown: 2f,
            unitScale: 0.3f)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Knight_Hurt", "Knight_VeryHurt");
    }

    protected override void InitializeEquipment()
    {
        // Prevent creating another sword if the unit is reinitialized.
        if (sword != null)
            return;

        sword = SceneManager.Create<Sword>();

        sword.InitializeWeapon(
            weaponOwner: this,
            weaponDamage: Damage,

            
            weaponOffset: new Vector2(0f, -55f),
            weaponScale: 0.1f);
    }
    
    protected override void PerformAttack(Unit target)
    {
        sword.StartSwing();
        //target.TakeDamage(Damage);
        Console.WriteLine(
            $"Knight attacked {target}. " +
            $"Health remaining: {target.CurrentHealth}");
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