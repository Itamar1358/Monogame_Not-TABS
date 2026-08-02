using System;

namespace MonoGame2026_Heb;

public class Wizard : Unit
{
    public Wizard()
        : base(
            spriteName: "Knight",
            maxHealth: 50,
            damage: 70,
            cost: 100,
            movementSpeed: 60f,
            attackRange: 100f,
            attackCooldown: 3f)
    {
        RotationOffset = 0f;
    }
    
    protected override void PerformAttack(Unit target)
    {
        Projectile projectile =
            SceneManager.Instance.Create<Projectile>();

        projectile.Initialize(
            Position,
            target,
            Damage,
            Team
        );
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