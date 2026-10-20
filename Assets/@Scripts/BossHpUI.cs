using UnityEngine;
using UnityEngine.UI;

public class BossHpUI : MonoBehaviour
{
    [SerializeField] Monster monster;
    [SerializeField] Image fill;

    private void OnEnable() => monster.onHealthChanged += Refresh;
    private void OnDisable() => monster.onHealthChanged -= Refresh;

    private void Refresh(int current, int max) => fill.fillAmount = (float)current / max;
}
