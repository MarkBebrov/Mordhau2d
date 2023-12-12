using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : NetworkBehaviour
{
	[SerializeField] private float walkSpeed = 5f;
	[SerializeField] private float runSpeed = 10f;

	private const float DIRECTION_THRESHOLD = 0.1f;

	private Rigidbody2D rb;
	private Vector2 movement;
	private NetworkIdentity networkIdentity;

	private bool isDirectionalMovement = false;
	[SerializeField] private float backwardSpeed = 2.5f;
	[SerializeField] private float sideStepSpeed = 3f;
	[SerializeField] private bool isRunning = false;

	private Animator animator;

	public enum Direction { Up, Down, Left, Right }
	private Direction lastDirection = Direction.Down;
	private AttackManager attackManager;
	private void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		networkIdentity = GetComponent<NetworkIdentity>();
		animator = GetComponent<Animator>();
		attackManager = GetComponent<AttackManager>();
	}

	public Vector2 GetCurrentMovement()
	{
		return movement;
	}

	private void Update()
	{
		if (!networkIdentity.isOwned || attackManager.isAttacking || attackManager.isWindingUp || attackManager.isThrustWindingUp)
		{
			return;
		}

		if (Input.GetKeyDown(KeyCode.LeftShift))
			isRunning = true;
		else if (Input.GetKeyUp(KeyCode.LeftShift))
			isRunning = false;

		if (Input.GetKeyDown(KeyCode.LeftAlt))
			isDirectionalMovement = !isDirectionalMovement;

		float moveX = Input.GetAxisRaw("Horizontal");
		float moveY = Input.GetAxisRaw("Vertical");

		if (isDirectionalMovement)
		{
			Vector2 directionToMouse = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition) - rb.position;
			directionToMouse.Normalize();

			Vector2 forwardMovement = Vector2.zero;
			if (moveY > 0)
				forwardMovement = moveY * directionToMouse;
			else if (moveY < 0)
				forwardMovement = moveY * backwardSpeed * directionToMouse;

			Vector2 perpendicular = new Vector2(directionToMouse.y, -directionToMouse.x);
			Vector2 sideMovement = moveX * sideStepSpeed * perpendicular;

			movement = (forwardMovement + sideMovement).normalized;
			movement *= isRunning ? runSpeed : walkSpeed;
		}
		else
		{
			movement = new Vector2(moveX, moveY).normalized;
			movement *= isRunning ? runSpeed : walkSpeed;
		}

		if (Mathf.Abs(movement.x) > DIRECTION_THRESHOLD || Mathf.Abs(movement.y) > DIRECTION_THRESHOLD)
		{
			if (movement.x > DIRECTION_THRESHOLD)
			{
				lastDirection = Direction.Right;
			}
			else if (movement.x < -DIRECTION_THRESHOLD)
			{
				lastDirection = Direction.Left;
			}
			else if (movement.y > DIRECTION_THRESHOLD)
			{
				lastDirection = Direction.Up;
			}
			else if (movement.y < -DIRECTION_THRESHOLD)
			{
				lastDirection = Direction.Down;
			}
		}

		animator.SetFloat("MoveX", movement.x);
		animator.SetFloat("MoveY", movement.y);
		animator.SetFloat("Speed", movement.sqrMagnitude);
		animator.SetBool("LastDirectionRight", lastDirection == Direction.Right);
		animator.SetBool("LastDirectionUp", lastDirection == Direction.Up);
		animator.SetBool("LastDirectionDown", lastDirection == Direction.Down);
	}

	private void FixedUpdate()
	{
		if (!networkIdentity.isOwned || attackManager.isAttacking || attackManager.isWindingUp || attackManager.isThrustWindingUp)
			return;

		MoveCharacter();
	}

	private void MoveCharacter()
	{
		rb.MovePosition(rb.position + movement * Time.fixedDeltaTime);
	}
}
