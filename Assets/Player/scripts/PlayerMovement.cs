using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private NetworkIdentity networkIdentity;

    private bool isDirectionalMovement = false; // Переменная для проверки режима движения
    [SerializeField] private float backwardSpeed = 2.5f; // Скорость движения назад
    [SerializeField] private float sideStepSpeed = 3f; // Скорость движения вбок
    [SerializeField] private bool isRunning = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }

    public Vector2 GetCurrentMovement()
    {
        return movement;
    }

    private void Update()
    {
        if (!networkIdentity.isOwned)
        {
            return;
        }
        if(Input.GetKeyDown(KeyCode.LeftShift))
            isRunning = true;
        else if(Input.GetKeyUp(KeyCode.LeftShift))
            isRunning = false;
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            isDirectionalMovement = !isDirectionalMovement; // Переключение режима движения
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (isDirectionalMovement)
        {
            Vector2 directionToMouse = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - rb.position;
            directionToMouse.Normalize();

            Vector2 forwardMovement = Vector2.zero;
            if (moveY > 0) // Движение вперед
            {
                forwardMovement = moveY * directionToMouse;
            }
            else if (moveY < 0) // Движение назад
            {
                forwardMovement = moveY * backwardSpeed * directionToMouse;
            }

            Vector2 perpendicular = new Vector2(directionToMouse.y, -directionToMouse.x); // Инвертированное перпендикулярное направление
            Vector2 sideMovement = moveX * sideStepSpeed * perpendicular;

            movement = (forwardMovement + sideMovement).normalized;
            movement *= isRunning ? runSpeed : walkSpeed;
        }
        else
        {
            movement = new Vector2(moveX, moveY).normalized;
            movement *= isRunning ? runSpeed : walkSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (!networkIdentity.isOwned)
        {
            return;
        }

        MoveCharacter();
    }

    private void MoveCharacter()
    {
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }
}
