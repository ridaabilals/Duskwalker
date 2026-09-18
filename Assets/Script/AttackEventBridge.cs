using UnityEngine;

/// <summary>
/// Sits on the _Sprite child (same GameObject as the Animator) and receives
/// the "CheckPunchHit" Animation Event from Attack.anim.
/// Forwards the call to PlayerAttack on the parent so the hit detection runs
/// at the exact frame the animation is at its swing peak.
/// </summary>
public class AttackEventBridge : MonoBehaviour
{
    private PlayerAttack m_playerAttack;

    private void Awake()
    {
        m_playerAttack = GetComponentInParent<PlayerAttack>();
    }

    /// <summary>Called by the Animation Event on Attack.anim at frame 2.</summary>
    public void CheckPunchHit()
    {
        m_playerAttack?.CheckHit();
    }
}
