using UnityEngine;
[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Follow Target")]
    [SerializeField] private Transform target;

    [Header("Framing")]
    [SerializeField] Vector3 offset = new Vector3(0f, 14f, -10f);

    [SerializeField] private float smoothTime = 0.08f;

    private Vector3 followVelocity;

    private void Awake()
    {
        if (target == null)
        {
            Debug.LogError("CameraFollow needs a Player Transform assigned", this);
            this.enabled = false;   // 컴포넌트 끄기
        }
    }

    private void Start()
    {
        transform.position = target.position + offset;
    }
    private void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref followVelocity,
            smoothTime
            );
    }
}
