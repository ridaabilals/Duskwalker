using UnityEngine;

public class BossAI : MonoBehaviour, IDamageable
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public int health = 6;
    
    [Header("Detection Settings")]
    public Transform player;
    public float detectionRange = 5f;

    [Header("Patrol Settings")]
    public Transform leftPoint;
    public Transform rightPoint;
    private Transform currentTarget;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentTarget = rightPoint; // Start by moving toward the right point
        
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            // Chase logic
            FollowPlayer();
        }
        else
        {
            // Patrol logic
            Patrol();
        }
    }

    void FollowPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        FlipSprite(direction.x);
    }

    void Patrol()
    {
        // Move toward current patrol point
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);
        FlipSprite(direction.x);

        // Switch target if we reach a point
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.5f)
        {
            currentTarget = (currentTarget == leftPoint) ? rightPoint : leftPoint;
        }
    }

    void FlipSprite(float directionX)
    {
        if (directionX > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (directionX < 0) transform.localScale = new Vector3(-1, 1, 1);
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0) Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    // Visualizes the detection range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}