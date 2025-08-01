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
    [SerializeField] private float fallSpeed = 8f; // Speed for falling back to ground
    private Vector3 dragStartPosition; // Position where drag started
    private bool isBeingDragged = false;
    private bool isDead = false;
    private bool isFallingBack = false;
    private bool deathEventFired = false; // Prevent multiple death events
    private Vector3 targetPosition; // Where warrior should land


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
            animator.SetTrigger("StartAttack");
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


            if (isDead)
            {
                animator.SetTrigger("Death");
                FireDeathEvent();
                StartCoroutine(DelayedDestroy(1.5f));
            }
            else
            {
                CheckIfNearCastle();
            }
        }
    }

    private void FireDeathEvent()
    {
        if (!deathEventFired)
        {
            deathEventFired = true;
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

    private void AttackCastle()
    {
        if (targetHealth != null && !isDead)
        {
            targetHealth.TakeDamage(damageAmount);
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

            targetHealth = other.GetComponent<HealthComponent>();
            if (targetHealth == null)
            {
                Debug.LogWarning("Castle doesn't have a HealthComponent!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("ourcastle"))
        {
            hasStopped = false;
            targetHealth = null;
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
        hasStopped = false;
        animator.SetTrigger("ReRun");
    }

    public void OnDragStop()
    {
        if (isDead) return;

        isBeingDragged = false;

        // Stop any residual velocity from dragging
        rb.velocity = Vector2.zero;

        // Check if warrior is in forbidden castle zone
        Vector3 adjustedTargetPosition = GetSafeDropPosition();

        // Set target position: X where released, Y where originally picked up
        targetPosition = new Vector3(adjustedTargetPosition.x, dragStartPosition.y, transform.position.z);


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
            rb.gravityScale = 0f;
        }
    }

    private Vector3 GetSafeDropPosition()
    {
        // Find the castle
        GameObject castle = GameObject.FindGameObjectWithTag("ourcastle");
        if (castle == null) return transform.position;

        // Get castle bounds
        Collider2D castleCollider = castle.GetComponent<Collider2D>();
        if (castleCollider == null) return transform.position;

        Bounds castleBounds = castleCollider.bounds;

        // Define forbidden zone (slightly larger than castle for buffer)
        float forbiddenZoneBuffer = 1f; // Adjust this value as needed
        float leftBoundary = castleBounds.min.x - forbiddenZoneBuffer;
        float rightBoundary = castleBounds.max.x + forbiddenZoneBuffer;
        float topBoundary = castleBounds.max.y + forbiddenZoneBuffer;
        float bottomBoundary = castleBounds.min.y - forbiddenZoneBuffer;

        Vector3 currentPos = transform.position;

        // Check if warrior is in forbidden zone
        bool inForbiddenZone = currentPos.x >= leftBoundary && currentPos.x <= rightBoundary &&
                              currentPos.y >= bottomBoundary && currentPos.y <= topBoundary;

        if (inForbiddenZone)
        {
            // Calculate safe drop position at the door (left side of castle)
            float doorXPosition = castleBounds.min.x - 0.5f; // Slightly to the left of castle
            return new Vector3(doorXPosition, currentPos.y, currentPos.z);
        }

        // If not in forbidden zone, return current position
        return currentPos;
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

        isBeingDragged = false;

        // Stop any residual velocity from dragging
        rb.velocity = Vector2.zero;

        // Use current position as both start and target if not dragging
        if (dragStartPosition == Vector3.zero)
            dragStartPosition = transform.position;

        targetPosition = new Vector3(transform.position.x, dragStartPosition.y, transform.position.z);


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
            rb.gravityScale = 0f;
        }
    }

    // Method to kill warrior instantly (for debugging or special cases)
    public void KillInstantly()
    {
        if (isDead) return;

        isDead = true;
        FireDeathEvent();
        Destroy(gameObject);
    }
}