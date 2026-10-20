// 수업 제출용 트레일러 연출. _TempBuildFlow 폴더째 지우면 같이 사라짐.
// 메뉴 Tools > Temp > Record Trailer 가 이 오브젝트를 만들고 녹화한다.
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TempTrailerDirector : MonoBehaviour
{
    public static Action OnFinished;
    public Texture2D logo;
    public Font bodyFont, titleFont;

    Camera cam;
    Image fade;
    Text caption, bigCaption, sub;
    RawImage logoImg;
    const float FadeTime = 0.6f;

    IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        Setup();
        HideOthers();
        yield return Run();
        OnFinished?.Invoke();
    }

    IEnumerator Run()
    {
        // 0. 검은 화면 첫 문장
        yield return Card("The dead awaken as skulls.", 2.6f);

        // 1. 마을 하늘 위로 로고 등장
        yield return LogoShot(new Vector3(66, 7, 16), new Vector3(67, 18, 80),
                              new Vector3(66, 14, 23), new Vector3(68, 28, 110), 5.2f);

        // 2. 주인공 주위를 천천히 돈다
        yield return Orbit(new Vector3(66, 4.6f, 29.8f), 5.6f, 0.4f, -45f, 8f, 4.2f,
                           "One skull wakes, remembering nothing.");

        // 3. 제단으로 밀고 들어감
        yield return Move(new Vector3(50, 21, 48), new Vector3(50, 10, 66),
                          new Vector3(50, 16.5f, 55), new Vector3(50, 10.5f, 66), 3.8f,
                          "Offer the Ashes of Peace. Pray.");

        // 4. 문 앞에서 뒤로 빠지며 세 문과 진정한 해골을 보여줌
        yield return Move(new Vector3(68, 20, 78), new Vector3(68, 30, 112),
                          new Vector3(68, 34, 45), new Vector3(68, 28, 112), 4.6f,
                          "Three doors. Oblivion, Longing, Grudge.");

        // 5. 보스 맵 (검은 화면 동안 로드)
        yield return Load("MapLayout_Boss");
        yield return Move(new Vector3(25, 9, -132), new Vector3(25, 5, -110),
                          new Vector3(25, 9, -96), new Vector3(24.4f, 7, -54), 4.2f,
                          "Those left without prayer become monsters...");

        // 6. 보스 로우앵글
        yield return Move(new Vector3(26.5f, 3f, -79), new Vector3(24.4f, 10.5f, -53.7f),
                          new Vector3(25.3f, 3.6f, -72), new Vector3(24.4f, 11.5f, -53.7f), 3.6f,
                          null, "THE NAMELESS ONE");

        // 7. 마지막 문장
        yield return Card("Become the Wholly Skull.", 3.2f);
    }

    // 장면 위에 로고를 띄움 (화면을 살짝 어둡게 해서 로고가 돋보이게)
    IEnumerator LogoShot(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float dur)
    {
        Set(caption, null); Set(bigCaption, null);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float k = t / dur; float e = k * k * (3f - 2f * k);
            cam.transform.position = Vector3.Lerp(p0, p1, e);
            cam.transform.LookAt(Vector3.Lerp(t0, t1, e));
            float edge = Mathf.Clamp01(Mathf.Min(t, dur - t) / FadeTime);
            float logoA = Mathf.Clamp01((t - 0.8f) / 1.2f) * Mathf.Clamp01((dur - t - 0.2f) / 0.7f);
            SetAlpha(fade, Mathf.Max(1f - edge, 0.35f * logoA));
            SetAlpha(logoImg, logoA);
            logoImg.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.9f, 1.0f, e);
            yield return null;
        }
        SetAlpha(fade, 1f); SetAlpha(logoImg, 0);
    }


    // ---------- 연출 도구 ----------

    IEnumerator Move(Vector3 p0, Vector3 t0, Vector3 p1, Vector3 t1, float dur, string text, string big = null)
    {
        yield return Shot(dur, text, big, k =>
        {
            cam.transform.position = Vector3.Lerp(p0, p1, k);
            cam.transform.LookAt(Vector3.Lerp(t0, t1, k));
        });
    }

    IEnumerator Orbit(Vector3 c, float r, float h, float a0, float a1, float dur, string text)
    {
        yield return Shot(dur, text, null, k =>
        {
            float a = Mathf.Lerp(a0, a1, k) * Mathf.Deg2Rad;
            cam.transform.position = c + new Vector3(Mathf.Sin(a) * r, h, Mathf.Cos(a) * r);
            cam.transform.LookAt(c + Vector3.up * 0.3f);
        });
    }

    IEnumerator Shot(float dur, string text, string big, Action<float> pose)
    {
        Set(caption, text); Set(bigCaption, big);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float k = t / dur;
            pose(k * k * (3f - 2f * k));                      // smoothstep 이징
            float edge = Mathf.Min(t, dur - t) / FadeTime;    // 앞뒤 페이드
            SetAlpha(fade, 1f - Mathf.Clamp01(edge));
            float txt = Mathf.Clamp01((t - 0.5f) / 0.5f) * Mathf.Clamp01((dur - t - 0.3f) / 0.4f);
            SetAlpha(caption, txt); SetAlpha(bigCaption, txt);
            yield return null;
        }
        SetAlpha(fade, 1f); SetAlpha(caption, 0); SetAlpha(bigCaption, 0);
    }

    IEnumerator Card(string text, float dur)
    {
        SetAlpha(fade, 1f); Set(bigCaption, text);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            SetAlpha(bigCaption, Mathf.Clamp01(Mathf.Min(t - 0.3f, dur - t) / 0.6f));
            yield return null;
        }
        SetAlpha(bigCaption, 0);
    }

    IEnumerator Logo(float dur)
    {
        SetAlpha(fade, 1f);
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float a = Mathf.Clamp01(Mathf.Min(t - 0.2f, dur - 0.2f - t) / 0.8f);
            SetAlpha(logoImg, a); SetAlpha(sub, Mathf.Clamp01(Mathf.Min(t - 1.0f, dur - 0.2f - t) / 0.8f));
            logoImg.rectTransform.localScale = Vector3.one * Mathf.Lerp(0.96f, 1.02f, t / dur);
            yield return null;
        }
    }

    IEnumerator Load(string scene)
    {
        SetAlpha(fade, 1f);
        var op = SceneManager.LoadSceneAsync(scene);
        while (!op.isDone) yield return null;
        yield return null;
        HideOthers();
    }

    // 게임 카메라와 HUD를 끄고 트레일러 카메라만 남김
    void HideOthers()
    {
        foreach (var c in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            if (c != cam) c.enabled = false;
        foreach (var cv in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (cv.transform.root != transform) cv.enabled = false;
    }

    // ---------- UI 만들기 ----------

    void Setup()
    {
        var camGo = new GameObject("TrailerCam");
        camGo.transform.SetParent(transform);
        cam = camGo.AddComponent<Camera>();
        cam.fieldOfView = 50f; cam.depth = 100; cam.nearClipPlane = 0.2f; cam.farClipPlane = 1000f;
        var urp = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
        urp.renderPostProcessing = true;

        var font = bodyFont ? bodyFont : Font.CreateDynamicFontFromOSFont(new[] { "Palatino Linotype", "Arial" }, 48);
        var big = titleFont ? titleFont : font;
        var cvGo = new GameObject("TrailerCanvas");
        cvGo.transform.SetParent(transform);
        var cv = cvGo.AddComponent<Canvas>(); cv.renderMode = RenderMode.ScreenSpaceCamera; cv.worldCamera = cam; cv.planeDistance = 0.5f; cv.sortingOrder = 999; // 카메라에 직접 그려서 녹화에 확실히 포함
        var scaler = cvGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080);

        fade = Make<Image>(cvGo, "Fade", Vector2.zero, Vector2.one, Vector2.zero);
        fade.color = Color.black;
        caption = MakeText(cvGo, font, "Caption", 40, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 150), 90);
        bigCaption = MakeText(cvGo, big, "BigCaption", 66, new Vector2(0, 0.5f), new Vector2(1, 0.5f), Vector2.zero, 140);
        sub = MakeText(cvGo, font, "Sub", 30, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0, -230), 60);
        sub.text = "";
        sub.color = new Color(0.78f, 0.7f, 0.95f, 0);

        logoImg = Make<RawImage>(cvGo, "Logo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -40));
        logoImg.texture = logo;
        float w = 980f, h = logo ? w * logo.height / logo.width : 300f;
        logoImg.rectTransform.sizeDelta = new Vector2(w, h);
        SetAlpha(logoImg, 0); SetAlpha(caption, 0); SetAlpha(bigCaption, 0);
    }

    T Make<T>(GameObject parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos) where T : Graphic
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var g = go.AddComponent<T>();
        var rt = g.rectTransform; rt.anchorMin = aMin; rt.anchorMax = aMax; rt.anchoredPosition = pos;
        if (aMin == Vector2.zero && aMax == Vector2.one) rt.sizeDelta = Vector2.zero;
        g.raycastTarget = false;
        return g;
    }

    Text MakeText(GameObject parent, Font font, string name, int size, Vector2 aMin, Vector2 aMax, Vector2 pos, float height)
    {
        var t = Make<Text>(parent, name, aMin, aMax, pos);
        t.rectTransform.sizeDelta = new Vector2(0, height);
        t.font = font; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter;
        t.color = new Color(0.95f, 0.92f, 1f, 0);
        var sh = t.gameObject.AddComponent<Shadow>(); sh.effectColor = new Color(0, 0, 0, 0.8f); sh.effectDistance = new Vector2(2, -2);
        return t;
    }

    static void Set(Text t, string s) { t.text = s ?? ""; }
    static void SetAlpha(Graphic g, float a) { var c = g.color; c.a = a; g.color = c; }
}
