using UnityEngine;

namespace CrispyCube
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField, Min(0f)] private float cameraSmoothing = 0.15f;

        private Vector3 playerOffset;
        private Vector3 followVelocity;

        private void Start()
        {
            if (player == null)
            {
                Debug.LogError("Camera Controller requires a player reference.", this);
                enabled = false;
                return;
            }

            playerOffset = transform.position - player.position;
        }

        private void LateUpdate()
        {
            Vector3 targetPosition = player.position + playerOffset;

            if (cameraSmoothing <= 0f)
            {
                transform.position = targetPosition;
                followVelocity = Vector3.zero;
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref followVelocity,
                cameraSmoothing);
        }
    }
}
