using System.Collections;
using UnityEngine;
using Mirror;

public class AttackManager : NetworkBehaviour
{
    public bool attackFromRight;
    public bool isAttacking;
    [SerializeField] private bool isAutoAttacking = false;
    private bool isWindingUp;
    private bool isReturning = false;
    public bool isThrustAttacking = false;
    private bool isThrustWindingUp = false; 

    private float currentStep = 0f;
    private float startAngle;
    private float endAngle;
    [SyncVar]
    private float syncedArcAngle;

    public GameObject sword;
    public GameObject targetObject;

    public Camera mainCamera;
    public AttackManager enemyAttack;
    private IndicatorsDebug indicators;
    private HitStates hitStates;
    private DefenceManager defenceManager;
    private PlayerController playerController;

    [SerializeField]  private LineRenderer lineRenderer;

    private Coroutine swordAttacking;
    private Coroutine windUpCoroutine;
    [Header("Attack")]
    [SerializeField] private float windupTime;
    [SerializeField] private float arcAngle = 90f;
    [SerializeField] private float attackSpeed = 1f;
    [SerializeField] private float radius = 5f;
    [SerializeField] private float returnAttackTime = 0.5f;
    [SerializeField] private float attackStamina = 15;
    [Header("Thrust Attack")]
    [SerializeField] private float thrustWindupTime;
    [SerializeField] private float thrustDistance = 3f; // Расстояние колющей атаки
    [SerializeField] private float thrustSpeed = 2f;
    [SerializeField] private float thrustAttackStamina = 10;
    [SerializeField] private int segments = 50;
    private void Start()
    {
        indicators = GetComponent<IndicatorsDebug>();
        hitStates = GetComponent<HitStates>();
        defenceManager = GetComponent<DefenceManager>();
        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        if (Input.GetKeyDown(KeyCode.T)) //Press while attacking 
        {
            if (isAutoAttacking)
            {
                SetAuto(false);
                isAutoAttacking = false;
            }
            else
            {
                SetAuto(true);
                isAutoAttacking = true;
            }
        }
        if (!isAttacking && !isReturning && !isThrustAttacking)
        {
            if (!isAutoAttacking)
            {
                if (!isThrustWindingUp && !isWindingUp)
                {

                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        defenceManager.CmdBlocking(true);
                    }
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.transform.position.y - targetObject.transform.position.y));
                    Vector2 direction = new Vector2(
                    mousePosition.x - targetObject.transform.position.x,
                    mousePosition.y - targetObject.transform.position.y
                          );
                    syncedArcAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    CmdSyncArcAngle(syncedArcAngle);
                }
                    if (Input.GetMouseButtonDown(0) && !isWindingUp)
                    {

                        if (!isThrustWindingUp)
                            CmdStartAttack(false, false);
                        else
                        {
                            CmdAttackCancel(false);
                            CmdStartAttack(false, true);
                        }
                    }
                    else if (Input.GetMouseButtonDown(1) && !isWindingUp)
                    {
                        if (!isThrustWindingUp)
                            CmdStartAttack(true, false);
                        else
                        {
                            CmdAttackCancel(false);
                            CmdStartAttack(true, true);
                        }
                    }

                    if (Input.GetAxis("Mouse ScrollWheel") > 0f && !isThrustWindingUp)
                    {
                        if (!isWindingUp)
                            CmdStartThrustAttack();
                        else
                        {
                            CmdAttackCancel(true);
                            CmdStartThrustAttack();
                        }
                    }
            }
            else
            {
                if (!isThrustWindingUp && !isWindingUp)
                    CmdStartThrustAttack();
            }
        }
        else
        {
            if(Input.GetKeyDown(KeyCode.T)) //Press while attacking 
            {
                if (isAutoAttacking) {
                    SetAuto(false);
                    isAutoAttacking = false;
                        }
                else
                {
                    SetAuto(true);
                    isAutoAttacking = true;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isWindingUp)
            {
                CmdAttackCancel(true);
            }
            else if (isThrustWindingUp)
            {
                CmdAttackCancel(false);
            }
        }
    }
    #region TestAutoAttack
    [ClientRpc]
    private void SetAuto(bool auto) { isAutoAttacking = auto; }
    #endregion
    [Command]
    private void CmdAttackCancel(bool isDefault)
    {
        if (isDefault)
        {
            isWindingUp = false;
            isAttacking = false;
        }
        else
        {
            isThrustWindingUp = false;
            isThrustAttacking = false;
        }
        RpcAttaclCancel(isDefault);
    }
    [ClientRpc]
    private void RpcAttaclCancel(bool isDefault)
    {
        if (isDefault)
        {
            isWindingUp = false;
            isAttacking = false;
            indicators.AttackWindUpIndicators(attackFromRight, false);
        }
        else
        {
            indicators.ThrustAttackWindUpIndicators(false);
            isThrustAttacking = false;
            isThrustWindingUp = false;

        }
        StopCoroutine(windUpCoroutine);
    }
    #region AttackBlocked
    [ClientRpc]
    public void RpcAttackStopped(AttackManager enemy) //метод для остановки и отката атаки 
    {
        Debug.Log("AttackStoped");
        enemy.StopCoroutine(enemy.swordAttacking);
        //enemy.StartCoroutine(enemy.ReturnAttack());
        enemy.StartCoroutine(enemy.SwordBackup());
        enemy.isAttacking = false;
        enemy.isThrustAttacking =false;
        enemy.currentStep = 0;
    }
    private IEnumerator SwordBackup()
    {
        if (sword == null) yield break;

        //while (currentStep <= segments)
        //{
        //    //SwordMoving(angleStep, !attackFromRight, backupAttackSpeed);
        //    yield return null;
        //}
        currentStep = 0;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        isThrustAttacking = false;
        //isBlocked = false;
    }
    #endregion
    #region DefaultAttack
    [Command]
    public void CmdSyncArcAngle(float newArcAngle)
    {
        syncedArcAngle = newArcAngle;
    }
    [Command]
    private void CmdStartAttack(bool attackFromRight, bool isRedirected)
    {
        if (playerController.Stamina < attackStamina) return;
        this.attackFromRight = attackFromRight;
        playerController.Stamina -= attackStamina;
        if (hitStates.isInChamberHit && attackFromRight != enemyAttack.attackFromRight && enemyAttack != null && !hitStates.isInHitBox)
        {           
            RpcAttackStopped(enemyAttack);
        }
        isWindingUp = true;
        RpcStartAttack(attackFromRight, isWindingUp);
    }
    [Command]
    private void CmdStartThrustAttack()
    {
        if (playerController.Stamina < thrustAttackStamina) return;
        playerController.Stamina -= thrustAttackStamina;
        if (hitStates.isInChamberHit && enemyAttack.isThrustAttacking && enemyAttack != null && !hitStates.isInHitBox)
            {
            RpcAttackStopped(enemyAttack);
            isThrustWindingUp = false;
            //this.isThrustAttacking = true;
            //RpcStartThrustAttack(isThrustWindingUp);
            }
        isThrustWindingUp = true;
        RpcStartThrustAttack(isThrustWindingUp);
    }
    [ClientRpc]
    private void RpcStartAttack(bool attackFromRight, bool isWindingUp)
    {
        this.attackFromRight = attackFromRight;
        this.isWindingUp = isWindingUp;
        indicators.AttackDirrection(attackFromRight);
        indicators.AttackWindUpIndicators(attackFromRight, isWindingUp);
        if (isWindingUp)
        {
            windUpCoroutine = StartCoroutine(WindUpAttack());
        }
        else
        {
            isAttacking = true;
            swordAttacking = StartCoroutine(SwordAttack());
        }
    }
    [ClientRpc]
    private void RpcStartThrustAttack(bool isWindingUp)
    {
        isThrustWindingUp = isWindingUp;
        indicators.ThrustAttackWindUpIndicators(isThrustWindingUp);
        if (isWindingUp)
        {
            windUpCoroutine = StartCoroutine(WindUpAttack(false));
        }
        else
        {
            this.isThrustAttacking = true;
            swordAttacking = StartCoroutine(SwordAttack(false));
        }
    }
    private void LateUpdate()
    {
        UpdateArc();
    }
    public void SetArc(float newArcAngle)
    {
        arcAngle = newArcAngle;
        UpdateArc();
    }

    private void UpdateArc()
    {
        if (targetObject == null || mainCamera == null) return;

        float angle = syncedArcAngle;
        startAngle = angle - arcAngle / 2f;
        endAngle = angle + arcAngle / 2f;

        lineRenderer.positionCount = segments + 1;
        float angleStep = arcAngle / segments;

        Vector3 targetPosition = targetObject.transform.position;

        for (int i = 0; i <= segments; i++)
        {
            float segmentAngle = Mathf.Deg2Rad * (startAngle + i * angleStep);
            float x = Mathf.Cos(segmentAngle) * radius;
            float y = Mathf.Sin(segmentAngle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, y, 0) + targetPosition);
        }
    }

    private void SwordMoving(float angleStep, bool isFromRight, float duration)
    {
        float segmentAngle;
        if (isFromRight)
        {
            segmentAngle = Mathf.Deg2Rad * (startAngle + currentStep * angleStep);
        }
        else
        {
            segmentAngle = Mathf.Deg2Rad * (endAngle - currentStep * angleStep);
        }
        float x = Mathf.Cos(segmentAngle) * radius;
        float y = Mathf.Sin(segmentAngle) * radius;
        sword.transform.position = new Vector3(x, y, 0) + targetObject.transform.position;
        sword.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Rad2Deg * segmentAngle) - 90f);

        currentStep += duration * Time.deltaTime; // Изменение здесь
    }
    private IEnumerator WindUpAttack(bool isDefaultAttack = true)
    {
        if (isDefaultAttack)
        {
            yield return new WaitForSeconds(windupTime);
            if (isWindingUp) // Добавьте эту проверку
            {
                isWindingUp = false;
                isAttacking = true;
                RpcStartAttack(attackFromRight, isWindingUp);
            }
        }
        else
        {
            yield return new WaitForSeconds(thrustWindupTime);
            if (isThrustWindingUp)
            {
                isThrustWindingUp = false;
                isThrustAttacking = true;
                RpcStartThrustAttack(isThrustWindingUp);
            }
        }
    }
    private IEnumerator SwordAttack(bool isDefaultAttack = true)
    {
        if (sword  == null) yield break;
        if (isDefaultAttack)
        {
            if (!isAttacking) yield break;
            isAttacking = true;
            float angleStep = arcAngle / segments;

            while (currentStep <= segments && isAttacking)
            {
                SwordMoving(angleStep, attackFromRight, attackSpeed);
                yield return null;
            }
            if (isAttacking)
            {
                StartCoroutine(ReturnAttack());
            }
        }
        else 
        {
            if (!isThrustAttacking) yield break;
            float middleAngle = Mathf.Deg2Rad * syncedArcAngle;
            float startX = Mathf.Cos(middleAngle) * radius;
            float startY = Mathf.Sin(middleAngle) * radius;
            sword.transform.position = new Vector3(startX, startY, 0) + targetObject.transform.position;
            sword.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Rad2Deg * middleAngle) - 90f);

            Vector3 originalPosition = sword.transform.position;
            Vector3 targetPosition = originalPosition + sword.transform.up * thrustDistance;

            float journeyLength = Vector3.Distance(originalPosition, targetPosition);
            float startTime = Time.time;
            float distanceCovered = 0;
            while (distanceCovered < journeyLength)
            {
                originalPosition = new Vector3(startX, startY, 0) + targetObject.transform.position;
                targetPosition = originalPosition + sword.transform.up * thrustDistance;

                float fracJourney = distanceCovered / journeyLength;
                sword.transform.position = Vector3.Lerp(originalPosition, targetPosition, fracJourney);
                distanceCovered = (Time.time - startTime) * thrustSpeed;
                yield return null;
            }

            sword.transform.position = targetPosition;

            while (distanceCovered > 0)
            {
                originalPosition = new Vector3(startX, startY, 0) + targetObject.transform.position;
                targetPosition = originalPosition + sword.transform.up * thrustDistance;

                float fracJourney = distanceCovered / journeyLength;
                sword.transform.position = Vector3.Lerp(originalPosition, targetPosition, fracJourney);
                distanceCovered -= (Time.time - startTime) * thrustSpeed;
                yield return null;
            }
            sword.transform.position = originalPosition;
            isThrustAttacking = false;
        }
    }
    private IEnumerator ReturnAttack()
    {
        isReturning = true;
        isAttacking = false;
        yield return new WaitForSeconds(returnAttackTime);
        isReturning = false;
        currentStep = 0;
    }
    #endregion
}
