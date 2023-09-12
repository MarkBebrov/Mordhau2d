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

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        networkIdentity = GetComponent<NetworkIdentity>();
    }



    private void Update()
    {
        if (!networkIdentity.isOwned)
        {
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        movement = new Vector2(moveX, moveY);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement = movement.normalized * runSpeed;
        }
        else
        {
            movement = movement.normalized * walkSpeed;
        }
    }

    private void FixedUpdate()
    {
        if (!networkIdentity.isOwned)
        {
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movement = new Vector2(moveX, moveY);

        if (Input.GetKey(KeyCode.LeftShift))
        {
            movement = movement.normalized * runSpeed;
        }
        else
        {
            movement = movement.normalized * walkSpeed;
        }

        MoveCharacter();
    }

    private void MoveCharacter()
    {
        rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
    }
}
