using System;
using UnityEngine;

public class BossMapGateTrigger : MonoBehaviour
{
    private bool isTriggered = false;
    public event Action OnPlayerEntered;

    private void OnTriggerEnter(Collider _other)
    {
        // 문은 한 번만 닫혀야 하므로 트리거 된 적이 있는지 체크
        // 디버깅
        //Debug.Log($"트리거 감지: {_other.name}, 태그: {_other.tag}");
        if (!isTriggered && _other.GetComponentInParent<Player>())
        {
            //Debug.Log("플레이어 들어옴");
            isTriggered = true;
            OnPlayerEntered?.Invoke();   // 보스몬스터 컨트롤러에 플레이어가 들어온 것을 알림
        }
    }
}
