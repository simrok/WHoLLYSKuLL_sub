using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    [SerializeField] Material flashMaterial;
    [SerializeField] float flashTime = 0.2f;

    Renderer[] renderers;
    Material[][] originalMaterials; // 렌더러마다 원래 Material 배열을 저장. 렌더러 하나가 머티리얼 여러 개를 가지고 있을 수 있음

    private Coroutine hitFlashRoutine;

    private void OnDisable()
    {
        if (hitFlashRoutine != null)
        {
            StopCoroutine(hitFlashRoutine);
            hitFlashRoutine = null;
        }
        RestoreMaterials(); // 끊겼을 경우 원래 머티리얼로 복구
    }
    private void Awake()
    {
        // 렌더러마다 '원래 머티리얼 배열'을 저장
        // 파티클(VFX)은 흰색으로 바꾸지 않음
        renderers = System.Array.FindAll(GetComponentsInChildren<Renderer>(), r => !(r is ParticleSystemRenderer));
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].sharedMaterials;    // 인스펙터에서 보이는 머티리얼 목록
        }
    }

    private void SetFlash()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            int count = originalMaterials[i].Length;    // 이 렌더러의 칸 수 (Body1이면 2)
            Material[] whites = new Material[count];    // 같은 칸 수의 빈 배열
            for (int j = 0; j < count; j++)
                whites[j] = flashMaterial;              // 모든 칸을 흰색으로
            renderers[i].sharedMaterials = whites;      // 통째로 넣기
        }
    }
    private void RestoreMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sharedMaterials = originalMaterials[i];    // 저장해둔 원래 목록을 통째로 넣기
    }

    private IEnumerator FlashRoutine()
    {
        SetFlash();
        yield return new WaitForSeconds(flashTime);
        RestoreMaterials();
    }

    public void Flash()
    {
        hitFlashRoutine = StartCoroutine(FlashRoutine());
    }
}