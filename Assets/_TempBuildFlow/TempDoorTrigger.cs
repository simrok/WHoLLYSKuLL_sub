using UnityEngine;
using UnityEngine.SceneManagement;

// shortcut: 학원 제출용 임시 흐름. 정식 문 진입(F 상호작용) 만들면 _TempBuildFlow 폴더째 삭제
// 마을 문 앞 트리거에 플레이어가 닿으면 보스 씬으로 이동
[RequireComponent(typeof(BoxCollider))]
public class TempDoorTrigger : MonoBehaviour
{
    [SerializeField] private string bossScene = "MapLayout_Boss";

    private bool loading;

    private void Reset() => GetComponent<BoxCollider>().isTrigger = true;

    private void OnTriggerEnter(Collider other)
    {
        if (loading || other.GetComponentInParent<Player>() == null) return;
        loading = true;
        SceneManager.LoadScene(bossScene);
    }
}
