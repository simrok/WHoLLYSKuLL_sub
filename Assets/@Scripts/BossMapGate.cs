using System;
using System.Collections;
using UnityEngine;

public class BossMapGate : MonoBehaviour
{
    [SerializeField] private BossMapGateTrigger bossMapGateTrigger;
    [SerializeField] private GameObject rightDoor;
    [SerializeField] private GameObject leftDoor;
    [SerializeField] private float leftDoorCloseDuration = 0.4f;
    [SerializeField] private float rightDoorCloseDuration = 1.0f;
    private Quaternion rightDoorEnd = Quaternion.identity;
    private Quaternion leftDoorEnd = Quaternion.identity;

    private Coroutine gateRoutine;
    public event Action OnGateClosed;

    private void OnEnable() { bossMapGateTrigger.OnPlayerEntered += HandleGate; }
    private void OnDisable() { bossMapGateTrigger.OnPlayerEntered -= HandleGate; }

    /// <summary>
    /// 알림을 받으면 보스 게이트 문을 닫음
    /// </summary>
    private void HandleGate()
    {
        gateRoutine = StartCoroutine(CloseGate());
        //Debug.Log("문 회전 코루틴 시작");
    }

    public IEnumerator CloseGate()
    {
        Quaternion rightDoorOriginRot = rightDoor.transform.localRotation;  // 오른쪽 문의 로컬각도
        Quaternion leftDoorOriginRot = leftDoor.transform.localRotation;  // 오른쪽 문의 로컬각도

        float elapsed = 0f;
        float longest = Mathf.Max(rightDoorCloseDuration, leftDoorCloseDuration);
        while (elapsed < longest)
        {
            elapsed += Time.deltaTime;
            float leftT = Mathf.Clamp01(elapsed / leftDoorCloseDuration);
            float rightT = Mathf.Clamp01(elapsed / rightDoorCloseDuration);
            leftDoor.transform.localRotation = Quaternion.Slerp(leftDoorOriginRot, leftDoorEnd, leftT);
            rightDoor.transform.localRotation = Quaternion.Slerp(rightDoorOriginRot, rightDoorEnd, rightT);

            yield return null;
        }
        leftDoor.transform.localRotation = leftDoorEnd;
        rightDoor.transform.localRotation = rightDoorEnd;

        OnGateClosed?. Invoke();
        yield return null;
    }
}
