using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private bool destroyOnHit = false;
    [SerializeField] private string[] targetTags = { "Player", "Enemy", "Castle" };

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isValidTarget = false;
        foreach (string tag in targetTags)
        {
            if (other.CompareTag(tag))
            {
                isValidTarget = true;
                break;
            }
        }

        if (!isValidTarget) return;

        HealthComponent health = other.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isValidTarget = false;
        foreach (string tag in targetTags)
        {
            if (collision.gameObject.CompareTag(tag))
            {
                isValidTarget = true;
                break;
            }
        }

        if (!isValidTarget) return;

        HealthComponent health = collision.gameObject.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.TakeDamage(damageAmount);

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }
}