using CrispyCube;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
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

    InputAction moveAction;
    InputAction sprintAction;


    void Awake()
    {
        if (!playerStaminaController)
        {
            playerStaminaController = GetComponent<PlayerStaminaController>();
        }

        CreateInputActions();
    }

    void OnEnable()
    {
        moveAction.Enable();
        sprintAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        sprintAction.Disable();

        isSprinting = false;

        if (playerStaminaController != null)
        {
            playerStaminaController.UpdateStamina(false);
        }
    }

    void OnDestroy()
    {
        moveAction.Dispose();
        sprintAction.Dispose();
    }

    void Update()
    {
        if (roundTimer == null || roundTimer.Value <= 0)
        {
            isSprinting = false;
            playerStaminaController.UpdateStamina(false);
            return;
        }

        Vector2 movementInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);
        Vector3 inputDirection = new Vector3(movementInput.x, 0f, movementInput.y);
        Vector3 moveDirection = Quaternion.Euler(0f, movementRotationOffset, 0f) * inputDirection;
        bool sprintInput = sprintAction.IsPressed();
        isSprinting = sprintInput && inputDirection.sqrMagnitude > 0f && playerStaminaController.PlayerStamina.Value > 0f;

        playerStaminaController.UpdateStamina(isSprinting);

        float currentMovementSpeed = isSprinting ? movementSpeed * sprintSpeedMultiplier : movementSpeed;

        transform.position += moveDirection * currentMovementSpeed * Time.deltaTime;
    }

    void CreateInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value, expectedControlType: "Vector2");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow")
            .With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow")
            .With("Right", "<Keyboard>/rightArrow");

        moveAction.AddBinding("<Gamepad>/leftStick");

        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Keyboard>/rightShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");
    }
}
