using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class ThirdPersonMouseFacing : MonoBehaviour
{
    const float MinimumAimDirectionSqrMagnitude = 0.0001f;

    [Header("AIMING")]
    [SerializeField] Camera aimCamera;
    [SerializeField, Min(0f)] float rotationSpeed = 720f;

    Rigidbody playerRigidbody;
    Quaternion targetRotation;
    bool hasTargetRotation;

    void Awake()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        ResolveAimCamera();
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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

        Ray cursorRay = aimCamera.ScreenPointToRay(Input.mousePosition);
        Plane aimPlane = new Plane(Vector3.up, transform.position);

        if (!aimPlane.Raycast(cursorRay, out float distanceToPlane))
        {
            return;
        }

        Vector3 aimPoint = cursorRay.GetPoint(distanceToPlane);
        Vector3 aimDirection = aimPoint - transform.position;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude <= MinimumAimDirectionSqrMagnitude)
        {
            return;
        }

        targetRotation = Quaternion.LookRotation(aimDirection, Vector3.up);
        hasTargetRotation = true;
    }

    void FixedUpdate()
    {
        if (!hasTargetRotation)
        {
            return;
        }

        Quaternion nextRotation = Quaternion.RotateTowards(
            playerRigidbody.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime);

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
