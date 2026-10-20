using UnityEngine;

// shortcut: 학원 제출용 임시 프레임 제한. 정식 그래픽 옵션(설정 메뉴) 만들면 삭제
// 그래픽 드라이버가 VSync를 무시해도 60fps로 고정 → GPU 과부하/코일 소음 방지
public static class TempFrameLimit
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Apply()
    {
        QualitySettings.vSyncCount = 0;      // vSync가 켜져 있으면 targetFrameRate가 무시됨
        Application.targetFrameRate = 60;
    }
}
