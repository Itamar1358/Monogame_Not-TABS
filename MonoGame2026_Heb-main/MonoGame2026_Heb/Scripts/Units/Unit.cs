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

    public enum DamageStages
    {
        Normal,
        Hurt,
        VeryHurt
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
    
    private DamageStages currentDamageVisualStage = DamageStages.Normal;
    private readonly string normalSpriteName;
    private string hurtSpriteName;
    private string veryHurtSpriteName;
    
    private const float attackRangeTolerance = 0.5f;
    public float AttackCooldown { get; private set; }
    public float RotationOffset { get; protected set; }
    private float attackCooldownTimer;
    
    private float walkingSwayTimer;
    private const float WalkingSwayAmount = 6f; // Degrees
    private const float WalkingSwaySpeed = 8f;
    
    public float UnitScale { get; private set; }

    public bool IsAlive =>
        CurrentHealth > 0 &&
        CurrentState != UnitState.Dead;

    public bool IsCombatEnabled { get; private set; }
    
    private const int unitSortingLayer = 10_000;
    
    // events used by ui, audio and battle systems
    public event Action<Unit, int, int> HealthChanged;
    public event Action<Unit> Died;
    public event Action<Unit, Unit> Attacked;
    public event Action<Unit, Team> TeamChanged;

    // Sets the permanent stats and visual setup shared by every unit type.
    // Math.Max prevents invalid negative or zero values from entering the unit.
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
        // Initialize health at its maximum value when the object is constructed.
        MaxHealth = Math.Max(1, maxHealth);
        CurrentHealth = MaxHealth;
        spriteName = normalSpriteName;
        Damage = Math.Max(0, damage);
        Cost = Math.Max(0, cost);

        // Store the movement and combat values supplied by the specific unit subclass.
        MovementSpeed = Math.Max(0, movementSpeed);
        AttackRange = Math.Max(0, attackRange);
        AttackCooldown = Math.Max(0.1f, attackCooldown);
        UnitScale = Math.Max(0.1f, unitScale);
        tm.scale =new Vector2(unitScale, unitScale);
        

        // every unit owns a collider
        // The collider is created separately, then linked to this unit as its parent.
        collider = SceneManager.Create<Collider>();
        collider.Parent = this;
        collider.IsTrigger = false;
        collider.IsEnabled = true;
    }

    // Resets this unit for placement on the battlefield before combat begins.
    public void InitializeUnit(
        Vector2 position,
        Team team)
    {
        
        tm.position = position;

        // Store both the current team and the original team used after hypnosis ends.
        UnitTeam = team;
        originalTeam = team;
        CurrentHealth = MaxHealth;
        CurrentState = UnitState.Idle;
        
        // Clear temporary hypnosis data from any previous use of this unit.
        isHypnotized = false;
        hypnosisTimer = 0;

        // Newly placed units begin without a target and cannot fight until combat is enabled.
        Target = null;
        IsCombatEnabled = false;
        attackCooldownTimer = 0;

        collider.IsEnabled = true;
        
        // Update the token colour and start the animation belonging to the initial state.
        ApplyTeamVisual();
        OnStateChanged(CurrentState);
        
        InitializeEquipment();
        Console.WriteLine(
            $"{UnitTeam} unit created at {tm.position}");
    }

    // Runs once per frame and updates temporary effects, cooldowns, combat and visuals.
    public override void Update(GameTime gameTime)
    {
        float deltaTime =
            (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        sortingOrder = unitSortingLayer + (int)tm.position.Y;

        // Coroutines only need processing while at least one coroutine is active.
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

        // Hypnosis and attack cooldowns continue counting down independently of combat movement.
        UpdateHypnosis(deltaTime);
        UpdateAttackCooldown(deltaTime);

        if (IsCombatEnabled)
        {
            UpdateCombat(deltaTime);
        }
        
        
        // Let the inherited Animation/Sprite classes update the frame and destination rectangle.
        base.Update(gameTime);
    }
    
    // Adds a coroutine to the list so it can continue over several Update calls.
    private void StartCoroutine(IEnumerator coroutine)
    {
        if (coroutine != null)
        {
            coroutines.Add(coroutine);
        }
    }

    // Advances each active coroutine by one step and removes completed routines.
    private void UpdateCoroutines(float deltaTime)
    {
        coroutineDeltaTime = deltaTime;

        // Iterate backwards because finished coroutines are removed during this loop.
        for (int i = coroutines.Count - 1; i >= 0; i--)
        {
            // MoveNext runs the coroutine until its next yield statement.
            bool isRunning = coroutines[i].MoveNext();

            if (!isRunning)
            {
                coroutines.RemoveAt(i);
            }
        }
    }

    // Enables or disables this unit's combat behaviour.
    public void SetCombatEnabled(bool isEnabled)
    {
        if (!IsAlive)
            return;

        IsCombatEnabled = isEnabled;
        Console.WriteLine(
            $"{UnitTeam} combat enabled: {IsCombatEnabled}");

        // Disabling combat also clears the target and returns the unit to idle.
        if (!isEnabled)
        {
            Target = null;
            ChangeState(UnitState.Idle);
        }

    }
    
    // Ranged subclasses can override this to spawn projectiles from a different point.
    protected virtual Vector2 GetProjectileSpawnPosition()
    {
        return tm.position;
    }
    

    // A valid target must be alive, be a different unit and belong to the opposing current team.
    public virtual bool CanTarget(Unit otherUnit)
    {
        return otherUnit != null &&
               otherUnit != this &&
               otherUnit.IsAlive &&
               otherUnit.UnitTeam != UnitTeam;
               
    }

    // Assigns a valid target and starts walking when combat is already active.
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

    // Removes the current target so an external battle system can assign a new one.
    public void ClearTarget()
    {
        Target = null;
    }

    // Chooses between attacking and moving based on the current target's distance.
    private void UpdateCombat(float deltaTime)
    {
        // wait for another system to assign a target
        // Invalid targets are cleared; the BattleManager is expected to find a replacement.
        if (!CanTarget(Target))
        {
            Target = null;
            walkingSwayTimer = 0f;
            ChangeState(UnitState.Idle);
            return;
        }

        RotateTowards(Target);

        // Units inside attack range stop swaying and attempt to attack.
        if (IsInAttackRange(Target))
        {
            walkingSwayTimer = 0f;

            // Face the target without swaying.
            RotateTowards(Target);
            ChangeState(UnitState.Attacking);
            TryAttack(Target);
        }
        // Units outside attack range move forward with a small side-to-side rotation.
        else
        {
            walkingSwayTimer += deltaTime;

            // Sine smoothly alternates the added rotation between left and right.
            float swayRotation = MathF.Sin(walkingSwayTimer * WalkingSwaySpeed) * WalkingSwayAmount;

            // Face the target, then add the side-to-side tilt.
            RotateTowards(Target);
            tm.rotation += swayRotation;
            
            ChangeState(UnitState.Walking);
            MoveTowards(Target, deltaTime);
            Vector2 positionBeforeMovement = tm.position;
            float actualMovement =
                Vector2.Distance(
                    positionBeforeMovement,
                    tm.position);

            Console.WriteLine(
                $"{GetType().Name} walking toward {Target.GetType().Name} | " +
                $"Distance: {Vector2.Distance(tm.position, Target.tm.position)} | " +
                $"Moved: {actualMovement}");
        }
    }

    // Checks centre-to-centre distance against this unit's attack range.
    public bool IsInAttackRange(Unit otherUnit)
    {
        if (otherUnit == null)
            return false;

        float distance = Vector2.Distance(
            tm.position,
            otherUnit.tm.position);

        return distance <= AttackRange + attackRangeTolerance;
    }

    // Moves toward the target without stepping past the edge of attack range.
    protected virtual void MoveTowards(
        Unit target,
        float deltaTime)
    {
        Vector2 direction =
            target.tm.position - tm.position;

        float distance = direction.Length();

        if (distance <= 0)
            return;

        // Normalizing keeps direction length at one so speed controls the movement amount.
        Vector2 normalizedDirection =
            direction / distance;

        float maximumMovement =
            MovementSpeed * deltaTime;

        // prevents the unit from moving through its attack range
        float distanceUntilAttackRange =
            Math.Max(0, distance - AttackRange);

        // Clamp movement so a large frame step cannot overshoot the stopping point.
        float movementAmount =
            Math.Min(
                maximumMovement,
                distanceUntilAttackRange);

        tm.position +=
            normalizedDirection * movementAmount;
    }

    // Rotates the token toward its target and applies the artwork's facing offset.
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

    // Performs an attack only when all combat requirements are satisfied.
    public bool TryAttack(Unit target)
    {
        if (!IsAlive)
            return false;

        if (!CanTarget(target))
            return false;

        if (!IsInAttackRange(target))
            return false;

        // The cooldown blocks repeated attacks until enough time has passed.
        if (attackCooldownTimer > 0)
            return false;

        // The concrete unit subclass decides whether this is melee, fireball, hypnosis, and so on.
        PerformAttack(target);

        attackCooldownTimer = AttackCooldown;

        // Notify any listeners after a successful attack is performed.
        Attacked?.Invoke(this, target);

        return true;
    }

    // each troop decides how its attack works
    protected abstract void PerformAttack(Unit target);

    // Counts the attack cooldown back down to zero.
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

    // Applies damage, notifies listeners and starts death when health reaches zero.
    public void TakeDamage(int damageAmount)
    {
        if (!IsAlive || damageAmount <= 0)
            return;

        AudioManager.PlaySFX?.Invoke("HitSFX");
        
        CurrentHealth = Math.Max(
            0,
            CurrentHealth - damageAmount);
        UpdateDamageVisual();
        // The event can update UI such as a health bar.
        HealthChanged?.Invoke(
            this,
            CurrentHealth,
            MaxHealth);

        if (CurrentHealth == 0)
        {
            Die();
        }
    }

    // Restores health without allowing it to exceed MaxHealth.
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

    // Temporarily or permanently changes allegiance, then clears the now-invalid target.
    // used later by the hypnotist
    public void ChangeTeam(Team newTeam)
    {
        if (!IsAlive || UnitTeam == newTeam)
            return;

        UnitTeam = newTeam;
        Target = null;

        // Refresh the token colour before notifying battle systems about the team change.
        ApplyTeamVisual();

        TeamChanged?.Invoke(this, newTeam);
    }

    // Changes state once and forwards the change to the animation hook.
    protected void ChangeState(UnitState newState)
    {
        // Avoid restarting the same state's animation every frame.
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

    // Selects animation behaviour for each state.
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

    // Stops gameplay behaviour, disables collision and begins the visual death sequence.
    protected virtual void Die()
    {
        if (CurrentState == UnitState.Dead)
            return;

        string deathSound = GetType().Name == "Wizard" ? "MagicianDeath" : GetType().Name + "Death";
        AudioManager.PlaySFX?.Invoke(deathSound);

        CurrentHealth = 0;
        IsCombatEnabled = false;
        Target = null;

        // Dead units remain drawable but no longer participate in collisions.
        collider.IsEnabled = false;

        ChangeState(UnitState.Dead);

        StartCoroutine(DeathAnimation());

        Console.WriteLine(
            $"{UnitTeam} unit died");

        Died?.Invoke(this);
    }

    // Colours the same token artwork according to its current team.
    protected virtual void ApplyTeamVisual()
    {
        color = UnitTeam == Team.Blue
            ? Color.Aquamarine
            : Color.WhiteSmoke;
    }

    // Converts a living enemy to the hypnotist's team for a limited duration.
    public void ApplyHypnosis(
        Team hypnotistTeam,
        float duration)
    {
        if (!IsAlive)
            return;

        // Ignore a fresh hypnosis attempt when the unit already belongs to that team.
        // Do not hypnotize a unit already on that team.
        if (!isHypnotized && UnitTeam == hypnotistTeam)
            return;

        // Save the real team only when hypnosis first begins.
        if (!isHypnotized)
        {
            originalTeam = UnitTeam;
        }

        // Refreshing these values also refreshes the duration of an existing hypnosis effect.
        isHypnotized = true;
        hypnosisTimer = Math.Max(0.1f, duration);

        // Team changes clear the old target because it may now be an ally.
        ChangeTeam(hypnotistTeam);
        IsCombatEnabled = true;
        ChangeState((UnitState.Idle));
    }
    
    // Counts down hypnosis and restores the saved original team when time expires.
    private void UpdateHypnosis(float deltaTime)
    {
        if (!isHypnotized)
            return;

        hypnosisTimer -= deltaTime;

        if (hypnosisTimer > 0)
            return;

        isHypnotized = false;
        hypnosisTimer = 0;

        // Changing back invalidates the current target, so it is cleared for reassignment.
        ChangeTeam(originalTeam);
        ClearTarget();
    }
    
    // Coroutine that enlarges, shrinks, rotates and fades the token over several frames.
    private IEnumerator DeathAnimation()
    {
        // Save the original appearance so the animation remains relative to this unit.
        Vector2 startingScale = tm.scale;
        float startingRotation = tm.rotation;
        Color startingColor = color;

        float elapsedTime = 0f;

        // Each loop iteration represents one frame because it yields at the bottom.
        while (elapsedTime < DeathAnimationDuration)
        {
            elapsedTime += coroutineDeltaTime;

            // Convert elapsed time into a normalized animation value from 0 to 1.
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

            // Apply the current scale, rotation and colour for this frame.
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

        // Force exact final values in case frame timing passed slightly beyond the duration.
        // Exact final corpse appearance.
        tm.scale = startingScale * 0.7f;
        tm.rotation = startingRotation + 90f;
        color = Color.Gray * 0.5f;
    }
    
    protected void ConfigureDamageSprites(
        string hurtSprite,
        string veryHurtSprite)
    {
        hurtSpriteName = hurtSprite;
        veryHurtSpriteName = veryHurtSprite;
    }
    
    private void UpdateDamageVisual()
    {
        float healthPercentage =
            (float)CurrentHealth / MaxHealth;

        DamageStages newStage;

        if (healthPercentage <= 1f / 3f)
        {
            newStage = DamageStages.VeryHurt;
        }
        else if (healthPercentage <= 2f / 3f)
        {
            newStage = DamageStages.Hurt;
        }
        else
        {
            newStage = DamageStages.Normal;
        }

        if (newStage == currentDamageVisualStage)
            return;

        currentDamageVisualStage = newStage;

        switch (currentDamageVisualStage)
        {
            case DamageStages.Normal:
                ChangeSprite(normalSpriteName);
                break;

            case DamageStages.Hurt:
                ChangeSprite(hurtSpriteName);
                break;

            case DamageStages.VeryHurt:
                ChangeSprite(veryHurtSpriteName);
                break;
        }

        OnStateChanged(CurrentState);
    }
    
    protected virtual void InitializeEquipment()
    {
    }
    
}