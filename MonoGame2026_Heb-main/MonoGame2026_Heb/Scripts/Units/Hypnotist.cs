namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;
public class Hypnotist : Unit
{
    private const float ProjectileSpeed = 150f;
    private const float HypnosisDuration = 8f;

    public Hypnotist()
        : base(
            spriteName: "Hypnotist",
            maxHealth: 50,
            damage: 10,
            cost: 5,
            movementSpeed: 60f,
            attackRange: 400f,
            attackCooldown: 5f,
            unitScale: 0.3f)
    {
        RotationOffset = 90f;
        ConfigureDamageSprites("Hypnotist_Hurt", "Hypnotist_VeryHurt");
    }

    protected override void PerformAttack(Unit target)
    {
        HypnosisProjectile projectile =
            SceneManager.Create<HypnosisProjectile>();

        projectile.InitializeHypnosisProjectile(
            owner: this,
            target: target,
            startPosition: GetProjectileSpawnPosition(),
            movementSpeed: ProjectileSpeed,
            duration: HypnosisDuration,
            projectileDamage: Damage);
    }
    
}