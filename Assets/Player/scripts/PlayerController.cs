using UnityEngine;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Transform naprTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float offsetDistance = 0.5f; // Расстояние от игрока до объекта "napr"

    private NetworkIdentity networkIdentity;

    private void Awake()
    {
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    private void Update()
    {
        if (!networkIdentity.isOwned)
        {
            return;
        }

        MoveNaprWithPlayer();
        RotateNaprTowardsMouse();
    }

    private void MoveNaprWithPlayer()
    {
        if (naprTransform != null)
        {
            naprTransform.position = transform.position + new Vector3(0, offsetDistance, 0);
        }
        else
        {
            Debug.LogWarning("Napr Transform is not assigned.");
        }
    }

    private void RotateNaprTowardsMouse()
    {
        if (naprTransform == null || mainCamera == null)
        {
            Debug.LogWarning("Napr Transform or Main Camera is not assigned.");
            return;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, mainCamera.transform.position.y - transform.position.y));

        Vector2 direction = new Vector2(
            mousePosition.x - transform.position.x,
            mousePosition.y - transform.position.y
        );

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        naprTransform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f)); // Подкорректировали угол на 90 градусов, если вершина круга направлена вверх
    }
}
