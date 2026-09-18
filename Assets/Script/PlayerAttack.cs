using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles melee attack input, triggers the attack animation,
/// and detects enemies in range using an overlap circle at the peak of the swing.
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Animator on the player's sprite child.")]
    public Animator animator;

    [Tooltip("Empty child transform positioned at the sword's tip.")]
    public Transform attackPos;

    [Tooltip("Reference to the player controller so animation state is kept in sync.")]
    public SupanthaPaul.PlayerController playerController;

    [Header("Attack Settings")]
    [Tooltip("Radius of the hit detection circle at attackPos.")]
    public float attackRange = 0.5f;

    [Tooltip("Physics layer(s) that enemies are on.")]
    public LayerMask enemyLayer;

    [Tooltip("Minimum time between attacks.")]
    public float attackCooldown = 0.4f;

    [Tooltip("Duration of the melee animation in seconds.")]
    [Min(0.01f)] public float attackDuration = 1f;

    private static readonly int MeleeHash = Animator.StringToHash("Melee");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private bool m_canAttack = true;
    private bool m_swingActive;
    private bool m_hitChecked;
    private Vector3 m_attackOffset;
    private readonly HashSet<IDamageable> m_hitTargets = new HashSet<IDamageable>();
    private static readonly List<RaycastResult> PointerHits = new List<RaycastResult>();

    public static bool AttackInputPressed()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (EventSystem.current != null && Input.GetMouseButtonDown(0))
            {
                var pointer = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                PointerHits.Clear();
                EventSystem.current.RaycastAll(pointer, PointerHits);
                foreach (var hit in PointerHits)
                {
                    if (hit.module is GraphicRaycaster)
                        return false;
                }
            }

            return true;
        }

        return false;
    }

    private static bool HasParameter(Animator animator, string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName))
            return false;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name == paramName)
                return true;
        }

        return false;
    }

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>(true);
        playerController = GetComponent<SupanthaPaul.PlayerController>();

        if (attackPos != null)
            m_attackOffset = transform.InverseTransformPoint(attackPos.position);

        if (animator == null)
            Debug.LogWarning("PlayerAttack: Animator was not found on the player or its children.");

        if (playerController == null)
            Debug.LogWarning("PlayerAttack: PlayerController was not found on this object.");
    }

    public void SetFacing(bool facingLeft)
    {
        if (attackPos == null) return;
        Vector3 offset = m_attackOffset;
        offset.x = Mathf.Abs(offset.x) * (facingLeft ? -1f : 1f);
        attackPos.position = transform.TransformPoint(offset);
    }

    private void Update()
    {
        if (playerController != null)
        {
            if (playerController.GetAttackInputDown() && m_canAttack)
                TryAttack();
            return;
        }

        if (m_canAttack && AttackInputPressed())
            TryAttack();
    }

    public void TryAttack()
    {
        if (playerController != null && !playerController.canMove)
            return;

        if (m_canAttack)
            StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        m_canAttack = false;
        m_swingActive = true;
        m_hitChecked = false;
        m_hitTargets.Clear();

        if (playerController != null)
        {
            SetFacing(playerController.FacingLeft);
            playerController.SetAttacking(true);
        }

        if (animator != null)
        {
            if (HasParameter(animator, "Attack"))
                animator.SetTrigger(AttackHash);
            else if (HasParameter(animator, "Melee"))
                animator.SetTrigger(MeleeHash);
        }

        float hitDelay = Mathf.Min(0.15f, attackDuration);
        yield return new WaitForSeconds(hitDelay);
        CheckHit();

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - hitDelay));
        m_swingActive = false;

        if (playerController != null)
            playerController.SetAttacking(false);

        if (attackCooldown > attackDuration)
            yield return new WaitForSeconds(attackCooldown - attackDuration);

        m_canAttack = true;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        m_swingActive = false;
        if (playerController != null)
            playerController.SetAttacking(false);
        m_canAttack = true;
    }

    /// <summary>
    /// Detects all enemies within attackRange of attackPos and calls TakeDamage on them.
    /// Can also be called directly via an Animation Event on Attack.anim.
    /// </summary>
    public void CheckHit()
    {
        if (attackPos == null || !m_swingActive || m_hitChecked)
            return;

        m_hitChecked = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPos.position, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable != null && m_hitTargets.Add(damageable))
                damageable.TakeDamage();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPos == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos.position, attackRange);
    }
}
