using System;
using System.Collections;
using System.Collections.Generic;
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
    //public BattleManager battleManager =  new BattleManager();
    
    private readonly List<IEnumerator> coroutines = new();
    private float coroutineDeltaTime;


    private bool isHypnotized;
    private float hypnosisTimer;
    private Team originalTeam;
    
    
    private const float DeathAnimationDuration = 0.6f;
    

    public int MaxHealth { get; }
    public int CurrentHealth { get; private set; }
    public int Damage { get; private set; }
    public int Cost { get; }
    public float MovementSpeed { get; private set; }
    public float AttackRange { get; private set; }
    public float AttackCooldown { get; private set; }
    public float RotationOffset { get; protected set; }
    
    public float UnitScale { get; private set; }

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
        float attackCooldown,
        float unitScale)
        : base(spriteName)
    {
        MaxHealth = Math.Max(1, maxHealth);
        CurrentHealth = MaxHealth;

        Damage = Math.Max(0, damage);
        Cost = Math.Max(0, cost);

        MovementSpeed = Math.Max(0, movementSpeed);
        AttackRange = Math.Max(0, attackRange);
        AttackCooldown = Math.Max(0.1f, attackCooldown);
        UnitScale = Math.Max(0.1f, unitScale);
        tm.scale =new Vector2(unitScale, unitScale);
        

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

        if (coroutines.Count > 0)
        {
            UpdateCoroutines(deltaTime);
        }

        // Dead units stop their behaviour but remain drawable.
        if (!IsAlive)
        {
            base.Update(gameTime);
            return;
        }

        UpdateHypnosis(deltaTime);
        UpdateAttackCooldown(deltaTime);

        if (IsCombatEnabled)
        {
            UpdateCombat(deltaTime);
        }

        base.Update(gameTime);
    }
    
    private void StartCoroutine(IEnumerator coroutine)
    {
        if (coroutine != null)
        {
            coroutines.Add(coroutine);
        }
    }

    private void UpdateCoroutines(float deltaTime)
    {
        coroutineDeltaTime = deltaTime;

        for (int i = coroutines.Count - 1; i >= 0; i--)
        {
            bool isRunning = coroutines[i].MoveNext();

            if (!isRunning)
            {
                coroutines.RemoveAt(i);
            }
        }
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
    
    protected virtual Vector2 GetProjectileSpawnPosition()
    {
        return tm.position;
    }
    

    public virtual bool CanTarget(Unit otherUnit)
    {
        return otherUnit != null &&
               otherUnit != this &&
               otherUnit.IsAlive &&
               otherUnit.UnitTeam != UnitTeam;
               
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
        if (IsAlive && IsCombatEnabled)
        {
            ChangeState(UnitState.Walking);
        } 
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
        switch (newState)
        {
            case UnitState.Idle:
                PlayAnimation(
                    isLooping: true,
                    samples: 6);
                break;

            case UnitState.Walking:
                PlayAnimation(
                    isLooping: true,
                    samples: 10);
                break;

            case UnitState.Attacking:
                PlayAnimation(
                    isLooping: true,
                    samples: 8);
                break;

            case UnitState.Dead:
                PauseAnimation();
                break;
        }
        
    }

    protected virtual void Die()
    {
        if (CurrentState == UnitState.Dead)
            return;

        CurrentHealth = 0;
        IsCombatEnabled = false;
        Target = null;

        collider.IsEnabled = false;

        ChangeState(UnitState.Dead);

        StartCoroutine(DeathAnimation());

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
        IsCombatEnabled = true;
        ChangeState((UnitState.Idle));
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
    
    private IEnumerator DeathAnimation()
    {
        Vector2 startingScale = tm.scale;
        float startingRotation = tm.rotation;
        Color startingColor = color;

        float elapsedTime = 0f;

        while (elapsedTime < DeathAnimationDuration)
        {
            elapsedTime += coroutineDeltaTime;

            float progress = MathHelper.Clamp(
                elapsedTime / DeathAnimationDuration,
                0f,
                1f);

            float scaleMultiplier;

            // Briefly enlarge the token.
            if (progress < 0.2f)
            {
                float popProgress = progress / 0.2f;

                scaleMultiplier = MathHelper.Lerp(
                    1f,
                    1.15f,
                    popProgress);
            }
            else
            {
                // Shrink into the final corpse size.
                float shrinkProgress =
                    (progress - 0.2f) / 0.8f;

                scaleMultiplier = MathHelper.Lerp(
                    1.15f,
                    0.7f,
                    shrinkProgress);
            }

            tm.scale = startingScale * scaleMultiplier;

            tm.rotation = MathHelper.Lerp(
                startingRotation,
                startingRotation + 90f,
                progress);

            color = Color.Lerp(
                startingColor,
                Color.Gray * 0.5f,
                progress);

            // Continue on the next frame.
            yield return null;
        }

        // Exact final corpse appearance.
        tm.scale = startingScale * 0.7f;
        tm.rotation = startingRotation + 90f;
        color = Color.Gray * 0.5f;
    }
}