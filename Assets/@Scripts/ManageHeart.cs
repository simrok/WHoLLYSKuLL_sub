using UnityEngine.UI;
using UnityEngine;

public class ManageHeart : MonoBehaviour
{
    [SerializeField] private Player player;

    [SerializeField] private Image heartPrefab;     // 하트 오브젝트의 프리팹. 기본 스프라이트는 Full
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;
    private Image[] hearts;     // 캔버스에 표시될 하트 오브젝트의 배열

    private void OnEnable() => player.OnHeartsChanged += Refresh;
    private void OnDisable() => player.OnHeartsChanged -= Refresh;

    private void Start()
    {
        // 최대 체력만큼의 하트 오브젝트를 생성한다.
        hearts = new Image[player.data.maxHearts];

        for (int i = 0; i < hearts.Length; i++)
            hearts[i] = Instantiate(heartPrefab, transform);    // 위치는 prefab의 부모에 위치한 HorizontalLayoutGroup이 정렬

        Refresh(player.data.currentHearts);
    }

    private void Refresh(int current)
    {
        // hearts 배열은 왼쪽부터 0번째
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].sprite = i < current ? fullHeart : emptyHeart;
    }
}