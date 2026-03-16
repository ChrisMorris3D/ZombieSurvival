using UnityEngine;

[DisallowMultipleComponent]
public class FirstPersonGraphicsController : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform graphicsRoot;

    [Header("ROTATION")]
    [SerializeField] bool captureInitialOffset = true;
    [SerializeField] Vector3 rotationOffsetEuler;
    [SerializeField] float rotationSmoothRate = 12f;

    Quaternion rotationOffset = Quaternion.identity;
    Quaternion smoothedRotation = Quaternion.identity;

    void Awake()
    {
        if (graphicsRoot == null)
        {
            graphicsRoot = transform;
        }

        CacheRotationOffset();
        SnapToTargetRotation();
    }

    void LateUpdate()
    {
        if (cameraTransform == null || graphicsRoot == null)
        {
            return;
        }

        Quaternion targetRotation = GetTargetRotation();
        float smoothing = 1f - Mathf.Exp(-rotationSmoothRate * Time.deltaTime);
        smoothedRotation = Quaternion.Slerp(smoothedRotation, targetRotation, smoothing);
        graphicsRoot.rotation = smoothedRotation;
    }

    void OnValidate()
    {
        rotationSmoothRate = Mathf.Max(0f, rotationSmoothRate);

        if (graphicsRoot == null)
        {
            graphicsRoot = transform;
        }
    }

    public void SnapToTargetRotation()
    {
        if (cameraTransform == null || graphicsRoot == null)
        {
            return;
        }

        smoothedRotation = GetTargetRotation();
        graphicsRoot.rotation = smoothedRotation;
    }

    public void RecalculateOffset()
    {
        CacheRotationOffset();
    }

    void CacheRotationOffset()
    {
        if (cameraTransform == null || graphicsRoot == null)
        {
            rotationOffset = Quaternion.Euler(rotationOffsetEuler);
            return;
        }

        rotationOffset = captureInitialOffset
            ? Quaternion.Inverse(cameraTransform.rotation) * graphicsRoot.rotation
            : Quaternion.Euler(rotationOffsetEuler);
    }

    Quaternion GetTargetRotation()
    {
        return cameraTransform.rotation * rotationOffset;
    }

    void Reset()
    {
        graphicsRoot = transform;

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            cameraTransform = mainCamera.transform;
        }
    }
}
