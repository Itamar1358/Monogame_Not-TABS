using System;
using System.Collections;
using System.Collections.Generic;
namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;

public abstract class Unit : Animation, IDamageable
{
    // ============ Variables & References ==================================================================================================================
    
    public enum Team { Blue, Red }
    public enum UnitState { Idle, Walking, Attacking, Dead }
    public enum DamageStages { Normal, Hurt, VeryHurt }
    public Collider collider { get; }
    public Team UnitTeam { get; private set; }
    public UnitState CurrentState { get; private set; } = UnitState.Idle;
    public Unit Target { get; private set; }
    
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
    public bool IsAlive => CurrentHealth > 0 && CurrentState != UnitState.Dead;
    public bool IsCombatEnabled { get; private set; }
    
    private const int unitSortingLayer = 10_000;
    public event Action<Unit, int, int> HealthChanged;
    public event Action<Unit> Died;
    public event Action<Unit, Unit> Attacked;
    public event Action<Unit, Team> TeamChanged;
    
    // =======================================================================================================================================================

    //  (Constructor): Sets up the default unit state, scale, and base stats
    protected Unit(string spriteName, int maxHealth, int damage, int cost, float movementSpeed, float attackRange, float attackCooldown, float unitScale) : base(spriteName)
    {
        MaxHealth = Math.Max(1, maxHealth);
        CurrentHealth = MaxHealth;
        normalSpriteName = spriteName;
        Damage = Math.Max(0, damage);
        Cost = Math.Max(0, cost);
        MovementSpeed = Math.Max(0, movementSpeed);
        AttackRange = Math.Max(0, attackRange);
        AttackCooldown = Math.Max(0.1f, attackCooldown);
        
        UnitScale = Math.Max(0.1f, unitScale);
        tm.scale =new Vector2(unitScale, unitScale);

        collider = SceneManager.Create<Collider>();
        collider.Parent = this;
        collider.IsTrigger = false;
        collider.IsEnabled = true;
    }
    
    public void InitializeUnit(Vector2 position, Team team) // Sets the unit's starting position, team, and triggers equipment setup
    {
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
        InitializeEquipment();
        Console.WriteLine($"{UnitTeam} unit created at {tm.position}");
    }

    public virtual void Cleanup() // Cleans up attached equipment when the unit is manually removed
    {
    }

    public override void Update(GameTime gameTime) // Handles state machine transitions, coroutines, hypnosis effects, and cooldowns
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        sortingOrder = unitSortingLayer + (int)tm.position.Y;

        // Coroutines only need processing while at least one coroutine is active.
        if (coroutines.Count > 0) { UpdateCoroutines(deltaTime); }

        if (!IsAlive)
        {
            base.Update(gameTime);
            return;
        }

        // Hypnosis and attack cooldowns continue counting down independently of combat movement.
        UpdateHypnosis(deltaTime);
        UpdateAttackCooldown(deltaTime);

        if (IsCombatEnabled) { UpdateCombat(deltaTime); }
        base.Update(gameTime);
    }
    
    private void StartCoroutine(IEnumerator coroutine) // Adds a coroutine to the list so it can continue over several Update calls
    {
        if (coroutine != null) { coroutines.Add(coroutine); }
    }
    
    private void UpdateCoroutines(float deltaTime) // Progresses all active coroutines frame-by-frame
    {
        coroutineDeltaTime = deltaTime;
        
        // Iterate backwards because finished coroutines are removed during this loop.
        for (int i = coroutines.Count - 1; i >= 0; i--)
        {
            // MoveNext runs the coroutine until its next yield statement.
            bool isRunning = coroutines[i].MoveNext();
            if (!isRunning) { coroutines.RemoveAt(i); }
        }
    }
    
    public void SetCombatEnabled(bool isEnabled) // Toggles whether the unit can engage in combat
    {
        if (!IsAlive) return;
        IsCombatEnabled = isEnabled;
        Console.WriteLine($"{UnitTeam} combat enabled: {IsCombatEnabled}");
        if (!isEnabled)
        {
            Target = null;
            ChangeState(UnitState.Idle);
        }
    }
    
    protected virtual Vector2 GetProjectileSpawnPosition() // Returns the local offset position where projectiles should originate
    {
        return tm.position;
    }

    public virtual bool CanTarget(Unit otherUnit) // Checks if a target is valid (alive, different team, etc...)
    {
        return otherUnit != null && otherUnit != this && otherUnit.IsAlive && otherUnit.UnitTeam != UnitTeam;
    }
    
    public void SetTarget(Unit newTarget) // Assigns a valid target and starts walking when combat is already active
    {
        if (newTarget == null)
        {
            Target = null;
            return;
        }
        if (!CanTarget(newTarget)) return;
        Target = newTarget;
        if (IsAlive && IsCombatEnabled) { ChangeState(UnitState.Walking); } 
        Console.WriteLine($"{UnitTeam} selected {Target.UnitTeam} target");
    }
    
    public void ClearTarget() //Removes the current target so an external battle system can assign a new one
    {
        Target = null;
    }

    private void UpdateCombat(float deltaTime) // Evaluates range and decides whether to move towards or attack the target
    {
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
            RotateTowards(Target);
            ChangeState(UnitState.Attacking);
            TryAttack(Target);
        }
        // Units outside attack range move forward with a small side-to-side rotation.
        else
        {
            walkingSwayTimer += deltaTime;
            float swayRotation = MathF.Sin(walkingSwayTimer * WalkingSwaySpeed) * WalkingSwayAmount;
            
            RotateTowards(Target);
            tm.rotation += swayRotation;
            
            ChangeState(UnitState.Walking);
            MoveTowards(Target, deltaTime);
            Vector2 positionBeforeMovement = tm.position;
            float actualMovement = Vector2.Distance(positionBeforeMovement, tm.position);

            Console.WriteLine($"{GetType().Name} walking toward {Target.GetType().Name} | " + $"Distance: {Vector2.Distance(tm.position, Target.tm.position)} | " + $"Moved: {actualMovement}");
        }
    }
    
    public bool IsInAttackRange(Unit otherUnit) // Checks if the target is within the unit's weapon range
    {
        if (otherUnit == null) return false;
        float distance = Vector2.Distance(tm.position, otherUnit.tm.position);
        return distance <= AttackRange + attackRangeTolerance;
    }
    
    protected virtual void MoveTowards(Unit target, float deltaTime) // Moves toward the target without stepping past the edge of attack range
    {
        Vector2 direction = target.tm.position - tm.position;
        float distance = direction.Length();
        if (distance <= 0) return;

        // Normalizing keeps direction length at one so speed controls the movement amount
        Vector2 normalizedDirection = direction / distance;

        float maximumMovement = MovementSpeed * deltaTime;

        // prevents the unit from moving through its attack range
        float distanceUntilAttackRange = Math.Max(0, distance - AttackRange);
        
        // Clamp movement so a large frame step cannot overshoot the stopping point.
        float movementAmount = Math.Min(maximumMovement, distanceUntilAttackRange);
        tm.position += normalizedDirection * movementAmount;
    }
    
    protected virtual void RotateTowards(Unit target) // Rotates the token toward its target and applies the artwork's facing offset
    {
        Vector2 direction = target.tm.position - tm.position;
        if (direction == Vector2.Zero) return;
        float angleInRadians = MathF.Atan2(direction.Y, direction.X);
        tm.rotation = MathHelper.ToDegrees(angleInRadians) + RotationOffset;
    }

    public bool TryAttack(Unit target) // Attempts to perform an attack if cooldown allows
    {
        if (!IsAlive) return false;
        if (!CanTarget(target)) return false;
        if (!IsInAttackRange(target)) return false;
        if (attackCooldownTimer > 0) return false;

        PerformAttack(target);
        attackCooldownTimer = AttackCooldown;
        Attacked?.Invoke(this, target);

        return true;
    }

    protected abstract void PerformAttack(Unit target); // (Abstract): Must be implemented by derived classes to execute the attack logic
    
    private void UpdateAttackCooldown(float deltaTime) // Counts the attack cooldown back down to zero
    {
        if (attackCooldownTimer <= 0) return;
        attackCooldownTimer -= deltaTime;
        if (attackCooldownTimer < 0) { attackCooldownTimer = 0; }
    }

    public void TakeDamage(int damageAmount) // Reduces health, flashes damage sprites, and checks for death
    {
        if (!IsAlive || damageAmount <= 0) return;

        AudioManager.PlaySFX?.Invoke("HitSFX");
        CurrentHealth = Math.Max(0, CurrentHealth - damageAmount);
        UpdateDamageVisual();
        HealthChanged?.Invoke(this, CurrentHealth, MaxHealth);

        if (CurrentHealth == 0) { Die(); }
    }

    public void Heal(int healAmount) // Increases health
    {
        if (!IsAlive || healAmount <= 0) return;
        int previousHealth = CurrentHealth;
        CurrentHealth = Math.Min(MaxHealth, CurrentHealth + healAmount);
        if (CurrentHealth != previousHealth) { HealthChanged?.Invoke(this, CurrentHealth, MaxHealth); }
    }

    public void ChangeTeam(Team newTeam) // Changes the unit's team alignment and triggers team visual updates (Hypnotist)
    {
        if (!IsAlive || UnitTeam == newTeam) return;
        UnitTeam = newTeam;
        Target = null;
        ApplyTeamVisual();
        TeamChanged?.Invoke(this, newTeam);
    }

    protected void ChangeState(UnitState newState) // Transitions the unit state (Idle, Walking, Attacking)
    {
        if (CurrentState == newState) return;
        if (CurrentState == UnitState.Dead) return;

        CurrentState = newState;
        Console.WriteLine($"{UnitTeam} changed state to {CurrentState}");
        OnStateChanged(newState);
    }

    protected virtual void OnStateChanged(UnitState newState) // (Virtual): Handles visual/animation changes on state transition
    {
        switch (newState)
        {
            case UnitState.Idle:
                PlayAnimation(isLooping: true, samples: 6);
                break;

            case UnitState.Walking:
                PlayAnimation(isLooping: true, samples: 10);
                break;

            case UnitState.Attacking:
                PlayAnimation(isLooping: true, samples: 8);
                break;

            case UnitState.Dead:
                PauseAnimation();
                break;
        }
    }
    
    protected virtual void Die() // Sets state to dead, disables colliders, and starts the death animation
    {
        if (CurrentState == UnitState.Dead) return;

        string deathSound = GetType().Name == "Wizard" ? "MagicianDeath" : GetType().Name + "Death";
        AudioManager.PlaySFX?.Invoke(deathSound);

        CurrentHealth = 0;
        IsCombatEnabled = false;
        Target = null;
        collider.IsEnabled = false;

        ChangeState(UnitState.Dead);
        StartCoroutine(DeathAnimation());
        Console.WriteLine($"{UnitTeam} unit died");
        Died?.Invoke(this);
    }

    protected virtual void ApplyTeamVisual() // Tints the unit color based on their team
    {
        color = UnitTeam == Team.Blue ? Color.Aquamarine : Color.WhiteSmoke;
    }
    
    public void ApplyHypnosis(Team hypnotistTeam, float duration) // Applies a temporary hypnosis state, changing the unit's team
    {
        if (!IsAlive) return;
        if (!isHypnotized && UnitTeam == hypnotistTeam) return;
        if (!isHypnotized) { originalTeam = UnitTeam; }

        // Refreshing these values also refreshes the duration of an existing hypnosis effect.
        isHypnotized = true;
        hypnosisTimer = Math.Max(0.1f, duration);

        // Team changes clear the old target because it may now be an ally.
        ChangeTeam(hypnotistTeam);
        IsCombatEnabled = true;
        ChangeState((UnitState.Idle));
    }
    
    private void UpdateHypnosis(float deltaTime) // Manages the hypnosis timer and reverts team alignment when it expires
    {
        if (!isHypnotized) return;
        hypnosisTimer -= deltaTime;
        if (hypnosisTimer > 0) return;
        isHypnotized = false;
        hypnosisTimer = 0;

        ChangeTeam(originalTeam);
        ClearTarget();
    }

    private IEnumerator DeathAnimation() // A coroutine that fades out the unit sprite upon death
    {
        // Save the original appearance so the animation remains relative to this unit.
        Vector2 startingScale = tm.scale;
        float startingRotation = tm.rotation;
        Color startingColor = color;
        float elapsedTime = 0f;

        // Each loop iteration represents one frame because it yields at the bottom
        while (elapsedTime < DeathAnimationDuration)
        {
            elapsedTime += coroutineDeltaTime;

            // Convert elapsed time into a normalized animation value from 0 to 1.
            float progress = MathHelper.Clamp(elapsedTime / DeathAnimationDuration, 0f, 1f);
            float scaleMultiplier;

            // Briefly enlarge the token.
            if (progress < 0.2f)
            {
                float popProgress = progress / 0.2f;
                scaleMultiplier = MathHelper.Lerp(1f, 1.15f, popProgress);
            }
            else
            {
                // Shrink into the final corpse size.
                float shrinkProgress = (progress - 0.2f) / 0.8f;
                scaleMultiplier = MathHelper.Lerp(1.15f, 0.7f, shrinkProgress);
            }

            // Apply the current scale, rotation and colour for this frame.
            tm.scale = startingScale * scaleMultiplier;
            tm.rotation = MathHelper.Lerp(startingRotation, startingRotation + 90f, progress);
            color = Color.Lerp(startingColor, Color.Gray * 0.5f, progress);

            yield return null;
        }
        tm.scale = startingScale * 0.7f;
        tm.rotation = startingRotation + 90f;
        color = Color.Gray * 0.5f;
    }
    
    protected void ConfigureDamageSprites(string hurtSprite, string veryHurtSprite) // Caches specific hurt/very hurt sprites for the unit
    {
        hurtSpriteName = hurtSprite;
        veryHurtSpriteName = veryHurtSprite;
    }
    
    private void UpdateDamageVisual() // Swaps the unit's sprite based on current health percentages
    {
        float healthPercentage = (float)CurrentHealth / MaxHealth;
        DamageStages newStage;

        if (healthPercentage <= 1f / 3f) { newStage = DamageStages.VeryHurt; }
        else if (healthPercentage <= 2f / 3f) { newStage = DamageStages.Hurt; }
        else { newStage = DamageStages.Normal; }
        if (newStage == currentDamageVisualStage) return;

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
    
    protected virtual void InitializeEquipment() // (Virtual): Sets up any weapons or accessories attached to the unit
    { }
}