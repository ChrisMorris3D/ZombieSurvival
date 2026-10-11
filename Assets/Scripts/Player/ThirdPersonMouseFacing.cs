using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonMouseFacing : MonoBehaviour
{
    [Header("AIMING")]
    [SerializeField] Camera aimCamera;
    [SerializeField, Min(0f)] float rotationSpeed = 720f;

    Rigidbody playerRigidbody;
    InputAction pointerPositionAction;
    InputAction gamepadLookAction;
    Quaternion targetRotation;
    bool hasTargetRotation;
    bool isUsingGamepadAim;
    

    const float MinimumAimDirectionSqrMagnitude = 0.0001f;
    const float MinimumGamepadInputSqrMagnitude = 0.01f;
    

    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        ResolveAimCamera();

        pointerPositionAction = new InputAction("Pointer Position", InputActionType.PassThrough, "<Pointer>/position", expectedControlType: "Vector2");
        pointerPositionAction.performed += OnPointerPositionChanged;

        gamepadLookAction = new InputAction("Gamepad Look", InputActionType.Value, "<Gamepad>/rightStick", expectedControlType: "Vector2");
    }

    void OnEnable()
    {
        pointerPositionAction.Enable();
        gamepadLookAction.Enable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        pointerPositionAction.Disable();
        gamepadLookAction.Disable();
        hasTargetRotation = false;
        isUsingGamepadAim = false;
    }

    void OnDestroy()
    {
        pointerPositionAction.performed -= OnPointerPositionChanged;
        pointerPositionAction.Dispose();
        gamepadLookAction.Dispose();
    }

    void Update()
    {
        if (aimCamera == null)
        {
            ResolveAimCamera();
        }

        if (aimCamera == null)
        {
            return;
        }

        Vector2 gamepadLookInput = gamepadLookAction.ReadValue<Vector2>();

        if (gamepadLookInput.sqrMagnitude > MinimumGamepadInputSqrMagnitude)
        {
            isUsingGamepadAim = true;
            UpdateGamepadTargetRotation(gamepadLookInput);
        }
        else if (!isUsingGamepadAim)
        {
            UpdatePointerTargetRotation();
        }
    }

    void UpdatePointerTargetRotation()
    {
        Vector2 pointerPosition = pointerPositionAction.ReadValue<Vector2>();
        Ray cursorRay = aimCamera.ScreenPointToRay(new Vector3(pointerPosition.x, pointerPosition.y, 0f));
        Plane aimPlane = new Plane(Vector3.up, transform.position);

        if (!aimPlane.Raycast(cursorRay, out float distanceToPlane))
        {
            return;
        }

        Vector3 aimPoint = cursorRay.GetPoint(distanceToPlane);
        Vector3 aimDirection = aimPoint - transform.position;
        aimDirection.y = 0f;

        SetTargetRotation(aimDirection);
    }

    void UpdateGamepadTargetRotation(Vector2 lookInput)
    {
        Vector3 cameraRight = Vector3.ProjectOnPlane(aimCamera.transform.right, Vector3.up);
        Vector3 cameraForward = Vector3.ProjectOnPlane(aimCamera.transform.forward, Vector3.up);

        if (cameraRight.sqrMagnitude <= MinimumAimDirectionSqrMagnitude)
        {
            return;
        }

        cameraRight.Normalize();

        if (cameraForward.sqrMagnitude <= MinimumAimDirectionSqrMagnitude)
        {
            cameraForward = Vector3.Cross(cameraRight, Vector3.up);
        }
        else
        {
            cameraForward.Normalize();
        }

        Vector3 aimDirection = cameraRight * lookInput.x + cameraForward * lookInput.y;
        SetTargetRotation(aimDirection);
    }

    void SetTargetRotation(Vector3 aimDirection)
    {
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude <= MinimumAimDirectionSqrMagnitude)
        {
            return;
        }

        targetRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
        hasTargetRotation = true;
    }

    void OnPointerPositionChanged(InputAction.CallbackContext context)
    {
        isUsingGamepadAim = false;
    }

    void FixedUpdate()
    {
        if (!hasTargetRotation)
        {
            return;
        }

        Quaternion nextRotation = Quaternion.RotateTowards(playerRigidbody.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        playerRigidbody.MoveRotation(nextRotation);
    }

    void ResolveAimCamera()
    {
        if (aimCamera == null)
        {
            aimCamera = Camera.main;
        }
    }

    void OnValidate()
    {
        rotationSpeed = Mathf.Max(0f, rotationSpeed);
    }

    void Reset()
    {
        aimCamera = Camera.main;
    }
}
