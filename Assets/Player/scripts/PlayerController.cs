using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform naprTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float offsetDistance = 0.5f;
    [SerializeField] private float staminaRecoveryDuration = 0.2f;
    [SyncVar]
    private float stamina = 100;
    public float Stamina
    {
        get { return stamina; }
        set 
        { 
            stamina = value;
            RpcStaminaSet(stamina);
            StartCoroutine(StaminaRecover());
        }
    }
    private float hp = 100;
    public float HP
    {
        get { return hp; }
        set 
        { 
            hp = value; 
            RpcHealthSet(hp);
        }
    }

    private NetworkIdentity networkIdentity;
    private IndicatorsDebug indicators;

    private void Awake()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
        indicators = GetComponent<IndicatorsDebug>();
    }
    [ClientRpc]
    private void RpcStaminaSet(float stamina)
    {
        this.stamina = stamina;
        indicators.StaminaChanger(stamina);
    }
    [ClientRpc]
    private void RpcHealthSet(float hp)
    {
        this.hp = hp;
        indicators.HealthChanger(hp);
    }
    //private void Update()
    //{
    //    if (!networkIdentity.isOwned)
    //    {
    //        return;
    //    }

    //    MoveNaprWithPlayer();
    //    RotateNaprTowardsMouse();
    //}

    private void MoveNaprWithPlayer()
    {
        //if (naprTransform != null)
        //{
        //    naprTransform.position = transform.position + new Vector3(0, offsetDistance, 0);
        //}
        //else
        //{
        //    Debug.LogWarning("Napr Transform is not assigned.");
        //}
    }

    private void RotateNaprTowardsMouse()
    {
        //if (naprTransform == null || mainCamera == null)
        //{
        //    Debug.LogWarning("Napr Transform or Main Camera is not assigned.");
        //    return;
        //}

        //Vector3 mousePosition = Input.mousePosition;
        //mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.transform.position.y - transform.position.y));

        //Vector2 direction = new Vector2(
        //    mousePosition.x - transform.position.x,
        //    mousePosition.y - transform.position.y
        //);

        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        //naprTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); // Подкорректировали угол на 90 градусов, если вершина круга направлена вверх
    }
    private IEnumerator StaminaRecover()
    {
        while (stamina < 100)
        {
            yield return new WaitForSeconds(staminaRecoveryDuration);
            stamina ++;
            RpcStaminaSet(stamina);
        }
    }
    //private IEnumerator StaminaMinus()
    //{

    //}
}
