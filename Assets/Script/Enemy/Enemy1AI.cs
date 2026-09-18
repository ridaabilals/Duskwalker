using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    private bool movingRight = true;

    [Header("Detection")]
    public Transform edgeCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) return;

        // Move the enemy
        rb.velocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.velocity.y);

        // Check for edge or wall
        bool hitWallOrEdge = !Physics2D.Raycast(edgeCheck.position, Vector2.down, 1f, groundLayer);

        if (hitWallOrEdge)
        {
            Flip();
        }
    }

    void Flip()
    {
    movingRight = !movingRight;
    
    // Get the current scale
    Vector3 localScale = transform.localScale;
    
    // Simply flip the X sign
    localScale.x *= -1f;
    
    // Re-assign it
    transform.localScale = localScale;
    }

    // Called by the HeadCheck child object script
    public void TakeDamage()
    {
        if (isDead) return;
        isDead = true;
        
        // Physics reaction: Disable main collider and fall through floor
        GetComponent<BoxCollider2D>().enabled = false;
        rb.velocity = new Vector2(0, 5f);// Small pop up before falling
        
        StartCoroutine(FadeAndDestroy());
    }

    IEnumerator FadeAndDestroy()
    {
        float duration = 1.5f;
        float currentTime = 0;
        Color startColor = sr.color;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }
  void FixedUpdate() 
{
    if (isDead) return;

    rb.velocity = new Vector2(movingRight ? moveSpeed : -moveSpeed, rb.velocity.y);

    // 1. CLIFF DETECTION (Detects if the ground ends)
    RaycastHit2D groundInfo = Physics2D.Raycast(edgeCheck.position, Vector2.down, 1f, groundLayer);

    // 2. WALL DETECTION (Detects if a wall is in front)
    // We shoot a ray horizontally in the direction we are moving
    Vector2 rayDirection = movingRight ? Vector2.right : Vector2.left;
    RaycastHit2D wallInfo = Physics2D.Raycast(edgeCheck.position, rayDirection, 0.5f, groundLayer);

    // Flip if we hit the end of a cliff OR if we walk into a wall
    if (groundInfo.collider == null || wallInfo.collider != null)
    {
        Flip();
    }
}

}