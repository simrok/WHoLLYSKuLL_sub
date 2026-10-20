using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageOverlayManager : MonoBehaviour
{ 
    public static DamageOverlayManager Instance { get; private set; }

    void Awake() => Instance = this;

    // 피해를 받은 경우 일시적으로 붉은 화면을 표현하기 위해 사용하는 변수들
    [Header("하트가 이 개수 이하면 경고")]
    [SerializeField] private int mLowHealthHearts = 1;
    [Header("피해를 받아 오버레이가 활성화되는 지속시간:")]
    [SerializeField] private float mHitOverlayDuration = 0.25f;

    [Header("피해를 받은 경우 일시적으로 보여줄 이미지")]
    [SerializeField] private Image mHitOverlayImage;
    [Range(0f, 1f)][SerializeField] private float mHitOverlayMaxAlpha;

    // 낮은 체력 경고에 관련된 변수. 플레이어가 피해를 받거나, 체력을 회복하여 일정 체력 비율 이상이거나/이하인 경우 이미지를 토글함

    [Header("체력이 낮을 경우 활성화 할 이미지")]
    [SerializeField] private CanvasGroup mLowHealthWarningGroup;
    [Range(0f, 1f)][SerializeField] private float mLowHealthWarningRatio;
    [Range(0f, 0.9f)][SerializeField] private float mLowHealthWarningMaxAlpha;
    [SerializeField] private float mLowHealthWarningLoopDuration = 0.5f;
    private bool mIsLowHealthWarningEnabled; // 현재 Low Health Warning이 활성화 되어 있는지
    private float mCurrentHealthRatio;

    private Coroutine mCoHitOverlay, mCoLoopLowHealthWarning, mCoReleaseLowHealthWarning;

    /// <summary>
    /// 현재 HP를 검사해서 UI 업데이트(낮은 체력 경고를 활성화 / 비활성화) 시도
    /// </summary>
    public void CheckHp(float currentHp, float maxHp)
    {
        mCurrentHealthRatio = (float)currentHp / maxHp;

        // 체력 비율 검사
        if (currentHp <= mLowHealthHearts)
        {
            //Debug.Log(mCurrentHealthRatio);

            if (mIsLowHealthWarningEnabled == false)
            {
                mIsLowHealthWarningEnabled = true;
                if (mCoLoopLowHealthWarning is not null)
                    StopCoroutine(mCoLoopLowHealthWarning);
                mCoLoopLowHealthWarning = StartCoroutine(CoLoopLowHealthWarning());
            }
        }
        else
        {
            if (mIsLowHealthWarningEnabled == true)
            {
                mIsLowHealthWarningEnabled = false;

                if (mCoLoopLowHealthWarning is not null)
                    StopCoroutine(mCoLoopLowHealthWarning);

                if (mCoReleaseLowHealthWarning is not null)
                    StopCoroutine(mCoReleaseLowHealthWarning);
                mCoReleaseLowHealthWarning = StartCoroutine(CoReleaseLowHealthWarning());
            }
        }
    }

    /// <summary>
    /// 낮은 체력 경고가 활성화된 경우, 코루틴을 이용하여 Loop 함
    /// </summary>
    private IEnumerator CoLoopLowHealthWarning()
    {
        while (true)
        {
            float process;
            float currentAlpha;
            float processDiv = 1 / mLowHealthWarningLoopDuration * 2f;

            // 페이드 인
            currentAlpha = mLowHealthWarningGroup.alpha;
            process = 0f;
            while (process < 1.0f)
            {
                process += Time.deltaTime * processDiv;
                mLowHealthWarningGroup.alpha = Mathf.Lerp(currentAlpha, Mathf.Lerp(0f, mLowHealthWarningMaxAlpha, 1.0f - mCurrentHealthRatio), process);

                yield return null;
            }

            // 페이드 아웃
            currentAlpha = mLowHealthWarningGroup.alpha;
            process = 0f;
            while (process < 1.0f)
            {
                process += Time.deltaTime * processDiv;
                mLowHealthWarningGroup.alpha = Mathf.Lerp(currentAlpha, 0.2f, process);

                yield return null;
            }
        }
    }

    private IEnumerator CoReleaseLowHealthWarning()
    {
        float process;
        float currentAlpha;

        // 페이드 인
        currentAlpha = mLowHealthWarningGroup.alpha;
        process = 0f;
        while (process < 1.0f)
        {
            process += Time.deltaTime;
            mLowHealthWarningGroup.alpha = Mathf.Lerp(currentAlpha, 0f, process);

            yield return null;
        }
    }

    /// <summary>
    /// 플레이어가 피해를 받을 경우, 오버레이(붉은 화면 이미지)를 일시적으로 활성화
    /// </summary>
    public void HitOverlay()
    {
        if (mCoHitOverlay is not null)
            StopCoroutine(mCoHitOverlay);
        mCoHitOverlay = StartCoroutine(CoHitOverlay());
    }

    /// <summary>
    /// HitOverlay()가 호출되면 이어서 호출되는 코루틴으로 일정 시간동안 투명도를 조절하는 코루틴
    /// </summary>

    private IEnumerator CoHitOverlay()
    {
        float process;
        float currentAlpha;
        float processDiv = 1 / mHitOverlayDuration * 2f;
        Color currentColor = mHitOverlayImage.color;

        // 페이드 인
        currentAlpha = mHitOverlayImage.color.a;
        process = 0f;
        while (process < 1.0f)
        {
            process += Time.deltaTime * processDiv;
            currentColor.a = Mathf.Lerp(currentAlpha, mHitOverlayMaxAlpha, process);
            mHitOverlayImage.color = currentColor;

            yield return null;
        }

        // 페이드 아웃
        currentAlpha = mHitOverlayImage.color.a;
        process = 0f;
        while (process < 1.0f)
        {
            process += Time.deltaTime * processDiv;
            currentColor.a = Mathf.Lerp(currentAlpha, 0f, process);
            mHitOverlayImage.color = currentColor;

            yield return null;
        }
    }
}
