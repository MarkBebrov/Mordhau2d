using UnityEngine;
using System.Collections;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ArcController : NetworkBehaviour
{
    public GameObject lefto;
    public GameObject righto;

    public GameObject leftSwing;
    public GameObject rightSwing;
    public GameObject CentralSwing;


    public GameObject targetObject;
    public Camera mainCamera;
    public GameObject sword;
    public float radius = 5f;
    public int segments = 50;
    public float arcAngle = 90f;
    public float attackSpeed = 1f;
    public float backupAttackSpeed = 1f;

    private LineRenderer lineRenderer;
    private float startAngle;
    private float endAngle;
    private float currentStep = 0f; //была локальной

    public GameObject healthBar;
    public float maxHealth = 100f;

    [Header("Block Settings")]
    public float blockCooldown = 2f; // Время ожидания после блокирования
    private bool canBlock = true; // Переменная для проверки, может ли игрок блокировать
    public float maxBlockTime = 3f; // Максимальное время блокирования
    private float blockTimer = 0f; // Таймер для блокирования

    [Header("Thrust Attack Settings")]
    public float thrustDistance = 3f; // Расстояние колющей атаки
    public float thrustSpeed = 2f; // Скорость колющей атаки
    private bool isThrusting = false; // Проверка, выполняется ли колющая атака

    [Header("Thrust Windup Settings")]
    public float thrustWindupTime = 1f; // Время замаха для колющей атаки
    [SyncVar]
    private bool isThrustWindingUp = false; // Проверка, находится ли игрок в состоянии замаха для колющей атаки




    [ClientRpc]
    public void RpcUpdateAttackIndicators()
    {
        UpdateAttackIndicators();
    }

    [Command]
    public void CmdCancelAttack()
    {
        RpcCancelAttack();
    }

    [ClientRpc]
    public void RpcCancelAttack()
    {
        CancelAttack();
    }

    [Command]
    public void CmdAttackStopped(ArcController enemy)
    {
        RpcAttackStopped(enemy);
    }

    [ClientRpc]
    public void RpcAttackStopped(ArcController enemy)
    {
        AttackStopped(enemy);
    }

    private Coroutine blockCooldownCoroutine;




    [SyncVar]
    private float currentHealth;

    [SyncVar]
    private bool isAttacking = false;

    [SyncVar]
    private bool isBlocking = false;

    [SyncVar]
    private bool attackFromRight = true;

    [SyncVar]
    public bool isAIPlayer; //для тестов

    [SyncVar]
    private bool isBlocked = false; //для тестов

    [SyncVar]
    private float syncedArcAngle;

    [SyncVar]
    private bool isInChamberHit = false;

    [SyncVar]
    private bool isInBlockHit = false;

    [SerializeField]
    private float swordDamage = 10f; // ���� �� ����, ������ ����� ������������� � ����������

    [Header("Attack Settings")]
    public float windupTime = 1f; // Время замаха
    [SyncVar]
    private bool isWindingUp = false; // Проверка, находится ли игрок в состоянии замаха

    private HashSet<GameObject> hitObjectsDuringCurrentAttack;

    private Coroutine swordAttacking; //Чтобы останавливать анимацию в 100 строке

    private ArcController enemyArcController;
    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        hitObjectsDuringCurrentAttack = new HashSet<GameObject>();
    }

    private void UpdateHealthBar()
    {
        float healthPercentage = currentHealth / maxHealth;
        Debug.Log("Updating health bar: " + healthPercentage); // ���������
        healthBar.transform.localScale = new Vector3(healthPercentage, healthBar.transform.localScale.y, healthBar.transform.localScale.z);
    }

    public void OnSwordHit(Collider2D other, ArcController enemyArcController)
    {
        Debug.Log("Collision detected with " + other.tag); // Добавлено для отладки

        if (enemyArcController != null && enemyArcController.isAttacking && !enemyArcController.hitObjectsDuringCurrentAttack.Contains(other.gameObject))
        {
            this.enemyArcController = enemyArcController;
            if (other.CompareTag("HitBox"))
            {
                Debug.Log("HitBox collision detected"); // Добавлено для отладки
                CmdChangeHealth(-swordDamage);
                enemyArcController.hitObjectsDuringCurrentAttack.Add(other.gameObject);
            }
            else if (other.CompareTag("HitBoxChamber"))
            {
                Debug.Log("HitBoxChamber collision detected"); // Добавлено для отладки
                isInChamberHit = true;
            }
            else if (other.CompareTag("HitBoxBlock"))
            {
                Debug.Log("HitBoxBlock collision detected");
                isInBlockHit = true;
            }
        }
    }
    public void OnSwordExitFromTrigger(Collider2D other)
    {
        if (enemyArcController != null)
        {
            if (other.CompareTag("HitBoxChamber"))
            {
                isInChamberHit = false;
            }
            else if (other.CompareTag("HitBoxBlock"))
            {
                isInBlockHit = false;
            }

        }
    }
    private void AttackStopped(ArcController enemy) //метод для остановки и отката атаки 
    {
        Debug.Log("AttackStoped");
        enemy.StopCoroutine(enemy.swordAttacking);
        enemy.UpdateAttackIndicators();
        enemy.isBlocked = true;
        enemy.StartCoroutine(enemy.SwordBackup());
        // Изменяем цвет объекта игрока на желтый на секунду
        StartCoroutine(ChangeColorForChamber());
    }

    private IEnumerator ChangeColorForChamber()
    {
        // Получаем компонент SpriteRenderer объекта игрока
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Изменяем цвет на желтый
            spriteRenderer.color = Color.yellow;

            // Ждем секунду
            yield return new WaitForSeconds(1f);

            // Возвращаем обратно оригинальный цвет
            spriteRenderer.color = Color.white;
        }
    }


    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!isAIPlayer)
        {
            if (!isLocalPlayer)
            {
                return;
            }

            if (!isAttacking && !isWindingUp && !isThrusting && !isThrustWindingUp)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    attackFromRight = false;
                    CmdStartAttack(attackFromRight);
                    UpdateAttackIndicators();
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    attackFromRight = true;
                    CmdStartAttack(attackFromRight);
                    UpdateAttackIndicators();
                }
                else if (Input.GetAxis("Mouse ScrollWheel") > 0f)
                {
                    StartCoroutine(ThrustAttack());
                }
                else if (Input.GetKeyDown(KeyCode.F) && blockTimer < maxBlockTime)
                {
                    CmdBlocking(true);
                }
            }

            if (isWindingUp && Input.GetKeyDown(KeyCode.X))
            {
                CancelAttack();
            }

            if (isThrustWindingUp && Input.GetKeyDown(KeyCode.X))
            {
                StopCoroutine(ThrustAttack());
                isThrustWindingUp = false;
                CentralSwing.GetComponent<SpriteRenderer>().color = Color.white;
            }

            Vector3 mousePosition = Input.mousePosition;
            mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.transform.position.y - targetObject.transform.position.y));

            Vector2 direction = new Vector2(
                mousePosition.x - targetObject.transform.position.x,
                mousePosition.y - targetObject.transform.position.y
            );

            syncedArcAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            CmdSyncArcAngle(syncedArcAngle);

            if (sword.GetComponent<SpriteRenderer>().color == Color.blue)
            {
                blockTimer += Time.deltaTime;
            }
            else
            {
                blockTimer = 0f;
            }
        }
        else
        {
            if (!isAttacking && !isBlocked)
            {
                CmdStartAttack(false);
            }
        }
    }





    public void CancelAttack()
    {
        if (isWindingUp)
        {
            StopCoroutine(WindupAttack());
            isWindingUp = false;
        }

        if (isAttacking)
        {
            if (swordAttacking != null)
            {
                StopCoroutine(swordAttacking);
            }
            isAttacking = false;
        }

        UpdateAttackIndicators();
    }


    //[Command]
    public void CmdStartAttack(bool attackFromRight)
    {
        Debug.Log("CmdStartAttack called for " + (isAIPlayer ? "AI" : "Player"));
        this.attackFromRight = attackFromRight;

        if (isInChamberHit && attackFromRight == enemyArcController.attackFromRight && enemyArcController != null)
        {
            Debug.Log("Chamber detected");
            AttackStopped(enemyArcController);
            isWindingUp = false;
            isAttacking = true;
            RpcStartAttack(attackFromRight);
            RpcUpdateAttackIndicators();
        }
        else
        {
            isWindingUp = true;
            RpcUpdateAttackIndicators();
            RpcStartWindupAttack();
        }
    }

    [ClientRpc]
    public void RpcStartWindupAttack()
    {
        StartCoroutine(WindupAttack());
    }





    [Command]
    public void CmdBlocking(bool isBlock)
    {
        if (isBlock && canBlock)
        {
            sword.GetComponent<SpriteRenderer>().color = Color.blue;
            if (isInBlockHit)
                AttackStopped(enemyArcController);

            // Если корутина уже запущена, останавливаем её
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

    private IEnumerator BlockDurationRoutine()
    {
        yield return new WaitForSeconds(maxBlockTime);
        CmdBlocking(false);
    }

    private IEnumerator ThrustAttack()
    {
        isThrustWindingUp = true;
        CentralSwing.GetComponent<SpriteRenderer>().color = Color.blue;

        yield return new WaitForSeconds(thrustWindupTime);

        if (isThrustWindingUp)
        {
            isThrustWindingUp = false;
            isThrusting = true;
            CentralSwing.GetComponent<SpriteRenderer>().color = Color.white;

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
            isThrusting = false;
        }
    }



    [Command]
    public void CmdSyncArcAngle(float newArcAngle)
    {
        syncedArcAngle = newArcAngle;
    }

    //[Command] 
    //public void CmdStartAttack(bool attackFromRight)
    //{
    //        this.attackFromRight = attackFromRight;
    //        isAttacking = true;
    //        RpcStartAttack(attackFromRight);
    //}

    [ClientRpc]
    public void RpcStartAttack(bool attackFromRight)
    {
        this.attackFromRight = attackFromRight;
        isAttacking = true;
        UpdateAttackIndicators();
        swordAttacking = StartCoroutine(SwordAttack());
    }

    private void UpdateAttackIndicators()
    {
        if (attackFromRight)
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
            rightSwing.GetComponent<SpriteRenderer>().color = isWindingUp ? Color.blue : Color.white; // Изменено местами
            leftSwing.GetComponent<SpriteRenderer>().color = Color.white; // Изменено местами
        }
        else
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            leftSwing.GetComponent<SpriteRenderer>().color = isWindingUp ? Color.blue : Color.white; // Изменено местами
            rightSwing.GetComponent<SpriteRenderer>().color = Color.white; // Изменено местами
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

    private IEnumerator BlockCooldownRoutine()
    {
        canBlock = false;
        yield return new WaitForSeconds(blockCooldown);
        canBlock = true;
    }


    private IEnumerator WindupAttack()
    {
        // Устанавливаем индикаторы замаха
        RpcUpdateAttackIndicators(); // Обновляем индикаторы на всех клиентах

        yield return new WaitForSeconds(windupTime);

        if (isWindingUp) // Добавьте эту проверку
        {
            isWindingUp = false;
            isAttacking = true;
            RpcStartAttack(attackFromRight);
            RpcUpdateAttackIndicators(); // Обновляем индикаторы на всех клиентах после завершения замаха
        }
    }



    private IEnumerator SwordBackup()
    {
        if (sword == null) yield break;
        float angleStep = arcAngle / segments;

        //while (currentStep <= segments)
        //{
        //    //SwordMoving(angleStep, !attackFromRight, backupAttackSpeed);
        //    yield return null;
        //}
        currentStep = 0;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
        isBlocked = false;
    }
    private IEnumerator SwordAttack()
    {
        if (!isAttacking || sword == null) yield break;

        isAttacking = true;
        hitObjectsDuringCurrentAttack.Clear();

        // Устанавливаем цвета индикаторов в зависимости от направления атаки
        if (attackFromRight)
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // 50% прозрачность
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f); // 50% прозрачность
        }
        else
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f); // 50% прозрачность
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // 50% прозрачность
        }

        float angleStep = arcAngle / segments;

        while (currentStep <= segments && isAttacking)
        {
            //float segmentAngle;
            //if (attackFromRight)
            //{
            //    segmentAngle = Mathf.Deg2Rad * (startAngle + currentStep * angleStep);
            //}
            //else
            //{
            //    segmentAngle = Mathf.Deg2Rad * (endAngle - currentStep * angleStep);
            //}

            //float x = Mathf.Cos(segmentAngle) * radius;
            //float y = Mathf.Sin(segmentAngle) * radius;
            //sword.transform.position = new Vector3(x, y, 0) + targetObject.transform.position;
            //sword.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Rad2Deg * segmentAngle) - 90f);

            //currentStep += attackSpeed * Time.deltaTime; // Изменение здесь

            SwordMoving(angleStep, attackFromRight, attackSpeed); //Сделал метод, который двигает меч, тк данный блок нужен будет для отката атаки
            yield return null;
        }
        currentStep = 0;
        isAttacking = false;
        UpdateAttackIndicators();
    }

    [Command]
    public void CmdChangeHealth(float amount)
    {
        currentHealth += amount;
        Debug.Log("Health changed: " + currentHealth);
        UpdateHealthBar();
        if (currentHealth <= 0)
        {
            // �������� ����� ������ ������
        }
    }
}