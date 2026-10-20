using UnityEngine;
using UnityEngine.UI;

public class ImageLerper : MonoBehaviour
{
    [SerializeField] private Sprite[] mSprites; // Lerp에 사용할 스프라이트 배열
    [SerializeField] private float mLerpDuration = 1.0f;    // Lerp에 걸리는 시간

    private Image[] mImageComponents;   // Image 컴포넌트 배열
    private int mCurrentIndex = 0;
    private float mLerpStartTime;
    private bool mIsLerping = false;

    private void Start()
    {
        mImageComponents = GetComponentsInChildren<Image>(true);

        if (mImageComponents.Length < 2 || mSprites.Length < 2)
        {
            Debug.LogError("최소 2개의 이미지 컴포넌트와 Sprite가 필요합니다.");
            return;
        }

        // 초기 상태 설정
        mImageComponents[0].sprite = mSprites[0];
        mImageComponents[1].sprite = mSprites[1];
    }

    private void Update()
    {
        if (!mIsLerping)
        {
            mIsLerping = true;
            mLerpStartTime = Time.time;
        }

        // Lerp 진행
        float elapsedTime = Time.time - mLerpStartTime;
        float t = Mathf.Clamp01(elapsedTime / mLerpDuration);

        // Lerp 진행 상황에 따라 투명도 조절
        Color lerpedColor = mImageComponents[0].color;
        lerpedColor.a = Mathf.Lerp(1.0f, 0.0f, t);
        mImageComponents[0].color = lerpedColor;

        lerpedColor = mImageComponents[1].color;
        lerpedColor.a = Mathf.Lerp(0.0f, 1.0f, t);
        mImageComponents[1].color = lerpedColor;

        // Lerp가 완료되면 다음 스프라이트 쌍으로 이동
        if (t >= 1.0f)
        {
            mCurrentIndex = (mCurrentIndex + 1) % mSprites.Length;
            mImageComponents[0].sprite = mSprites[mCurrentIndex];
            mImageComponents[1].sprite = mSprites[(mCurrentIndex + 1) % mSprites.Length];

            lerpedColor.a = 1.0f;
            mImageComponents[0].color = lerpedColor;
            lerpedColor.a = 0.0f;
            mImageComponents[1].color = lerpedColor;

            mIsLerping = false;
        }
    }
}
