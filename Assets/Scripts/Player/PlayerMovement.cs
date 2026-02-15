using System;
using System.Collections;
using System.Collections.Generic;
using CrispyCube;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("PLAYER OPTIONS")]
    public float baseMovementSpeed = 5;
    public float runSpeedMultiplier = 2;

    [Header("RAYCAST")]
    public float groundCheckDistance;
    public LayerMask groundLayer;

    [Header("VARIABLES")]
    public PlayerStaminaController playerStaminaController;
    public PlayerHealthController playerHealthController;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private Rigidbody rb;

    private bool isGrounded = true;
    private float movementSpeed;
    private bool running;

    private float horizontalInput;
    private float verticalInput;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    void Start()
    {
        movementSpeed = baseMovementSpeed;
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        GetInput();

        MovePlayer();
        CheckForGround();
    }

    void FixedUpdate()
    {

    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    private void GetInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            running = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            running = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("TryJump");
            Jump();
        }
    }

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

    void MovePlayer()
    {
        movementSpeed = baseMovementSpeed;
        if (running && playerStaminaController.PlayerStamina.Value > 0)
        {
            movementSpeed *= runSpeedMultiplier;
        }

        playerStaminaController.UpdateStamina(running);

        Vector3 moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * movementSpeed * 100, ForceMode.Force);
    }

    private void Jump()
    {
        if (!isGrounded)
        {
            Debug.Log("Player In Air.  Jump Failed");
            return;
        }

        isGrounded = false;
        rb.AddForce(Vector3.up * 1000, ForceMode.Impulse);
    }

    private void CheckForGround()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * groundCheckDistance, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, groundLayer))
        {
            // Debug.Log("Hit " + hit.distance);
            if (hit.distance <= groundCheckDistance + .05f)
            {
                isGrounded = true;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.green);
            }
            else
            {
                isGrounded = false;
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.white);
            Debug.Log("Ground Missing!!!");
        }
    }
}
