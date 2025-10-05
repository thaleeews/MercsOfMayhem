using UnityEngine;

namespace MercsOfMayhem.Utilities
{
    /// <summary>
    /// Camera controller for following the player.
    /// Supports smooth following with offset and boundaries.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("Target Settings")]
        [SerializeField] private Transform target;

        [Header("Follow Settings")]
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -10);
        [SerializeField] private float smoothSpeed = 0.125f;

        [Header("Boundaries (Optional)")]
        [SerializeField] private bool useBoundaries = false;
        [SerializeField] private float minX = -10f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = 0f;
        [SerializeField] private float maxY = 10f;

        private void LateUpdate()
        {
            if (target == null)
            {
                Debug.LogWarning("CameraController: No target assigned!");
                return;
            }

            FollowTarget();
        }

        private void FollowTarget()
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            if (useBoundaries)
            {
                smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);
                smoothedPosition.y = Mathf.Clamp(smoothedPosition.y, minY, maxY);
            }

            transform.position = smoothedPosition;
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmosSelected()
        {
            if (useBoundaries)
            {
                Gizmos.color = Color.yellow;
                Vector3 topLeft = new Vector3(minX, maxY, 0);
                Vector3 topRight = new Vector3(maxX, maxY, 0);
                Vector3 bottomLeft = new Vector3(minX, minY, 0);
                Vector3 bottomRight = new Vector3(maxX, minY, 0);

                Gizmos.DrawLine(topLeft, topRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(bottomRight, bottomLeft);
                Gizmos.DrawLine(bottomLeft, topLeft);
            }
        }
    }
}
