using UnityEngine;
using UnityEngine.SceneManagement;

// shortcut: 학원 제출용 임시 타이틀 화면(OnGUI). 정식 UI(Canvas) 만들면 _TempBuildFlow 폴더째 삭제
// 방향키(←→↑↓)로 선택, Enter/Space/D로 결정, 마우스 클릭도 됨
public class TempTitleMenu : MonoBehaviour
{
    [SerializeField] private string firstScene = "Village";
    [SerializeField] private Texture2D background, logo;
    [SerializeField] private Texture2D startOn, startOff, quitOn, quitOff;

    private int selected;   // 0 = Start Game, 1 = Quit

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow)) selected = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow)) selected = 1;
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.D)) Confirm();
    }

    private void Confirm()
    {
        if (selected == 0) { SceneManager.LoadScene(firstScene); return; }
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGUI()
    {
        float w = Screen.width, h = Screen.height;
        GUI.DrawTexture(new Rect(0, 0, w, h), background, ScaleMode.ScaleAndCrop);

        float lw = w * 0.56f, lh = lw * logo.height / logo.width;
        GUI.DrawTexture(new Rect((w - lw) / 2f, h * 0.01f, lw, lh), logo);

        float bh = h * 0.083f, bw = bh * startOn.width / startOn.height, by = h * 0.86f;
        Rect startRect = new Rect(w / 2f - bw * 0.95f, by, bw, bh);
        Rect quitRect = new Rect(w / 2f, by, bw, bh);

        Vector2 mouse = Event.current.mousePosition;
        if (startRect.Contains(mouse)) selected = 0;
        else if (quitRect.Contains(mouse)) selected = 1;
        if (Event.current.type == EventType.MouseDown && (startRect.Contains(mouse) || quitRect.Contains(mouse))) Confirm();

        GUI.DrawTexture(startRect, selected == 0 ? startOn : startOff);
        GUI.DrawTexture(quitRect, selected == 1 ? quitOn : quitOff);
    }
}
