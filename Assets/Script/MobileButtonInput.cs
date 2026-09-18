using UnityEngine;

namespace SupanthaPaul
{
    public class MobileButtonInput : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerAttack playerAttack;

        private void Awake()
        {
            if (player == null)
            {
                GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
                foreach (var playerObject in playerObjects)
                {
                    var playerComponent = playerObject.GetComponent<SupanthaPaul.PlayerController>();
                    if (playerComponent != null)
                    {
                        player = playerComponent;
                        break;
                    }
                }
            }

            if (playerAttack == null && player != null)
                playerAttack = player.GetComponent<PlayerAttack>();
        }

        public void PointerDownLeft()
        {
            if (player != null) player.mobileLeftPressed = true;
        }

        public void PointerUpLeft()
        {
            if (player != null) player.mobileLeftPressed = false;
        }

        public void PointerDownRight()
        {
            if (player != null) player.mobileRightPressed = true;
        }

        public void PointerUpRight()
        {
            if (player != null) player.mobileRightPressed = false;
        }

        public void PointerDownJump()
        {
            if (player != null) player.mobileJumpPressed = true;
        }

        public void PointerUpJump()
        {
            if (player != null) player.mobileJumpPressed = false;
        }

        public void PointerDownAttack()
        {
            if (player == null)
                return;

            if (playerAttack == null)
                playerAttack = player.GetComponent<PlayerAttack>();

            player.mobileAttackPressed = true;

            if (playerAttack != null)
                playerAttack.TryAttack();
            else
                player.Attack();
        }

        public void PointerUpAttack()
        {
            if (player != null)
                player.mobileAttackPressed = false;
        }
    }
}