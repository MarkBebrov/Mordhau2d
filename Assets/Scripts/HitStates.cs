using Mirror;
using UnityEngine;

public class HitStates : NetworkBehaviour
{
    public bool isInHitBox;
    public bool isInChamberHit;
    public bool isInBlockHit;

    public AttackManager playerAttackManager;
  
    public void OnSwordHit(Collider2D other, AttackManager enemyAttack)
    {
        //Debug.Log("Collision detected with " + other.tag); // Добавлено для отладки

        if (enemyAttack != null && (enemyAttack.isAttacking || enemyAttack.isThrustAttacking))
        {
            playerAttackManager.enemyAttack = enemyAttack;
            if (other.CompareTag("HitBox"))
            {
                //ChangeStamina(10f);
                //Debug.Log("HitBox collision detected"); // Добавлено для отладки
                //CmdChangeHealth(-swordDamage);
                isInHitBox = true;
                //enemyArcController.hitObjectsDuringCurrentAttack.Add(other.gameObject);
            }
            else if (other.CompareTag("HitBoxChamber"))
            {
                //Debug.Log("HitBoxChamber collision detected"); // Добавлено для отладки
                isInChamberHit = true;
            }
            else if (other.CompareTag("HitBoxBlock"))
            {
                //Debug.Log("HitBoxBlock collision detected");
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
