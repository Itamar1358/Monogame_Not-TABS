using System;
using Microsoft.Xna.Framework;
namespace MonoGame2026_Heb;

public class HypnosisProjectile : Projectile
{
    private float hypnosisDuration;
    private static Random random = new Random();

    public HypnosisProjectile()
        : base("HypnosisBall")
    {
    }

    public void InitializeHypnosisProjectile(
        Unit owner,
        Unit target,
        Vector2 startPosition,
        float movementSpeed,
        float duration)
    {
        hypnosisDuration = Math.Max(0.1f, duration);

        InitializeProjectile(
            owner,
            target,
            GetProjectileSpawnPosition(),
            movementSpeed,
            duration);
    }

    protected override void ApplyEffect(Unit hitUnit)
    {
        const double hypnosisChance = 0.30;

        bool hypnosisSucceeded =
            random.NextDouble() < hypnosisChance;

        if (hypnosisSucceeded)
        {
            hitUnit.ApplyHypnosis(
                FiringTeam,
                hypnosisDuration);
            Console.WriteLine(
                $"Hypnosis succeeded on {hitUnit.UnitTeam} unit.");
        }
        else
        {
            Console.WriteLine("Hypnosis failed.");
        }
       
    }
    private Vector2 GetProjectileSpawnPosition()
    {
        return tm.position;
    }
}