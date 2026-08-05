using System;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb.MeleeWeapons;

public abstract class MeleeWeapon : Sprite
{
    public Collider collider { get; }
    private const int weaponSortingLayer = 20_000;

    private Unit owner;
    private int damage;

    // Position relative to the center of the unit.
    private Vector2 localOffset;

    private readonly float swingDuration;
    private readonly float swingAngle;

    private bool isSwinging;
    private bool hasHit;
    private float swingTimer;

    protected MeleeWeapon(
        string spriteName,
        Vector2 colliderSize,
        float swingDuration,
        float swingAngle)
        : base(spriteName)
    {
        this.swingDuration =
            Math.Max(0.05f, swingDuration);

        this.swingAngle =
            Math.Max(0f, swingAngle);

        collider =
            SceneManager.Create<Collider>();

        // The collider follows the weapon sprite.
        collider.Parent = this;
        collider.IsTrigger = true;
        collider.IsEnabled = false;
        collider.SizeMultiplier = colliderSize;

        collider.RegisterOnTrigger(
            OnWeaponTrigger);
    }

    public void InitializeWeapon(
        Unit weaponOwner,
        int weaponDamage,
        Vector2 weaponOffset,
        float weaponScale)
    {
        owner = weaponOwner;
        damage = Math.Max(0, weaponDamage);
        localOffset = weaponOffset;

        float safeScale =
            Math.Max(0.01f, weaponScale);

        tm.scale =
            new Vector2(safeScale, safeScale);

        isSwinging = false;
        hasHit = false;
        swingTimer = 0f;

        collider.IsEnabled = false;
    }

    public void StartSwing()
    {
        if (owner == null || !owner.IsAlive)
            return;

        // Prevent another swing from starting before this one finishes.
        if (isSwinging)
            return;

        isSwinging = true;
        hasHit = false;
        swingTimer = 0f;
    }

    public override void Update(GameTime gameTime)
    {
        if (owner == null)
        {
            base.Update(gameTime);
            return;
        }
        sortingOrder = owner.sortingOrder + 1;
        
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        sortingOrder = weaponSortingLayer + (int)owner.tm.position.Y;

        float swingRotation = 0f;

        if (!owner.IsAlive)
        {
            isSwinging = false;
            collider.IsEnabled = false;
        }
        else if (isSwinging)
        {
            swingTimer += deltaTime;

            float progress = MathHelper.Clamp(swingTimer / swingDuration, 0f, 1f);

            // Swing from one side to the other.
            swingRotation = MathHelper.Lerp(-swingAngle, swingAngle, progress);

            // Only deal damage during the middle of the swing.
            bool isInDamageWindow = progress >= 0.25f && progress <= 0.75f;

            collider.IsEnabled = isInDamageWindow && !hasHit;

            if (progress >= 1f)
            {
                isSwinging = false;
                collider.IsEnabled = false;
                swingRotation = 0f;
            }
        }
        else
        {
            collider.IsEnabled = false;
        }

        FollowOwner(swingRotation);

        base.Update(gameTime);
    }

    private void FollowOwner(float swingRotation)
    {
        float rotationRadians =
            MathHelper.ToRadians(owner.tm.rotation);

        float cosine =
            MathF.Cos(rotationRadians);

        float sine =
            MathF.Sin(rotationRadians);

        // Rotates the local offset with the unit.
        Vector2 rotatedOffset = new Vector2(
            localOffset.X * cosine -
            localOffset.Y * sine,

            localOffset.X * sine +
            localOffset.Y * cosine);

        tm.position =
            owner.tm.position + rotatedOffset;

        tm.rotation =
            owner.tm.rotation + swingRotation;
    }

    private void OnWeaponTrigger(
        Collider thisCollider,
        Collider otherCollider)
    {
        if (!isSwinging || hasHit || owner == null)
            return;

        Unit hitUnit =
            otherCollider.Parent as Unit;

        // CanTarget ignores the owner, allies and dead units.
        if (!owner.CanTarget(hitUnit))
            return;

        hasHit = true;
        collider.IsEnabled = false;

        hitUnit.TakeDamage(damage);
    }
}