using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image fillImage;

    [Header("Health Component Connection")]
    [SerializeField] private HealthComponent healthComponent;

    [Header("Color Settings")]
    [SerializeField] private Color healthyColor = Color.green;
    [SerializeField] private Color damagedColor = Color.yellow;
    [SerializeField] private Color criticalColor = Color.red;

    [Header("Animation")]
    [SerializeField] private bool animateChanges = true;
    [SerializeField] private float animationSpeed = 2f;

    private float targetHealth = 1f;

    void Start()
    {
        // Initialize slider
        if (healthSlider == null)
            healthSlider = GetComponent<Slider>();

        if (fillImage == null && healthSlider != null)
            fillImage = healthSlider.fillRect.GetComponent<Image>();

        // Find HealthComponent if not assigned
        if (healthComponent == null)
        {
            // Try to find it on a GameObject tagged "Castle"
            GameObject castle = GameObject.FindGameObjectWithTag("Castle");
            if (castle != null)
            {
                healthComponent = castle.GetComponent<HealthComponent>();
            }
        }

        // Connect to health component events
        if (healthComponent != null)
        {
            // Subscribe to health changes using UnityEvent
            healthComponent.OnHealthChangedUnity.AddListener(UpdateHealth);

            // Set initial health value
            UpdateHealth(healthComponent.GetHealthPercentage());
        }
        else
        {
            Debug.LogWarning("HealthComponent not found! Please assign it in the inspector or tag your castle with 'Castle'");
        }

        // Set initial values
        if (healthSlider != null)
        {
            healthSlider.value = 1f;
            UpdateHealthColor(1f);
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from events to prevent errors
        if (healthComponent != null)
        {
            healthComponent.OnHealthChangedUnity.RemoveListener(UpdateHealth);
        }
    }

    void Update()
    {
        // Animate health bar changes
        if (animateChanges && healthSlider != null)
        {
            if (Mathf.Abs(healthSlider.value - targetHealth) > 0.01f)
            {
                healthSlider.value = Mathf.Lerp(healthSlider.value, targetHealth,
                    animationSpeed * Time.deltaTime);
                UpdateHealthColor(healthSlider.value);
            }
        }
    }

    public void UpdateHealth(float healthPercentage)
    {
        targetHealth = Mathf.Clamp01(healthPercentage);

        if (!animateChanges && healthSlider != null)
        {
            healthSlider.value = targetHealth;
            UpdateHealthColor(targetHealth);
        }

    }

    private void UpdateHealthColor(float healthPercentage)
    {
        if (fillImage == null) return;

        if (healthPercentage > 0.6f)
        {
            fillImage.color = healthyColor;
        }
        else if (healthPercentage > 0.3f)
        {
            fillImage.color = damagedColor;
        }
        else
        {
            fillImage.color = criticalColor;
        }
    }

    // Manual method for testing
    [ContextMenu("Test Health 50%")]
    public void TestHealth50()
    {
        UpdateHealth(0.5f);
    }

    [ContextMenu("Test Health 25%")]
    public void TestHealth25()
    {
        UpdateHealth(0.25f);
    }
}