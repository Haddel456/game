using UnityEngine;

public class GlobalMouseManager : MonoBehaviour
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothness = 30f; // How smoothly warrior follows mouse
    [SerializeField] private LayerMask warriorLayerMask = -1; // Which layers can be dragged

    private Camera mainCamera;
    private WarriorController draggedWarrior;
    private Rigidbody2D draggedRigidbody;
    private bool isDragging = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            mainCamera = FindObjectOfType<Camera>();
    }

    void Update()
    {
        HandleMouseInput();
        if (isDragging && draggedWarrior != null && draggedWarrior.IsAlive())
        {
            DragWarrior();
        }
        else if (isDragging)
        {
            // Warrior died or was destroyed while dragging
            ForceStopDrag();
        }
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
            StartDrag();
        }
        else if (Input.GetMouseButtonUp(0)) // Left mouse button released
        {
            StopDrag();
        }
    }

    void StartDrag()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Raycast to see what we clicked on
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero, Mathf.Infinity, warriorLayerMask);

        if (hit.collider != null)
        {
            // Try to get the warrior controller
            WarriorController warrior = hit.collider.GetComponent<WarriorController>();
            if (warrior != null && warrior.IsAlive())
            {
                draggedWarrior = warrior;
                draggedRigidbody = hit.collider.GetComponent<Rigidbody2D>();

                if (draggedRigidbody != null)
                {
                    isDragging = true;
                    draggedWarrior.OnDragStart();
                }
            }
        }
    }

    void DragWarrior()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Use much smoother and more responsive movement
        Vector3 difference = mousePosition - draggedWarrior.transform.position;
        Vector3 targetPosition = draggedWarrior.transform.position + (difference * dragSmoothness * Time.deltaTime);

        // Make it even more responsive by moving closer to mouse position
        targetPosition = Vector3.Lerp(draggedWarrior.transform.position, mousePosition, dragSmoothness * Time.deltaTime);

        draggedRigidbody.MovePosition(targetPosition);
    }

    void StopDrag()
    {
        if (isDragging && draggedWarrior != null && draggedWarrior.IsAlive())
        {
            draggedWarrior.OnDragStop();
        }
        ForceStopDrag();
    }

    public void ForceStopDrag()
    {
        isDragging = false;
        draggedWarrior = null;
        draggedRigidbody = null;
    }
}