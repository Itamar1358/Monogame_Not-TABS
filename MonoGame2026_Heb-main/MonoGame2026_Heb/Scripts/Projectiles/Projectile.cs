using System;
using Microsoft.Xna.Framework;
namespace MonoGame2026_Heb;

public abstract class Projectile : Animation
{
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

    protected Projectile(string spriteName, Vector2 colliderSize)
        : base(spriteName)
    {
        // Every projectile owns a trigger collider.
        collider = SceneManager.Create<Collider>();

        collider.Parent = this;
        collider.IsTrigger = true;
        collider.IsEnabled = false;
        collider.SizeMultiplier = colliderSize;
        collider.RegisterOnTrigger(OnProjectileTrigger);
    }

    public void InitializeProjectile(
        Unit owner,
        Unit target,
        Vector2 startPosition,
        float movementSpeed,
        float projectileScale)
    {
        Owner = owner;
        Target = target;

        // Store the team when the projectile is fired.
        // This prevents its team changing if the owner is hypnotized later.
        FiringTeam = owner.UnitTeam;
        tm.position = startPosition;

        MovementSpeed = Math.Max(0, movementSpeed);
        ProjectileScale = Math.Max(0.01f, projectileScale);
        tm.scale =new Vector2(projectileScale, projectileScale);

        hasHit = false;
        isDestroyed = false;
        lifetimeTimer = MaximumLifetime;

        collider.IsEnabled = true;
        PlayAnimation(
            isLooping: true,
            samples: 8);
    }

    public override void Update(GameTime gameTime)
    {
        if (isDestroyed)
            return;

        float deltaTime =
            (float)gameTime.ElapsedGameTime.TotalSeconds;

        lifetimeTimer -= deltaTime;

        if (lifetimeTimer <= 0)
        {
            DestroyProjectile();
            return;
        }

        // Stop the projectile if its target died.
        if (Target == null || !Target.IsAlive)
        {
            DestroyProjectile();
            return;
        }

        MoveTowardsTarget(deltaTime);

        // Updates the sprite rectangle using tm.
        base.Update(gameTime);
    }

    private void MoveTowardsTarget(float deltaTime)
    {
        Vector2 direction =
            Target.tm.position - tm.position;

        float distance = direction.Length();

        if (distance <= 0)
            return;

        Vector2 normalizedDirection =
            direction / distance;

        tm.position +=
            normalizedDirection *
            MovementSpeed *
            deltaTime;

        // Rotate the projectile toward its target.
        RotateTowards(Target);
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

    private void OnProjectileTrigger(
        Collider thisCollider,
        Collider otherCollider)
    {
        if (hasHit || isDestroyed)
            return;

        Unit hitUnit =
            otherCollider.Parent as Unit;

        // Ignore non-unit objects.
        if (hitUnit == null)
            return;

        // Ignore dead units.
        if (!hitUnit.IsAlive)
            return;

        // Ignore the unit that fired the projectile.
        if (hitUnit == Owner)
            return;

        // Ignore units belonging to the firing team.
        if (hitUnit.UnitTeam == FiringTeam)
            return;

        hasHit = true;

        ApplyEffect(hitUnit);
        DestroyProjectile();
    }

    // Mage and Hypnotist implement different effects.
    protected abstract void ApplyEffect(Unit hitUnit);

    protected virtual void DestroyProjectile()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        collider.IsEnabled = false;

        collider.UnregisterOnTrigger(
            OnProjectileTrigger);

        // Replace this with your SceneManager's removal call.
        SceneManager.Remove(collider);
        SceneManager.Remove(this);
    }
    
}