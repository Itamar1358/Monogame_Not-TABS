namespace MonoGame2026_Heb;

public interface IDamageable
{
    // ============ Variables & References ==================================================================================================================
    
    int MaxHealth { get; }
    int CurrentHealth { get; }
    bool IsAlive { get; }

    // ======================================================================================================================================================
    
    void TakeDamage(int damageAmount); // Interface method to apply damage
    void Heal(int healAmount); // Interface method to restore health
}