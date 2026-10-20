using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// 카메라와 플레이어 사이를 가리는 오브젝트(게이트, 남쪽 펜스 등)를 반투명하게 만든다.
public class OcclusionFader : MonoBehaviour
{
    [SerializeField] private Transform target;            // Player
    [SerializeField] private Transform[] occluderRoots;   // 게이트, 남쪽 펜스 등 (자식 렌더러 전부 대상)
    [SerializeField] private float targetHeight = 1.8f;   // 발이 아니라 몸 중앙을 기준으로
    [SerializeField] private float padding = 1.0f;        // 플레이어 몸 두께만큼 여유
    [SerializeField] private float fadedAlpha = 0.25f;
    [SerializeField] private float fadeSpeed = 6f;

    private class Entry
    {
        public Renderer renderer;
        public Material[] original;
        public Material[] faded;   // 처음 가려질 때 한 번만 만든다
        public float alpha = 1f;
    }

    // occluderRoots 하나(게이트, 가로등, 펜스 조각)를 한 묶음으로 같이 흐리게 한다
    private readonly List<List<Entry>> groups = new List<List<Entry>>();
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

    private void Start()
    {
        foreach (var root in occluderRoots)
        {
            if (!root) continue;
            var group = new List<Entry>();
            foreach (var r in root.GetComponentsInChildren<Renderer>())
                if (!(r is ParticleSystemRenderer))
                    group.Add(new Entry { renderer = r, original = r.sharedMaterials });
            if (group.Count > 0) groups.Add(group);
        }
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 from = transform.position;
        Vector3 to = target.position + Vector3.up * targetHeight;
        float length = Vector3.Distance(from, to);
        if (length < 0.01f) return;
        var ray = new Ray(from, (to - from) / length);

        foreach (var group in groups)
        {
            // 묶음 안 렌더러 하나라도 가리면 묶음 전체를 흐리게
            bool blocking = false;
            foreach (var e in group)
            {
                if (!e.renderer) continue;
                Bounds b = e.renderer.bounds;
                b.Expand(padding * 2f);
                if (b.IntersectRay(ray, out float d) && d < length) { blocking = true; break; }
            }

            float goal = blocking ? fadedAlpha : 1f;
            foreach (var e in group)
            {
                if (!e.renderer || Mathf.Approximately(e.alpha, goal)) continue;
                e.alpha = Mathf.MoveTowards(e.alpha, goal, fadeSpeed * Time.deltaTime);
                Apply(e);
            }
        }
    }

    private void Apply(Entry e)
    {
        // 완전히 돌아오면 원래(불투명) 머티리얼로 되돌린다
        if (e.alpha >= 0.999f) { e.renderer.sharedMaterials = e.original; return; }

        if (e.faded == null)
        {
            e.faded = new Material[e.original.Length];
            for (int i = 0; i < e.original.Length; i++) e.faded[i] = MakeTransparent(e.original[i]);
        }
        if (e.renderer.sharedMaterials.Length == 0 || e.renderer.sharedMaterials[0] != e.faded[0])
            e.renderer.sharedMaterials = e.faded;

        foreach (var m in e.faded)
        {
            if (m && m.HasProperty(BaseColor))
            {
                var c = m.GetColor(BaseColor); c.a = e.alpha; m.SetColor(BaseColor, c);
            }
        }

        // URP Lit이 아닌 셰이더는 반투명이 안 되니 반쯤 사라지면 그림자만 남기고 숨긴다
        bool lit = e.faded.Length > 0 && e.faded[0] && e.faded[0].HasProperty("_Surface");
        e.renderer.shadowCastingMode = (!lit && e.alpha < 0.5f) ? ShadowCastingMode.ShadowsOnly : ShadowCastingMode.On;
    }

    private static Material MakeTransparent(Material src)
    {
        if (!src) return src;
        var m = new Material(src);
        if (!m.HasProperty("_Surface")) return m;   // URP Lit이 아니면 그대로(위에서 숨김 처리)

        m.SetFloat("_Surface", 1f);                  // Transparent
        m.SetFloat("_Blend", 0f);                    // Alpha
        m.SetOverrideTag("RenderType", "Transparent");
        m.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.renderQueue = (int)RenderQueue.Transparent;
        return m;
    }
}
