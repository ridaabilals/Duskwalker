using System.Collections.Generic;
using UnityEngine;

namespace SupanthaPaul
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 7f;
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float deceleration = 24f;

        [Header("Jumping")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float fallMultiplier = 2.5f;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask whatIsGround;
        [SerializeField] private int extraJumpCount = 1;
        [SerializeField] private GameObject jumpEffect;

        [Header("Dashing")]
        [SerializeField] private float dashSpeed = 30f;
        [Tooltip("Amount of time (in seconds) the player will be in the dashing speed")]
        [SerializeField] private float startDashTime = 0.1f;
        [Tooltip("Time (in seconds) between dashes")]
        [SerializeField] private float dashCooldown = 0.2f;
        [SerializeField] private GameObject dashEffect;

        [Header("Mobile Input")]
        public bool mobileLeftPressed;
        public bool mobileRightPressed;
        public bool mobileJumpPressed;
        public bool mobileAttackPressed;
        public MobileJoyStick mobileJoystick;

        // Access needed for handling animation in Player script and other uses
        [HideInInspector] public bool isGrounded;
        [HideInInspector] public float moveInput;
        [HideInInspector] public bool canMove = true;
        [HideInInspector] public bool isDashing = false;
        [HideInInspector] public bool actuallyWallGrabbing = false;
        [HideInInspector] public bool isCurrentlyPlayable = false;
        [HideInInspector] public bool isAttacking = false;
        public bool FacingLeft => !m_facingRight;

        [Header("Wall grab & jump")]
        [Tooltip("Right offset of the wall detection sphere")]
        public Vector2 grabRightOffset = new Vector2(0.16f, 0f);
        public Vector2 grabLeftOffset = new Vector2(-0.16f, 0f);
        public float grabCheckRadius = 0.24f;
        public float slideSpeed = 2.5f;
        public Vector2 wallJumpForce = new Vector2(10.5f, 18f);
        public Vector2 wallClimbForce = new Vector2(4f, 14f);

        private Rigidbody2D m_rb;
        private Animator m_animator;
        private SpriteRenderer m_spriteRenderer;
        private PlayerAttack m_playerAttack;
        private ParticleSystem m_dustParticle;
        private bool m_facingRight = true;
        private readonly float m_groundedRememberTime = 0.25f;
        private float m_groundedRemember = 0f;
        private int m_extraJumps;
        private float m_extraJumpForce;
        private float m_dashTime;
        private bool m_hasDashedInAir = false;
        private bool m_onWall = false;
        private bool m_onRightWall = false;
        private bool m_onLeftWall = false;
        private bool m_wallGrabbing = false;
        private readonly float m_wallStickTime = 0.25f;
        private float m_wallStick = 0f;
        private bool m_wallJumping = false;
        private float m_dashCooldown;
        private bool m_prevMobileAttackPressed;
        private bool m_hasSpeedParameter;
        private bool m_hasGroundedParameter;
        private bool m_hasYVelocityParameter;
        private bool m_hasJumpParameter;
        private bool m_hasDoubleJumpParameter;
        private bool m_hasAttackParameter;
        private bool m_hasWallGrabParameter;

        // 0 -> none, 1 -> right, -1 -> left
        private int m_onWallSide = 0;
        private int m_playerSide = 1;

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int GroundedHash = Animator.StringToHash("Grounded");
        private static readonly int YVelocityHash = Animator.StringToHash("YVelocity");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int WallGrabHash = Animator.StringToHash("WallGrab");
        private static readonly int MeleeHash = Animator.StringToHash("Melee");

        private void Awake()
        {
            m_rb = GetComponent<Rigidbody2D>();
            if (m_rb == null)
            {
                Debug.LogError("PlayerController: Rigidbody2D is missing from this player.");
                enabled = false;
                return;
            }

            m_animator = GetComponentInChildren<Animator>(true);
            m_spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
            m_playerAttack = GetComponent<PlayerAttack>();

            if (m_animator == null)
                Debug.LogWarning("PlayerController: Animator was not found on the player or its children.");
            else if (m_animator.runtimeAnimatorController == null)
                Debug.LogWarning("PlayerController: Animator Controller is missing on the attached Animator.");

            if (m_spriteRenderer == null)
                Debug.LogWarning("PlayerController: SpriteRenderer was not found on the player hierarchy.");

            if (m_playerAttack == null)
                Debug.LogWarning("PlayerController: PlayerAttack component is missing on this player.");

            CacheAnimatorParameters();
        }

        private void Start()
        {
            if (jumpEffect != null)
                PoolManager.instance.CreatePool(jumpEffect, 2);

            if (dashEffect != null)
                PoolManager.instance.CreatePool(dashEffect, 2);

            // if it's the player, make this instance currently playable
            if (transform.CompareTag("Player"))
                isCurrentlyPlayable = true;

            m_extraJumps = extraJumpCount;
            m_dashTime = startDashTime;
            m_dashCooldown = dashCooldown;
            m_extraJumpForce = jumpForce * 0.7f;
            m_dustParticle = GetComponentInChildren<ParticleSystem>();
        }

        private void FixedUpdate()
        {
            if (m_rb == null)
                return;

            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, whatIsGround);
            var position = transform.position;
            m_onWall = Physics2D.OverlapCircle((Vector2)position + grabRightOffset, grabCheckRadius, whatIsGround)
                      || Physics2D.OverlapCircle((Vector2)position + grabLeftOffset, grabCheckRadius, whatIsGround);
            m_onRightWall = Physics2D.OverlapCircle((Vector2)position + grabRightOffset, grabCheckRadius, whatIsGround);
            m_onLeftWall = Physics2D.OverlapCircle((Vector2)position + grabLeftOffset, grabCheckRadius, whatIsGround);

            CalculateSides();

            if ((m_wallGrabbing || isGrounded) && m_wallJumping)
                m_wallJumping = false;

            if (!isCurrentlyPlayable)
                return;

            if (m_wallJumping)
            {
                float targetX = moveInput * speed;
                float smoothedX = Mathf.Lerp(m_rb.velocity.x, targetX, 1.25f * Time.fixedDeltaTime);
                m_rb.velocity = new Vector2(smoothedX, m_rb.velocity.y);
            }
            else
            {
                if (canMove && !m_wallGrabbing)
                {
                    float targetVelocityX = moveInput * speed;
                    float smoothX = Mathf.MoveTowards(m_rb.velocity.x, targetVelocityX, (Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration) * Time.fixedDeltaTime);
                    m_rb.velocity = new Vector2(smoothX, m_rb.velocity.y);
                }
                else if (!canMove)
                {
                    float smoothX = Mathf.MoveTowards(m_rb.velocity.x, 0f, deceleration * Time.fixedDeltaTime);
                    m_rb.velocity = new Vector2(smoothX, m_rb.velocity.y);
                }
            }

            if (m_rb.velocity.y < 0f)
            {
                m_rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1f) * Time.fixedDeltaTime;
            }

            if (!m_facingRight && moveInput > 0f)
                Flip();
            else if (m_facingRight && moveInput < 0f)
                Flip();

            if (isDashing)
            {
                if (m_dashTime <= 0f)
                {
                    isDashing = false;
                    m_dashCooldown = dashCooldown;
                    m_dashTime = startDashTime;
                    m_rb.velocity = new Vector2(0f, m_rb.velocity.y);
                }
                else
                {
                    m_dashTime -= Time.deltaTime;
                    m_rb.velocity = new Vector2(m_facingRight ? dashSpeed : -dashSpeed, m_rb.velocity.y);
                }
            }

            if (m_onWall && !isGrounded && m_rb.velocity.y <= 0f && m_playerSide == m_onWallSide)
            {
                actuallyWallGrabbing = true;
                m_wallGrabbing = true;
                float wallVelocityX = Mathf.Lerp(m_rb.velocity.x, moveInput * speed, 0.8f);
                m_rb.velocity = new Vector2(wallVelocityX, Mathf.Lerp(m_rb.velocity.y, -slideSpeed, 0.85f));
                m_wallStick = m_wallStickTime;
            }
            else
            {
                m_wallStick -= Time.deltaTime;
                actuallyWallGrabbing = false;
                if (m_wallStick <= 0f)
                    m_wallGrabbing = false;
            }

            if (m_wallGrabbing && isGrounded)
                m_wallGrabbing = false;

            if (m_dustParticle != null)
            {
                float playerVelocityMag = m_rb.velocity.sqrMagnitude;
                if (m_dustParticle.isPlaying && playerVelocityMag == 0f)
                {
                    m_dustParticle.Stop();
                }
                else if (!m_dustParticle.isPlaying && playerVelocityMag > 0f)
                {
                    m_dustParticle.Play();
                }
            }
        }

        private void Update()
        {
            moveInput = GetHorizontalInput();

            if (isGrounded)
                m_extraJumps = extraJumpCount;

            m_groundedRemember -= Time.deltaTime;
            if (isGrounded)
                m_groundedRemember = m_groundedRememberTime;

            if (!isCurrentlyPlayable)
                return;

            if (!isDashing && !m_hasDashedInAir && m_dashCooldown <= 0f && GetDashInputDown())
            {
                isDashing = true;
                if (dashEffect != null)
                    PoolManager.instance.ReuseObject(dashEffect, transform.position, Quaternion.identity);
                if (!isGrounded)
                    m_hasDashedInAir = true;
            }

            m_dashCooldown -= Time.deltaTime;

            if (m_hasDashedInAir && isGrounded)
                m_hasDashedInAir = false;

            bool jumpPressed = GetJumpInputDown();

            if (jumpPressed && m_wallGrabbing && m_onWallSide != 0 && moveInput != 0 && Mathf.Sign(moveInput) == m_onWallSide)
            {
                WallClimbJump();
            }
            else if (jumpPressed && m_wallGrabbing && m_onWallSide != 0)
            {
                WallJump();
            }
            else if (jumpPressed && m_extraJumps > 0 && !isGrounded && !m_wallGrabbing)
            {
                Jump(m_extraJumpForce);
                m_extraJumps--;
            }
            else if (jumpPressed && (isGrounded || m_groundedRemember > 0f))
            {
                Jump(jumpForce);
            }

            if (m_animator != null)
            {
                if (m_hasSpeedParameter)
                    m_animator.SetFloat(SpeedHash, Mathf.Abs(m_rb.velocity.x));
                if (m_hasYVelocityParameter)
                    m_animator.SetFloat(YVelocityHash, m_rb.velocity.y);
                if (m_hasGroundedParameter)
                    m_animator.SetBool(GroundedHash, isGrounded);
                if (m_hasWallGrabParameter)
                    m_animator.SetBool(WallGrabHash, actuallyWallGrabbing);
            }

            m_prevMobileAttackPressed = mobileAttackPressed;
        }

        public float GetHorizontalInput()
        {
            float keyboard = InputSystem.HorizontalRaw();
            if (Mathf.Abs(keyboard) > 0.01f)
                return keyboard;

            if (mobileJoystick != null)
            {
                float joystick = mobileJoystick.Horizontal;
                if (Mathf.Abs(joystick) > 0.01f)
                    return joystick;
            }

            if (mobileLeftPressed && !mobileRightPressed)
                return -1f;
            if (mobileRightPressed && !mobileLeftPressed)
                return 1f;

            return 0f;
        }

        public bool GetJumpInputDown()
        {
            bool keyboardJump = InputSystem.Jump();
            bool mobileJump = mobileJumpPressed;
            mobileJumpPressed = false;
            return keyboardJump || mobileJump;
        }

        public bool GetDashInputDown()
        {
            return InputSystem.Dash();
        }

        public bool GetAttackInputDown()
        {
            bool keyboardAttack = Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.J);
            bool mobileAttack = mobileAttackPressed && !m_prevMobileAttackPressed;
            return keyboardAttack || mobileAttack;
        }

        public void SetAttacking(bool attacking)
        {
            isAttacking = attacking;
            if (m_animator != null && m_hasAttackParameter)
                m_animator.SetBool(AttackHash, attacking);
        }

        public void Attack()
        {
            if (m_playerAttack != null)
            {
                m_playerAttack.TryAttack();
                return;
            }

            if (GetComponent<PlayerAttack>() != null)
            {
                m_playerAttack = GetComponent<PlayerAttack>();
                m_playerAttack.TryAttack();
                return;
            }

            Debug.LogWarning("PlayerController: No PlayerAttack component found to execute attack.");
        }

        public void TryAttack()
        {
            Attack();
        }

        private void Jump(float jumpValue)
        {
            m_rb.velocity = new Vector2(m_rb.velocity.x, jumpValue);
            if (jumpEffect != null)
                PoolManager.instance.ReuseObject(jumpEffect, groundCheck != null ? groundCheck.position : transform.position, Quaternion.identity);
            if (m_animator != null)
            {
                if (m_hasJumpParameter)
                    m_animator.SetTrigger(JumpHash);
                if (m_hasDoubleJumpParameter && m_extraJumps > 0 && !isGrounded)
                    m_animator.SetTrigger(DoubleJumpHash);
            }
        }

        private void WallJump()
        {
            m_wallGrabbing = false;
            m_wallJumping = true;
            if (m_playerSide == m_onWallSide)
                Flip();
            m_rb.velocity = new Vector2(-m_onWallSide * wallJumpForce.x, wallJumpForce.y);
        }

        private void WallClimbJump()
        {
            m_wallGrabbing = false;
            m_wallJumping = true;
            if (m_playerSide == m_onWallSide)
                Flip();
            m_rb.velocity = new Vector2(-m_onWallSide * wallClimbForce.x, wallClimbForce.y);
        }

        void Flip()
        {
            m_facingRight = !m_facingRight;
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }

        void CalculateSides()
        {
            if (m_onRightWall)
                m_onWallSide = 1;
            else if (m_onLeftWall)
                m_onWallSide = -1;
            else
                m_onWallSide = 0;

            m_playerSide = m_facingRight ? 1 : -1;
        }

        private void CacheAnimatorParameters()
        {
            if (m_animator == null)
                return;

            foreach (AnimatorControllerParameter parameter in m_animator.parameters)
            {
                int parameterHash = Animator.StringToHash(parameter.name);
                m_hasSpeedParameter |= parameterHash == SpeedHash;
                m_hasGroundedParameter |= parameterHash == GroundedHash;
                m_hasYVelocityParameter |= parameterHash == YVelocityHash;
                m_hasJumpParameter |= parameterHash == JumpHash;
                m_hasDoubleJumpParameter |= parameterHash == DoubleJumpHash;
                m_hasAttackParameter |= parameterHash == AttackHash;
                m_hasWallGrabParameter |= parameterHash == WallGrabHash;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
            Gizmos.DrawWireSphere((Vector2)transform.position + grabRightOffset, grabCheckRadius);
            Gizmos.DrawWireSphere((Vector2)transform.position + grabLeftOffset, grabCheckRadius);
        }
    }
}
