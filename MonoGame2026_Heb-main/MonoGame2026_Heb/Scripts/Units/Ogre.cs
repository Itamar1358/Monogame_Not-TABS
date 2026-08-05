using System;
using Microsoft.Xna.Framework;
using MonoGame2026_Heb.MeleeWeapons;

namespace MonoGame2026_Heb;

public class Ogre : Unit
{
    private Club club;
    public Ogre()
        : base(
            spriteName: "Ogre",
            maxHealth: 200,
            damage: 15,
            cost: 75,
            movementSpeed: 70f,
            attackRange: 140f,
            attackCooldown: 2f,
            unitScale: 0.35f)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Ogre_Hurt", "Ogre_VeryHurt");
    }

    protected override void InitializeEquipment()
    {
        // Prevent creating another sword if the unit is reinitialized.
        if (club != null)
            return;

        club = SceneManager.Create<Club>();

        club.InitializeWeapon(
            weaponOwner: this,
            weaponDamage: Damage,

            // In front of an upward-facing token.
            weaponOffset: new Vector2(0f, -55f),

            // Start around this value and adjust visually.
            weaponScale: 0.1f);
    }
    
    protected override void PerformAttack(Unit target)
    {
        club.StartSwing();
        //target.TakeDamage(Damage);
        Console.WriteLine(
            $"Ogre attacked {target} " +
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