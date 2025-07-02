using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class WarriorController : MonoBehaviour
{
    [Header("Movement")]
    Rigidbody2D rb;
    Animator animator;
    public float speed = 1f;
    bool hasStopped = false;

    [Header("Combat")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float attackRate = 1f;
    private float nextAttackTime = 0f;
    private HealthComponent targetHealth;

    [Header("Mouse Interaction")]
    [SerializeField] private float killHeightPercentage = 0.6f;
    [SerializeField] private float normalGravity = 3f;
    [SerializeField] private float fallSpeed = 8f; // Speed for falling back to ground
    private Vector3 dragStartPosition; // Position where drag started
    private bool isBeingDragged = false;
    private bool isDead = false;
    private bool isFallingBack = false;
    private bool deathEventFired = false; // NEW: Prevent multiple death events
    private Vector3 targetPosition; // Where warrior should land

    [Header("Death Effects")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private float ghostDuration = 2f;

    // Events
    public static event Action OnWarriorDeath;

    private GlobalMouseManager mouseManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        mouseManager = FindObjectOfType<GlobalMouseManager>();
    }

    void FixedUpdate()
    {
        if (!hasStopped && !isBeingDragged && !isDead && !isFallingBack)
        {
            rb.velocity = new Vector2(speed, 0f);
        }
        else if (isFallingBack)
        {
            FallBackToGround();
        }
    }

    void Update()
    {
        if (hasStopped && targetHealth != null && targetHealth.IsAlive() && !isDead)
        {
            if (Time.time >= nextAttackTime)
            {
                AttackCastle();
                nextAttackTime = Time.time + (1f / attackRate);
            }
        }

        if (isBeingDragged)
        {
            CheckKillHeight();
        }
    }

    private void CheckKillHeight()
    {
        float currentHeight = transform.position.y;
        float originalHeight = dragStartPosition.y;
        float heightDifference = currentHeight - originalHeight;

        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2f;
        float heightPercentage = heightDifference / screenHeight;

        if (heightPercentage >= killHeightPercentage)
        {
            KillWarrior();
        }
    }

    private void FallBackToGround()
    {
        Vector3 currentPos = transform.position;
        Vector3 direction = (targetPosition - currentPos).normalized;

        // Move towards target position
        rb.velocity = direction * fallSpeed;

        // Check if we've reached the target position
        if (Vector3.Distance(currentPos, targetPosition) < 0.2f)
        {
            // Snap to exact target position
            transform.position = targetPosition;
            rb.velocity = Vector2.zero;
            isFallingBack = false;

            Debug.Log($"Warrior landed at: {targetPosition}");

            if (isDead)
            {
                // Warrior was killed, now spawn ghost and destroy
                animator.SetTrigger("Death");
                //SpawnGhost();

                // FIXED: Fire death event immediately when landing, not in coroutine
                FireDeathEvent();
                StartCoroutine(DelayedDestroy(1.5f));
            }
            else
            {
                // Warrior survived, resume normal behavior
                CheckIfNearCastle();
            }
        }
    }

    // NEW: Centralized death event firing
    private void FireDeathEvent()
    {
        if (!deathEventFired)
        {
            deathEventFired = true;
            Debug.Log($"{gameObject.name} death event fired!");
            OnWarriorDeath?.Invoke();
        }
    }

    private System.Collections.IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private void CheckIfNearCastle()
    {
        // Check if still near castle after landing
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        bool foundCastle = false;

        foreach (var col in colliders)
        {
            if (col.CompareTag("ourcastle"))
            {
                hasStopped = true;
                targetHealth = col.GetComponent<HealthComponent>();
                foundCastle = true;
                break;
            }
        }

        if (!foundCastle)
        {
            // Not near castle anymore, resume movement
            hasStopped = false;
            animator.SetTrigger("ReRun");
            targetHealth = null;
        }
    }

    private void KillWarrior()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{gameObject.name} was killed by being lifted too high!");

        if (mouseManager != null)
        {
            mouseManager.ForceStopDrag();
        }

        // Stop dragging but start falling back to ground
        isBeingDragged = false;
        rb.velocity = Vector2.zero;

        // Set target position for dead warrior: X where released, Y where originally picked up
        targetPosition = new Vector3(transform.position.x, dragStartPosition.y, transform.position.z);

        // Start falling back to ground even though dead
        isFallingBack = true;
        rb.gravityScale = 0f; // Use controlled movement, not gravity
    }

    private void SpawnGhost()
    {
        if (ghostPrefab != null)
        {
            GameObject ghost = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
            Destroy(ghost, ghostDuration);
        }
    }

    private void AttackCastle()
    {
        if (targetHealth != null && !isDead)
        {
            targetHealth.TakeDamage(damageAmount);
            Debug.Log($"{gameObject.name} dealt {damageAmount} damage to castle!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ourcastle") && !isDead && !isFallingBack)
        {
            hasStopped = true;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            Debug.Log("castle is attacked");
            animator.SetTrigger("StartAttack");

            targetHealth = other.GetComponent<HealthComponent>();
            if (targetHealth == null)
            {
                Debug.LogWarning("Castle doesn't have a HealthComponent!");
            }
        }
    }

    public void OnDragStart()
    {
        if (isDead) return;

        isBeingDragged = true;
        isFallingBack = false;
        dragStartPosition = transform.position; // Remember where drag started
        rb.gravityScale = 0f;
        rb.drag = 0f; // Remove drag for smoother mouse following
        rb.velocity = Vector2.zero;

        Debug.Log($"Drag started at position: {dragStartPosition}");
    }

    public void OnDragStop()
    {
        if (isDead) return;

        isBeingDragged = false;

        // Stop any residual velocity from dragging
        rb.velocity = Vector2.zero;

        // Set target position: X where released, Y where originally picked up
        targetPosition = new Vector3(transform.position.x, dragStartPosition.y, transform.position.z);

        Debug.Log($"Drag stopped. Current pos: {transform.position}, Target pos: {targetPosition}");

        // Check if warrior should die based on current height
        float currentHeight = transform.position.y;
        float originalHeight = dragStartPosition.y;
        float heightDifference = currentHeight - originalHeight;

        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2f;
        float heightPercentage = heightDifference / screenHeight;

        if (heightPercentage >= killHeightPercentage)
        {
            KillWarrior();
        }
        else
        {
            // Warrior survives, fall back to target position
            isFallingBack = true;
            rb.gravityScale = 0f; // Don't use gravity, use controlled movement
        }
    }

    public bool IsAlive() { return !isDead; }
    public bool IsBeingDragged() { return isBeingDragged; }
    public float GetDamageAmount() { return damageAmount; }

    public void SetDamageAmount(float newDamage)
    {
        damageAmount = newDamage;
    }

    void OnMouseDown()
    {
        if (isDead) return;

        // FIXED: This should handle click-to-kill functionality
        isBeingDragged = false;

        // Stop any residual velocity from dragging
        rb.velocity = Vector2.zero;

        // Use current position as both start and target if not dragging
        if (dragStartPosition == Vector3.zero)
            dragStartPosition = transform.position;

        targetPosition = new Vector3(transform.position.x, dragStartPosition.y, transform.position.z);

        Debug.Log($"Mouse clicked. Current pos: {transform.position}, Target pos: {targetPosition}");

        // Check if warrior should die based on current height
        float currentHeight = transform.position.y;
        float originalHeight = dragStartPosition.y;
        float heightDifference = currentHeight - originalHeight;

        Camera cam = Camera.main;
        float screenHeight = cam.orthographicSize * 2f;
        float heightPercentage = heightDifference / screenHeight;

        if (heightPercentage >= killHeightPercentage)
        {
            KillWarrior();
        }
        else
        {
            // Warrior survives, fall back to target position
            isFallingBack = true;
            rb.gravityScale = 0f; // Don't use gravity, use controlled movement
        }
    }

    // NEW: Method to kill warrior instantly (for debugging or special cases)
    public void KillInstantly()
    {
        if (isDead) return;

        isDead = true;
        FireDeathEvent();
        Destroy(gameObject);
    }
}