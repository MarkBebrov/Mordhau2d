using UnityEngine;
using System.Collections;
using Mirror;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class ArcController : NetworkBehaviour
{
    public GameObject lefto;
    public GameObject righto;

    public GameObject targetObject;
    public Camera mainCamera;
    public GameObject sword;
    public float radius = 5f;
    public int segments = 50;
    public float arcAngle = 90f;
    public float attackSpeed = 1f;

    private LineRenderer lineRenderer;
    private float startAngle;
    private float endAngle;

    public GameObject healthBar;
    public float maxHealth = 100f;

    [SyncVar]
    private float currentHealth;

    [SyncVar]
    private bool isAttacking = false;

    [SyncVar]
    private bool attackFromRight = true;

    [SyncVar]
    private float syncedArcAngle;

    [SerializeField]
    private float swordDamage = 10f; // ���� �� ����, ������ ����� ������������� � ����������

    private HashSet<GameObject> hitObjectsDuringCurrentAttack;

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


    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collision detected with: " + other.gameObject.name);
        if (isAttacking && !hitObjectsDuringCurrentAttack.Contains(other.gameObject))
        {
            Debug.Log("Collision detected with: " + other.gameObject.tag); // ���������
            if (other.CompareTag("HitBox"))
            {
                ArcController enemyArcController = other.GetComponentInParent<ArcController>();
                if (enemyArcController != null)
                {
                    enemyArcController.CmdChangeHealth(-swordDamage); // ���������� ���������� swordDamage ��� �����
                    hitObjectsDuringCurrentAttack.Add(other.gameObject);
                }
            }
            else if (other.CompareTag("HitBoxChamber"))
            {
                ArcController enemyArcController = other.GetComponentInParent<ArcController>();
                if (enemyArcController != null && enemyArcController.isAttacking)
                {
                    if (attackFromRight != enemyArcController.attackFromRight)
                    {
                        enemyArcController.StopCoroutine(enemyArcController.SwordAttack());
                        enemyArcController.isAttacking = false;
                        enemyArcController.UpdateAttackIndicators();

                        StartCoroutine(SwordAttack());
                        hitObjectsDuringCurrentAttack.Add(other.gameObject);
                    }
                }
            }
        }
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!isAttacking)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                CmdStartAttack(true);
            }
            else if (Input.GetKeyDown(KeyCode.Z))
            {
                CmdStartAttack(false);
            }
        }

        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            CmdStartAttack(attackFromRight);
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


    [Command]
    public void CmdSyncArcAngle(float newArcAngle)
    {
        syncedArcAngle = newArcAngle;
    }

    [Command]
    public void CmdStartAttack(bool attackFromRight)
    {
        this.attackFromRight = attackFromRight;
        isAttacking = true;
        RpcStartAttack(attackFromRight);
    }

    [ClientRpc]
    public void RpcStartAttack(bool attackFromRight)
    {
        this.attackFromRight = attackFromRight;
        isAttacking = true;
        UpdateAttackIndicators();
        StartCoroutine(SwordAttack());
    }


    private void UpdateAttackIndicators()
    {
        if (attackFromRight)
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
        }
        else
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
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


    private IEnumerator SwordAttack()
    {

        if (sword == null) yield break;

        isAttacking = true;
        hitObjectsDuringCurrentAttack.Clear();

        // ������������� ����� ����������� � ����������� �� ����������� �����
        if (attackFromRight)
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // 50% ������������
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f); // 50% ������������
        }
        else
        {
            lefto.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 0.5f); // 50% ������������
            righto.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.5f); // 50% ������������
        }

        float angleStep = arcAngle / segments;
        float currentStep = 0f;

        while (currentStep <= segments)
        {
            float segmentAngle;
            if (attackFromRight)
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

            currentStep += attackSpeed;
            yield return null;
        }

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
