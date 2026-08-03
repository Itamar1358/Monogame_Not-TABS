using System;

namespace MonoGame2026_Heb;

public class Knight : Unit
{
    public Knight()
        : base(
            spriteName: "Knight",
            maxHealth: 100,
            damage: 25,
            cost: 25,
            movementSpeed: 100f,
            attackRange: 125f,
            attackCooldown: 1f,
            unitScale: 0.3f)
    {
        RotationOffset = 90f;
    }
    
    protected override void PerformAttack(Unit target)
    {
        target.TakeDamage(Damage);
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