using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Unity Events (for Inspector)")]
    public UnityEvent<float> OnHealthChangedUnity; // For Inspector connections
    public UnityEvent OnDeathUnity; // For Inspector connections

    // C# Events for code subscriptions
    public System.Action<float> OnHealthChanged; // Sends current health percentage (0-1)
    public System.Action OnDeath;

    private Animator animator;

    void Start()
    {
        // Initialize health to maximum
        currentHealth = maxHealth;
        // Notify health bar of initial health
        NotifyHealthChanged(GetHealthPercentage());
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return; // Already dead

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify health bar of change
        NotifyHealthChanged(GetHealthPercentage());
        HandleHealthAnimations();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void HandleHealthAnimations()
    {
        float healthPercentage = GetHealthPercentage();

        if (healthPercentage <= 0.1f)
        {
            animator.SetTrigger("Fall");
        }
        else if (healthPercentage <= 0.5f)
        {
            animator.SetTrigger("Destroy");
        }
    }

    public void Heal(float healAmount)
    {
        if (currentHealth <= 0) return; // Can't heal if dead

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify health bar of change
        NotifyHealthChanged(GetHealthPercentage());
    }

    private void Die()
    {
        // Invoke both Unity Events and C# Events
        OnDeathUnity?.Invoke();
        OnDeath?.Invoke();
    }

    private void NotifyHealthChanged(float healthPercentage)
    {
        // Invoke both Unity Events and C# Events
        OnHealthChangedUnity?.Invoke(healthPercentage);
        OnHealthChanged?.Invoke(healthPercentage);
    }

    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void SetMaxHealth(float health)
    {
        maxHealth = health;
    }

    public void RestoreToFullHealth()
    {
        currentHealth = maxHealth;
        if (animator != null)
        {
            animator.Play("defend_Animation", -1, 0f);
        }

        // Notify of health change
        NotifyHealthChanged(GetHealthPercentage());
    }
}