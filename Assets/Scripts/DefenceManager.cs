using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DefenceManager : NetworkBehaviour
{
    private bool canBlock = true;

    [SerializeField] private GameObject sword;

    private HitStates hitStates;
    private AttackManager attackManager;
    private PlayerController playerController;

    private Coroutine blockCooldownCoroutine;
    [Header("Block Settings")]
    [SerializeField] private float blockCooldown = 2f;
    [SerializeField] private float maxBlockTime = 3f;
    [SerializeField] private float blockStamina = 6f;
    private void Start()
    {
        hitStates = GetComponent<HitStates>();
        attackManager = GetComponent<AttackManager>();
        playerController = GetComponent<PlayerController>();
    }

    [Command]
    public void CmdBlocking(bool isBlock)
    {
        RpcBlocking(isBlock);
    }
    [ClientRpc]
    public void RpcBlocking(bool isBlock)
    {
        if (isBlock && canBlock && !hitStates.isInHitBox)
        {
            playerController.Stamina -= blockStamina;
            sword.GetComponent<SpriteRenderer>().color = Color.blue;
            if (hitStates.isInBlockHit)
                attackManager.RpcAttackStopped(attackManager.enemyAttack);

            if (blockCooldownCoroutine != null)
            {
                StopCoroutine(blockCooldownCoroutine);
            }

            blockCooldownCoroutine = StartCoroutine(BlockCooldownRoutine());
            StartCoroutine(BlockDurationRoutine());
        }
        else
        {
            sword.GetComponent<SpriteRenderer>().color = Color.white;
        }
    }
    IEnumerator BlockCooldownRoutine()
    {
        canBlock = false;
        yield return new WaitForSeconds(blockCooldown);
        canBlock = true;
    }
    private IEnumerator BlockDurationRoutine()
    {
        yield return new WaitForSeconds(maxBlockTime);
        CmdBlocking(false);
    }
}
