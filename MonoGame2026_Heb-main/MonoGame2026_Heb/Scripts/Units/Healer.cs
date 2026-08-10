using System.Collections.Generic;

namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;

public class Healer : Unit
{
    // ============ Variables & References ==================================================================================================================
    
    private const float ProjectileSpeed = 180f;
    public const int BaseHealth = 80;
    public const int BaseDamage = 15;
    public const int BaseCost = 200;
    public const float BaseMovementSpeed = 60f;
    public const float BaseAttackRange = 500f;
    public const float BaseAttackCooldown = 3.1f;
    public const float BaseUnitScale = 0.3f;
    public const float healingCircleOffset = 0.5f;
    
    private HealingCircle healingCircle;
    private Collider healingCircleCollider;
    private readonly HashSet<Unit> healedUnits = new();
    private BattleManager battleManager;
    private bool healingCircleActive;
    private const int healAmount = 50;
    private float healInterval = 10f;
    private const float healCircleDuration = 1f;
    private float healCircleTimer;
    private float healTimer;
    
    // ========================================================================================================================================================
    
    // (Constructor): Sets base stats like health, damage, and speed
    public Healer() : base(spriteName: "Healer", maxHealth: BaseHealth, damage: BaseDamage, cost: BaseCost, movementSpeed: BaseMovementSpeed, attackRange: BaseAttackRange, attackCooldown: BaseAttackCooldown, unitScale: BaseUnitScale)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Healer_Hurt", "Healer_VeryHurt");
    }

    public override void Cleanup()
    {
        if (healingCircle != null)
        {
            SceneManager.Remove(healingCircleCollider);
            SceneManager.Remove(healingCircle);
            healingCircle = null;
            healingCircleCollider = null;
        }
        base.Cleanup();
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        
        if (!IsAlive)
        {
            healingCircle.color = Color.Transparent;
            healingCircleCollider.IsEnabled = false;
            return;
        }
        
        
        
        float deltaTime =
            (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Circle follows the Healer.
        
        healingCircle.tm.position = tm.position + new Vector2(0,  healingCircleOffset) ;

        // Circle is drawn behind the Healer.
        healingCircle.sortingOrder =
            sortingOrder - 1;
        
        if(!IsCombatEnabled) return;
        healTimer -= deltaTime;
        
        if (!healingCircleActive &&
            healTimer <= 0f)
        {
            StartHealingPulse();
        }
        
        // If the circle is currently visible, count down how long it should remain active.
        if (healingCircleActive)
        {
            healCircleTimer -= deltaTime;

            if (healCircleTimer <= 0f)
            {
                EndHealingPulse();
            }
        }
    }
    
    protected override void InitializeEquipment()
    {
        
        healingCircle =
            SceneManager.Create<HealingCircle>();

        healingCircle.tm.position = tm.position;

        healingCircle.tm.scale = new Vector2(2f, 2f);
        
        healingCircle.color = Color.Transparent;

        healingCircleCollider = SceneManager.Create<Collider>();
        healingCircleCollider.SizeMultiplier = new Vector2(2.2f, 1.5f);
        healingCircleCollider.Parent = healingCircle;
        healingCircleCollider.IsTrigger = true;
        healingCircleCollider.IsEnabled = true;

        healingCircleCollider.RegisterOnTrigger(OnHealingCircleTrigger);

        healTimer = healInterval;
        healCircleTimer = 0f;
        healingCircleActive = false;
    }
    
    protected override void PerformAttack(Unit target) // Instantiates a HealerProjectile and fires it at the target
    {
        HealerProjectile projectile = SceneManager.Create<HealerProjectile>();
        AudioManager.PlaySFX?.Invoke(" ");
        projectile.InitializeHealerProjectile(owner: this, target: target, startPosition: GetProjectileSpawnPosition(), movementSpeed: ProjectileSpeed, projectileDamage: Damage);
    }
    
    private void OnHealingCircleTrigger(Collider thisCollider, Collider otherCollider)
    {
        if (!healingCircleActive)
            return;

        Unit unit =
            otherCollider.Parent as Unit;

        if (unit == null)
            return;

        if (!unit.IsAlive)
            return;

        // Only heal allies.
        if (unit.UnitTeam != UnitTeam)
            return;

        if (unit.CurrentHealth >= unit.MaxHealth)
            return;
        
        if(unit == this) return;
        
        
        // The Healer is not allowed to heal duplicates.
        if (healedUnits.Contains(unit))
            return;

        unit.Heal(healAmount);
        healedUnits.Add(unit);
    }
    
    private void StartHealingPulse()
    {
        
        
        healingCircleActive = true;
        
        healCircleTimer = healCircleDuration;
        
        healTimer = healInterval;
        
        // Nobody has been healed by this new pulse yet.
        healedUnits.Clear();

        // Show the circle.
        healingCircle.color = Color.White;

        // Allow it to detect allies.
        healingCircleCollider.IsEnabled = true;
    }
    
    private void EndHealingPulse()
    {
        healingCircleActive = false;

        // Hide the circle again.
        healingCircle.color = Color.Transparent;

        // Stop detecting units.
        healingCircleCollider.IsEnabled = false;

        // Start waiting another 7 seconds.
        healTimer = healInterval;
    }
}