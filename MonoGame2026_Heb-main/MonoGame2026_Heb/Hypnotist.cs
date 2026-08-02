namespace MonoGame2026_Heb;
using Microsoft.Xna.Framework;
public class Hypnotist : Unit
{
    private const float ProjectileSpeed = 250f;
    private const float HypnosisDuration = 8f;

    public Hypnotist()
        : base(
            spriteName: "Hypnotist",
            maxHealth: 50,
            damage: 0,
            cost: 5,
            movementSpeed: 60f,
            attackRange: 250f,
            attackCooldown: 4f)
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
        return new Vector2(
            destRect.Center.X,
            destRect.Center.Y);
    }
}