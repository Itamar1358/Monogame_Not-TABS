using System;

namespace MonoGame2026_Heb;

public class Knight : Unit
{
    public Knight()
        : base(
            spriteName: "orangeBird",
            maxHealth: 100,
            damage: 20,
            cost: 25,
            movementSpeed: 100f,
            attackRange: 50f,
            attackCooldown: 1f)
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