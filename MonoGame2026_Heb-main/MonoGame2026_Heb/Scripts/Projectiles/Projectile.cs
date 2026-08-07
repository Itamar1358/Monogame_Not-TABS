using System;
using Microsoft.Xna.Framework;
namespace MonoGame2026_Heb;

public abstract class Projectile : Animation
{
    // ============ Variables & References ==================================================================================================================
    
    protected Unit Owner { get; private set; }
    protected Unit Target { get; private set; }
    protected Unit.Team FiringTeam { get; private set; }
    public Collider collider { get; }
    public float RotationOffset { get; protected set; }
    public float MovementSpeed { get; private set; }
    public float ProjectileScale{get; private set;}
    private bool hasHit;
    private bool isDestroyed;
    private float lifetimeTimer;
    private const float MaximumLifetime = 5.0f;
    
    // ==========================================================================================================================================================

    protected Projectile(string spriteName, Vector2 colliderSize) : base(spriteName) // (Constructor): Initializes the projectile sprite and base collider properties
    {
        // Every projectile owns a trigger collider.
        collider = SceneManager.Create<Collider>();
        collider.Parent = this;
        collider.IsTrigger = true;
        collider.IsEnabled = false;
        collider.SizeMultiplier = colliderSize;
        collider.RegisterOnTrigger(OnProjectileTrigger);
    }

    public void InitializeProjectile(Unit owner, Unit target, Vector2 startPosition, float movementSpeed, float projectileScale) // Sets up the owner, target, speed, damage, and resets state
    {
        Owner = owner;
        Target = target;
        FiringTeam = owner.UnitTeam;
        tm.position = startPosition;

        MovementSpeed = Math.Max(0, movementSpeed);
        ProjectileScale = Math.Max(0.01f, projectileScale);
        tm.scale =new Vector2(projectileScale, projectileScale);

        hasHit = false;
        isDestroyed = false;
        lifetimeTimer = MaximumLifetime;
        collider.IsEnabled = true;
        PlayAnimation(isLooping: true, samples: 8);
    }

    public override void Update(GameTime gameTime) // Handles movement towards the target, lifetime, and checks for collisions
    {
        if (isDestroyed) return;
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        lifetimeTimer -= deltaTime;
        if (lifetimeTimer <= 0)
        {
            DestroyProjectile();
            return;
        }
        if (Target == null || !Target.IsAlive)
        {
            DestroyProjectile();
            return;
        }
        MoveTowardsTarget(deltaTime);
        base.Update(gameTime);
    }

    private void MoveTowardsTarget(float deltaTime) // Interpolates position towards the target position
    {
        Vector2 direction = Target.tm.position - tm.position;
        float distance = direction.Length();
        if (distance <= 0) return;

        Vector2 normalizedDirection = direction / distance;
        tm.position += normalizedDirection * MovementSpeed * deltaTime;
        
        RotateTowards(Target);
    }
    protected virtual void RotateTowards(Unit target) // Rotates the sprite to face the target
    {
        Vector2 direction = target.tm.position - tm.position;
        if (direction == Vector2.Zero) return;
        float angleInRadians = MathF.Atan2(direction.Y, direction.X);
        tm.rotation = MathHelper.ToDegrees(angleInRadians) + RotationOffset;
    }

    private void OnProjectileTrigger(Collider thisCollider, Collider otherCollider) // Triggers logic when the projectile hits an enemy unit
    {
        if (hasHit || isDestroyed) return;
        Unit hitUnit = otherCollider.Parent as Unit;

        if (hitUnit == null) return;
        if (!hitUnit.IsAlive) return;
        if (hitUnit == Owner) return;
        if (hitUnit.UnitTeam == FiringTeam) return;
        hasHit = true;
        ApplyEffect(hitUnit);
        DestroyProjectile();
    }

    protected abstract void ApplyEffect(Unit hitUnit); // (Abstract): Applies the unique projectile effect to the hit unit

    protected virtual void DestroyProjectile() // Removes the projectile from the active scene
    {
        if (isDestroyed) return;
        isDestroyed = true;
        collider.IsEnabled = false;
        collider.UnregisterOnTrigger(OnProjectileTrigger);

        SceneManager.Remove(collider);
        SceneManager.Remove(this);
    }
    
}