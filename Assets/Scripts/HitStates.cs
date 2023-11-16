using Mirror;
using UnityEngine;

public class HitStates : NetworkBehaviour
{
    public bool isInHitBox;
    public bool isInChamberHit;
    public bool isInBlockHit;

    [SerializeField] private AttackManager playerAttackManager;
    [SerializeField] private PlayerController playerController;
  
    public void OnSwordHit(Collider2D other, AttackManager enemyAttack,float damage)
    {
        if (enemyAttack != null && (enemyAttack.isAttacking || enemyAttack.isThrustAttacking))
        {
            playerAttackManager.enemyAttack = enemyAttack;
            if (other.CompareTag("HitBox"))
            {
                playerController.HP -= damage;

                playerAttackManager.RpcAttackStopped(playerAttackManager);
                isInHitBox = true;
            }
            else if (other.CompareTag("HitBoxChamber"))
            {
                isInChamberHit = true;
            }
            else if (other.CompareTag("HitBoxBlock"))
            {
                isInBlockHit = true;
            }
        }
    }
    public void OnSwordExitFromTrigger(Collider2D other)
    {
        if (playerAttackManager.enemyAttack != null)
        {
            if (other.CompareTag("HitBoxChamber"))
            {
                isInChamberHit = false;
            }
            else if (other.CompareTag("HitBoxBlock"))
            {
                isInBlockHit = false;
            }
            else if (other.CompareTag("HitBox"))
            {
                isInHitBox = false;
            }

        }
    }
}
