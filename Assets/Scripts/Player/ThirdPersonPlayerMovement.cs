using CrispyCube;
using UnityEngine;

[RequireComponent(typeof(PlayerStaminaController))]
public class ThirdPersonPlayerMovement : MonoBehaviour
{
    [Header("ROUND")]
    [SerializeField] IntegerVariable roundTimer;

    [Header("PLAYER OPTIONS")]
    [SerializeField] float movementSpeed = 5f;
    [SerializeField] float sprintSpeedMultiplier = 2f;
    [SerializeField] float movementRotationOffset = 0f;

    [Header("SPRINTING")]
    [SerializeField] bool isSprinting;
    [SerializeField] PlayerStaminaController playerStaminaController;


    void Update()
    {
        if (roundTimer == null || roundTimer.Value <= 0)
        {
            isSprinting = false;
            playerStaminaController.UpdateStamina(false);
            return;
        }

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 moveDirection = Quaternion.Euler(0f, movementRotationOffset, 0f) * inputDirection;
        bool sprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        isSprinting = sprintInput && inputDirection.sqrMagnitude > 0f && playerStaminaController.PlayerStamina.Value > 0f;

        playerStaminaController.UpdateStamina(isSprinting);

        float currentMovementSpeed = isSprinting ? movementSpeed * sprintSpeedMultiplier : movementSpeed;

        transform.position += moveDirection * currentMovementSpeed * Time.deltaTime;
    }
}
