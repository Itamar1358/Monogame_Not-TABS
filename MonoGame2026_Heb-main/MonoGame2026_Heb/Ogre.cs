using System;

namespace MonoGame2026_Heb;

public class Ogre : Unit
{
    public Ogre()
        : base(
            spriteName: "ogre",
            maxHealth: 200,
            damage: 15,
            cost: 75,
            movementSpeed: 70f,
            attackRange: 50f,
            attackCooldown: 2f)
    {
        RotationOffset = 0f;
    }
    
    protected override void PerformAttack(Unit target)
    {
        target.TakeDamage(Damage);
        Console.WriteLine(
            $"{UnitTeam} attacked {target.UnitTeam}. " +
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