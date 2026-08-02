namespace MonoGame2026_Heb;

public interface IDamageable
{
    int MaxHealth { get; }
    int CurrentHealth { get; }

    bool IsAlive { get; }

    void TakeDamage(int damageAmount);
    void Heal(int healAmount);
}