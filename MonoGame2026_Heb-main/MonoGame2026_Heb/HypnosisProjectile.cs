using System;
using Microsoft.Xna.Framework;
namespace MonoGame2026_Heb;

public class HypnosisProjectile : Projectile
{
    private float hypnosisDuration;
    private int damage;
    private static Random random = new Random();

    public HypnosisProjectile()
        : base("HypnosisBall", new Vector2(0.5f, 0.5f))
    {
    }

    public void InitializeHypnosisProjectile(
        Unit owner,
        Unit target,
        Vector2 startPosition,
        float movementSpeed,
        float duration,
        int projectileDamage)
    {
        hypnosisDuration = Math.Max(0.1f, duration);
        damage = Math.Max(0, projectileDamage);
        InitializeProjectile(
            owner,
            target,
            startPosition,
            movementSpeed,
            0.5f);
    }

    protected override void ApplyEffect(Unit hitUnit)
    {
        const double hypnosisChance = 0.30;
        

        bool hypnosisSucceeded =
            random.NextDouble() < hypnosisChance;

        if (hypnosisSucceeded)
        {
            hitUnit.TakeDamage(damage);
            Console.WriteLine($"Hypnosis hit {hitUnit} and dealt {damage} damage.");
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
}