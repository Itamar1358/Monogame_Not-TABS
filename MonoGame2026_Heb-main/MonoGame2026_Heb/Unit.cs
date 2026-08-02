using System;

namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;

public abstract class Unit : Animation, IDamageable
{
    public enum Team
    {
        Blue,
        Red
    }

    public enum UnitState
    {
        Idle,
        Walking,
        Attacking,
        Dead
    }

    public Collider collider { get; }
    public Team UnitTeam { get; private set; }

    public UnitState CurrentState { get; private set; }
        = UnitState.Idle;

    public Unit Target { get; private set; }

    private bool isHypnotized;
    private float hypnosisTimer;
    private Team originalTeam;

    public int MaxHealth { get; }
    public int CurrentHealth { get; private set; }
    public int Damage { get; private set; }
    public int Cost { get; }
    public float MovementSpeed { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackCooldown { get; private set; }
    public float RotationOffset { get; protected set; }

    public bool IsAlive =>
        CurrentHealth > 0 &&
        CurrentState != UnitState.Dead;

    public bool IsCombatEnabled { get; private set; }

    private float attackCooldownTimer;

    // events used by ui, audio and battle systems
    public event Action<Unit, int, int> HealthChanged;
    public event Action<Unit> Died;
    public event Action<Unit, Unit> Attacked;
    public event Action<Unit, Team> TeamChanged;

    protected Unit(
        string spriteName,
        int maxHealth,
        int damage,
        int cost,
        float movementSpeed,
        float attackRange,
        float attackCooldown)
        : base(spriteName)
    {
        MaxHealth = Math.Max(1, maxHealth);
        CurrentHealth = MaxHealth;

        Damage = Math.Max(0, damage);
        Cost = Math.Max(0, cost);

        MovementSpeed = Math.Max(0, movementSpeed);
        AttackRange = Math.Max(0, attackRange);
        AttackCooldown = Math.Max(0.1f, attackCooldown);

        // every unit owns a collider
        collider = SceneManager.Create<Collider>();
        collider.Parent = this;
        collider.IsTrigger = false;
        collider.IsEnabled = true;
    }

    public void InitializeUnit(
        Vector2 position,
        Team team)
    {
        // tm is inherited from Sprite
        tm.position = position;

        UnitTeam = team;
        originalTeam = team;
        CurrentHealth = MaxHealth;
        CurrentState = UnitState.Idle;
        
        isHypnotized = false;
        hypnosisTimer = 0;

        Target = null;
        IsCombatEnabled = false;
        attackCooldownTimer = 0;

        collider.IsEnabled = true;

        ApplyTeamVisual();
        OnStateChanged(CurrentState);
        Console.WriteLine(
            $"{UnitTeam} unit created at {tm.position}");
    }

    public override void Update(GameTime gameTime)
    {
        float deltaTime =
            (float)gameTime.ElapsedGameTime.TotalSeconds;

        UpdateHypnosis(deltaTime);
        UpdateAttackCooldown(deltaTime);

        if (IsCombatEnabled && IsAlive)
        {
            UpdateCombat(deltaTime);
        }

        base.Update(gameTime);
    }

    public void SetCombatEnabled(bool isEnabled)
    {
        if (!IsAlive)
            return;

        IsCombatEnabled = isEnabled;
        Console.WriteLine(
            $"{UnitTeam} combat enabled: {IsCombatEnabled}");

        if (!isEnabled)
        {
            Target = null;
            ChangeState(UnitState.Idle);
        }

    }

    public bool IsEnemy(Unit otherUnit)
    {
        return otherUnit != null &&
               otherUnit.UnitTeam != UnitTeam;
    }

    public virtual bool CanTarget(Unit otherUnit)
    {
        return otherUnit != null &&
               otherUnit != this &&
               otherUnit.IsAlive &&
               IsEnemy(otherUnit);
    }

    public void SetTarget(Unit newTarget)
    {
        if (newTarget == null)
        {
            Target = null;
            return;
        }

        if (!CanTarget(newTarget))
            return;

        Target = newTarget;
        Console.WriteLine(
            $"{UnitTeam} selected {Target.UnitTeam} target");
    }

    public void ClearTarget()
    {
        Target = null;
    }

    private void UpdateCombat(float deltaTime)
    {
        // wait for another system to assign a target
        if (!CanTarget(Target))
        {
            Target = null;
            ChangeState(UnitState.Idle);
            return;
        }

        RotateTowards(Target);

        if (IsInAttackRange(Target))
        {
            ChangeState(UnitState.Attacking);
            TryAttack(Target);
        }
        else
        {
            ChangeState(UnitState.Walking);
            MoveTowards(Target, deltaTime);
        }
    }

    public bool IsInAttackRange(Unit otherUnit)
    {
        if (otherUnit == null)
            return false;

        float distance = Vector2.Distance(
            tm.position,
            otherUnit.tm.position);

        return distance <= AttackRange;
    }

    protected virtual void MoveTowards(
        Unit target,
        float deltaTime)
    {
        Vector2 direction =
            target.tm.position - tm.position;

        float distance = direction.Length();

        if (distance <= 0)
            return;

        Vector2 normalizedDirection =
            direction / distance;

        float maximumMovement =
            MovementSpeed * deltaTime;

        // prevents the unit from moving through its attack range
        float distanceUntilAttackRange =
            Math.Max(0, distance - AttackRange);

        float movementAmount =
            Math.Min(
                maximumMovement,
                distanceUntilAttackRange);

        tm.position +=
            normalizedDirection * movementAmount;
    }

    protected virtual void RotateTowards(Unit target)
    {
        Vector2 direction =
            target.tm.position - tm.position;

        if (direction == Vector2.Zero)
            return;

        float angleInRadians =
            MathF.Atan2(direction.Y, direction.X);

        // your transform stores rotation in degrees
        tm.rotation =
            MathHelper.ToDegrees(angleInRadians)
            + RotationOffset;
    }

    public bool TryAttack(Unit target)
    {
        if (!IsAlive)
            return false;

        if (!CanTarget(target))
            return false;

        if (!IsInAttackRange(target))
            return false;

        if (attackCooldownTimer > 0)
            return false;

        PerformAttack(target);

        attackCooldownTimer = AttackCooldown;

        Attacked?.Invoke(this, target);

        return true;
    }

    // each troop decides how its attack works
    protected abstract void PerformAttack(Unit target);

    private void UpdateAttackCooldown(float deltaTime)
    {
        if (attackCooldownTimer <= 0)
            return;

        attackCooldownTimer -= deltaTime;

        if (attackCooldownTimer < 0)
        {
            attackCooldownTimer = 0;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive || damageAmount <= 0)
            return;

        CurrentHealth = Math.Max(
            0,
            CurrentHealth - damageAmount);

        HealthChanged?.Invoke(
            this,
            CurrentHealth,
            MaxHealth);

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (!IsAlive || healAmount <= 0)
            return;

        int previousHealth = CurrentHealth;

        CurrentHealth = Math.Min(
            MaxHealth,
            CurrentHealth + healAmount);

        if (CurrentHealth != previousHealth)
        {
            HealthChanged?.Invoke(
                this,
                CurrentHealth,
                MaxHealth);
        }
    }

    // used later by the hypnotist
    public void ChangeTeam(Team newTeam)
    {
        if (!IsAlive || UnitTeam == newTeam)
            return;

        UnitTeam = newTeam;
        Target = null;

        ApplyTeamVisual();

        TeamChanged?.Invoke(this, newTeam);
    }

    protected void ChangeState(UnitState newState)
    {
        if (CurrentState == newState)
            return;

        // dead units cannot return to another state
        if (CurrentState == UnitState.Dead)
            return;

        CurrentState = newState;
        Console.WriteLine(
            $"{UnitTeam} changed state to {CurrentState}");

        OnStateChanged(newState);
    }

    protected virtual void OnStateChanged(
        UnitState newState)
    {
        // specific troops will change animations here
    }

    protected virtual void Die()
    {
        if (CurrentState == UnitState.Dead)
            return;

        CurrentHealth = 0;
        IsCombatEnabled = false;
        Target = null;

        // the corpse remains visible but no longer collides
        collider.IsEnabled = false;

        ChangeState(UnitState.Dead);
        Console.WriteLine(
            $"{UnitTeam} unit died");
        Died?.Invoke(this);
    }

    protected virtual void ApplyTeamVisual()
    {
        color = UnitTeam == Team.Blue
            ? Color.CadetBlue
            : Color.IndianRed;
    }

    public void ApplyHypnosis(
        Team hypnotistTeam,
        float duration)
    {
        if (!IsAlive)
            return;

        // Do not hypnotize a unit already on that team.
        if (!isHypnotized && UnitTeam == hypnotistTeam)
            return;

        // Save the real team only when hypnosis first begins.
        if (!isHypnotized)
        {
            originalTeam = UnitTeam;
        }

        isHypnotized = true;
        hypnosisTimer = Math.Max(0.1f, duration);

        ChangeTeam(hypnotistTeam);

        // Forces the unit to search for a new enemy.
        ClearTarget();
    }
    
    private void UpdateHypnosis(float deltaTime)
    {
        if (!isHypnotized)
            return;

        hypnosisTimer -= deltaTime;

        if (hypnosisTimer > 0)
            return;

        isHypnotized = false;
        hypnosisTimer = 0;

        ChangeTeam(originalTeam);
        ClearTarget();
    }
}