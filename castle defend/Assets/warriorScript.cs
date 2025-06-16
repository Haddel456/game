using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class WarriorScript : MonoBehaviour
{
    Rigidbody2D rb;
    Animator animator;
    public float speed = 1f;
    bool hasStopped = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // No gravity, and never allow rotation
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void FixedUpdate()
    {
        if (!hasStopped)
        {
            // Only move when we haven’t yet hit the castle
            rb.velocity = new Vector2(speed, 0f);
        }
    }

    void Update()
    {
        // Walk anim only while we’re moving
        //animator.SetBool("isWalking", !hasStopped);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("ourcastle"))
        {
            hasStopped = true;

            // kill any leftover motion
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;

            // Freeze everything (pos + rot)
            rb.constraints = RigidbodyConstraints2D.FreezeAll;

            //animator.SetBool("isWalking", false);


        }
    }
}

