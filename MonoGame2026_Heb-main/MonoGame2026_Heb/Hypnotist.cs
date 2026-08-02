namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;
public class Hypnotist : Unit
{
    private const float ProjectileSpeed = 30f;
    private const float HypnosisDuration = 8f;

    public Hypnotist()
        : base(
            spriteName: "Hypnotist",
            maxHealth: 50,
            damage: 0,
            cost: 5,
            movementSpeed: 60f,
            attackRange: 200f,
            attackCooldown: 1f,
            unitScale: 0.3f)
    {
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
            duration: HypnosisDuration);
    }
    
    private Vector2 GetProjectileSpawnPosition()
    {
        return tm.position;
    }
}