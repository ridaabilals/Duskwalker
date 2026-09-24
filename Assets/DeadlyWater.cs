using System.Collections;
using SupanthaPaul;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeadlyWater : MonoBehaviour
{
    [Header("Water physics")]
    [SerializeField] private float waterGravityMultiplier = 0.3f;
    [SerializeField] private float waterDrag = 6f;
    [SerializeField] private float maxSinkSpeed = 8f;
    [SerializeField] private float sinkAcceleration = 18f;

    [Header("Water damage")]
    [SerializeField] private float drowningDuration = 3f;

    private Rigidbody2D playerRb;
    private PlayerController playerController;
    private PlayerCollision playerCollision;
    private Coroutine drainRoutine;
    private float originalGravityScale;
    private float originalDrag;
    private bool originalControllerEnabled;
    private bool isInWater;

    private void Awake()
    {
        Collider2D waterCollider = GetComponent<Collider2D>();
        if (waterCollider != null)
            waterCollider.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Rigidbody2D rb = other.attachedRigidbody != null ? other.attachedRigidbody : other.GetComponent<Rigidbody2D>();
        PlayerController controller = other.GetComponent<PlayerController>();
        PlayerCollision collision = other.GetComponent<PlayerCollision>();

        if (rb == null || controller == null || collision == null)
            return;

        if (isInWater && playerRb == rb)
            return;

        if (drainRoutine != null)
            StopCoroutine(drainRoutine);

        isInWater = true;
        playerRb = rb;
        playerController = controller;
        playerCollision = collision;

        originalGravityScale = playerRb.gravityScale;
        originalDrag = playerRb.drag;
        originalControllerEnabled = playerController.enabled;

        CancelUpwardVelocity();
        playerRb.gravityScale = originalGravityScale * waterGravityMultiplier;
        playerRb.drag = waterDrag;

        playerController.enabled = false;
        playerController.canMove = false;
        playerController.mobileLeftPressed = false;
        playerController.mobileRightPressed = false;
        playerController.mobileJumpPressed = false;
        playerController.mobileAttackPressed = false;

        drainRoutine = StartCoroutine(DrainHeartsOverTime());
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isInWater || playerRb == null || !other.CompareTag("Player"))
            return;

        if (other.attachedRigidbody != playerRb && other.GetComponent<Rigidbody2D>() != playerRb)
            return;

        CancelUpwardVelocity();

        float currentY = playerRb.velocity.y;
        if (currentY > -maxSinkSpeed)
        {
            currentY = Mathf.MoveTowards(currentY, -maxSinkSpeed, sinkAcceleration * Time.fixedDeltaTime);
            playerRb.velocity = new Vector2(playerRb.velocity.x, currentY);
        }
        else
        {
            playerRb.velocity = new Vector2(playerRb.velocity.x, -maxSinkSpeed);
        }

        playerRb.velocity = new Vector2(Mathf.Lerp(playerRb.velocity.x, 0f, 0.25f), playerRb.velocity.y);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerRb == null)
            return;

        if (other.attachedRigidbody != playerRb && other.GetComponent<Rigidbody2D>() != playerRb)
            return;

        StopWaterEffect();
    }

    private void OnDisable()
    {
        StopWaterEffect();
    }

    private void OnDestroy()
    {
        StopWaterEffect();
    }

    private void StopWaterEffect()
    {
        if (drainRoutine != null)
        {
            StopCoroutine(drainRoutine);
            drainRoutine = null;
        }

        if (playerRb != null)
        {
            playerRb.gravityScale = originalGravityScale;
            playerRb.drag = originalDrag;
        }

        if (playerController != null)
        {
            playerController.enabled = originalControllerEnabled;
            if (playerCollision != null && playerCollision.health > 0)
                playerController.canMove = true;
            else
                playerController.canMove = false;

            playerController.mobileLeftPressed = false;
            playerController.mobileRightPressed = false;
            playerController.mobileJumpPressed = false;
            playerController.mobileAttackPressed = false;
        }

        isInWater = false;
        playerRb = null;
        playerController = null;
        playerCollision = null;
    }

    private IEnumerator DrainHeartsOverTime()
    {
        if (playerCollision == null)
            yield break;

        int heartsRemaining = Mathf.Clamp(playerCollision.health, 1, 5);
        float damageInterval = drowningDuration / heartsRemaining;

        for (int i = 0; i < heartsRemaining; i++)
        {
            if (playerCollision == null || playerCollision.health <= 0 || !isInWater)
                yield break;

            playerCollision.TakeDamage(1);

            if (playerCollision == null || playerCollision.health <= 0 || !isInWater)
                yield break;

            yield return new WaitForSeconds(damageInterval);
        }
    }

    private void CancelUpwardVelocity()
    {
        if (playerRb == null)
            return;

        if (playerRb.velocity.y > 0f)
            playerRb.velocity = new Vector2(playerRb.velocity.x, 0f);
    }
}