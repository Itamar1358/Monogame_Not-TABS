namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;
public class Hypnotist : Unit
{
    // ============ Variables & References ==================================================================================================================
    
    private const float ProjectileSpeed = 150f;
    private const float HypnosisDuration = 8f;
    public const int BaseHealth = 50;
    public const int BaseDamage = 10;
    public const int BaseCost = 125;
    public const float BaseMovementSpeed = 60f;
    public const float BaseAttackRange = 400f;
    public const float BaseAttackCooldown = 5f;
    public const float BaseUnitScale = 0.3f;

    // =======================================================================================================================================================
    
    // (Constructor): Sets base stats like health, damage, and speed
    public Hypnotist() : base(spriteName: "Hypnotist", maxHealth: BaseHealth, damage: BaseDamage, cost: BaseCost, movementSpeed: BaseMovementSpeed, attackRange: BaseAttackRange, attackCooldown: BaseAttackCooldown, unitScale: BaseUnitScale)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Hypnotist_Hurt", "Hypnotist_VeryHurt");
    }

    protected override void PerformAttack(Unit target) // Instantiates a HypnosisProjectile and fires it at the target
    {
        HypnosisProjectile projectile = SceneManager.Create<HypnosisProjectile>();
        AudioManager.PlaySFX?.Invoke("ConfusionSpellSFX");
        projectile.InitializeHypnosisProjectile(owner: this, target: target, startPosition: GetProjectileSpawnPosition(), movementSpeed: ProjectileSpeed, duration: HypnosisDuration, projectileDamage: Damage);
    }
}