using System;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb.MeleeWeapons;

public abstract class MeleeWeapon : Sprite
{
    // ============ Variables & References ==================================================================================================================
    
    public Collider collider { get; }
    private const int weaponSortingLayer = 20_000;
    private Unit owner;
    private int damage;
    private Vector2 localOffset; // Position relative to the center of the unit.
    private readonly float swingDuration;
    private readonly float swingAngle;
    private bool isSwinging;
    private bool hasHit;
    private float swingTimer;
    
    // =======================================================================================================================================================

    protected MeleeWeapon(string spriteName, Vector2 colliderSize, float swingDuration, float swingAngle) : base(spriteName) // (Constructor): Sets swing duration and angle, and initializes the weapon's collider
    {
        this.swingDuration = Math.Max(0.05f, swingDuration);
        this.swingAngle = Math.Max(0f, swingAngle);
        collider = SceneManager.Create<Collider>();
        
        // The collider follows the weapon sprite.
        collider.Parent = this;
        collider.IsTrigger = true;
        collider.IsEnabled = false;
        collider.SizeMultiplier = colliderSize;
        collider.RegisterOnTrigger(OnWeaponTrigger);
    }

    public override void Update(GameTime gameTime) // Updates the weapon's sorting order, handles the swing timer and rotation logic, and delegates positioning
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
        else { collider.IsEnabled = false; }
        FollowOwner(swingRotation);
        base.Update(gameTime);
    }
    public void InitializeWeapon(Unit weaponOwner, int weaponDamage, Vector2 weaponOffset, float weaponScale) // Sets up the weapon's damage, owner, and collision logic
    {
        owner = weaponOwner;
        damage = Math.Max(0, weaponDamage);
        localOffset = weaponOffset;
        float safeScale = Math.Max(0.01f, weaponScale);
        tm.scale = new Vector2(safeScale, safeScale);
        isSwinging = false;
        hasHit = false;
        swingTimer = 0f;
        collider.IsEnabled = false;
    }

    public void StartSwing() // Triggers the swinging logic and resets swing timers if the weapon isn't already swinging
    {
        if (owner == null || !owner.IsAlive) return;
        if (isSwinging) return;

        isSwinging = true;
        hasHit = false;
        swingTimer = 0f;
    }

    private void FollowOwner(float swingRotation) // Rotates and positions the weapon relative to the owner unit's transform and current swing rotation
    {
        float rotationRadians = MathHelper.ToRadians(owner.tm.rotation);
        float cosine = MathF.Cos(rotationRadians);
        float sine = MathF.Sin(rotationRadians);

        // Rotates the local offset with the unit.
        Vector2 rotatedOffset = new Vector2(
            localOffset.X * cosine -
            localOffset.Y * sine,
            localOffset.X * sine +
            localOffset.Y * cosine);

        tm.position = owner.tm.position + rotatedOffset;
        tm.rotation = owner.tm.rotation + swingRotation;
    }

    private void OnWeaponTrigger(Collider thisCollider, Collider otherCollider) // Handles logic when the weapon's collider hits a valid enemy target, dealing damage and playing a sound effect
    {
        if (!isSwinging || hasHit || owner == null) return;
        Unit hitUnit = otherCollider.Parent as Unit;

        if (!owner.CanTarget(hitUnit)) return;

        hasHit = true;
        collider.IsEnabled = false;

        string hitSound = GetType().Name + "HitSFX";
        AudioManager.PlaySFX?.Invoke(hitSound);

        hitUnit.TakeDamage(damage);
    }
}