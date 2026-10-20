using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// shortcut: 학원 제출용 임시 흐름. 정식 사망/보상 처리 만들면 _TempBuildFlow 폴더째 삭제
// 보스 씬: 플레이어 하트 0 → 마을로, 보스 HP 0 → CLEAR 표시 후 게임 종료
// 기존 코드는 건드리지 않고 public 이벤트(OnHeartsChanged, onHealthChanged)만 구독
public class TempBossFlow : MonoBehaviour
{
    [SerializeField] private string villageScene = "Village";
    [SerializeField] private float deathDelay = 2f;
    [SerializeField] private float clearDelay = 4f;

    private Player player;
    private BossMonsterController boss;
    private string message;
    private GUIStyle style;
    private Color color;

    // 마을 보라 불꽃(0.325, 0, 0.831)을 밝게 한 연보라 / 원한의 문 계열 핏빛 빨강
    private static readonly Color ClearColor = new Color(0.80f, 0.62f, 1f);
    private static readonly Color DeathColor = new Color(0.72f, 0.08f, 0.16f);

    private void Start()
    {
        player = FindFirstObjectByType<Player>();
        boss = FindFirstObjectByType<BossMonsterController>();
        if (player == null || boss == null)
        {
            Debug.LogError("TempBossFlow: 씬에서 Player 또는 BossMonsterController를 못 찾음", this);
            return;
        }
        player.OnHeartsChanged += OnHeartsChanged;
        boss.onHealthChanged += OnBossHealthChanged;
    }

    private void OnDestroy()
    {
        if (player != null) player.OnHeartsChanged -= OnHeartsChanged;
        if (boss != null) boss.onHealthChanged -= OnBossHealthChanged;
    }

    private void OnHeartsChanged(int hearts)
    {
        if (hearts <= 0) End("YOU DIED", false);
    }

    private void OnBossHealthChanged(int current, int max)
    {
        if (current <= 0) End("CLEAR", true);
    }

    private void End(string text, bool clear)
    {
        if (message != null) return;   // 한 번만 (동시에 죽는 경우 먼저 온 쪽)
        message = text;
        color = clear ? ClearColor : DeathColor;
        StartCoroutine(CoEnd(clear));
    }

    private IEnumerator CoEnd(bool clear)
    {
        yield return new WaitForSeconds(clear ? clearDelay : deathDelay);
        if (!clear)
        {
            SceneManager.LoadScene(villageScene);
            yield break;
        }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGUI()
    {
        if (message == null) return;
        style ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = Screen.height / 8,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        var rect = new Rect(0, 0, Screen.width, Screen.height);
        // 어두운 그림자를 먼저 깔아 보라 안개 배경에서도 읽히게
        style.normal.textColor = new Color(0.05f, 0.02f, 0.08f, 0.85f);
        GUI.Label(new Rect(4, 4, Screen.width, Screen.height), message, style);
        style.normal.textColor = color;
        GUI.Label(rect, message, style);
    }
}
