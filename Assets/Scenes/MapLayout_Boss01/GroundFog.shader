// Ground fog: one transparent plane, two scrolling noise layers, soft edge where it meets geometry.
Shader "WhollySkull/GroundFog"
{
    Properties
    {
        _NoiseTex ("Noise", 2D) = "white" {}
        _Color ("Color", Color) = (0.40, 0.45, 0.62, 1)
        _Alpha ("Max Alpha", Range(0,1)) = 0.35
        _Scale1 ("Layer1 Scale (world m)", Float) = 28
        _Scale2 ("Layer2 Scale (world m)", Float) = 11
        _Speed1 ("Layer1 Speed xy", Vector) = (0.012, 0.006, 0, 0)
        _Speed2 ("Layer2 Speed xy", Vector) = (-0.02, 0.011, 0, 0)
        _Contrast ("Contrast", Range(0.5,4)) = 1.6
        _SoftDepth ("Soft Edge Depth", Float) = 1.2
        _EdgeFade ("Plane Edge Fade (0-0.5)", Range(0,0.5)) = 0.12
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent-1" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST; half4 _Color; half _Alpha; float _Scale1; float _Scale2;
                float4 _Speed1; float4 _Speed2; half _Contrast; float _SoftDepth; half _EdgeFade;
            CBUFFER_END

            struct A { float4 pos : POSITION; float2 uv : TEXCOORD0; };
            struct V { float4 pos : SV_POSITION; float3 wpos : TEXCOORD0; float2 uv : TEXCOORD1; float4 spos : TEXCOORD2; float fog : TEXCOORD3; };

            V vert (A i)
            {
                V o;
                VertexPositionInputs p = GetVertexPositionInputs(i.pos.xyz);
                o.pos = p.positionCS; o.wpos = p.positionWS; o.uv = i.uv;
                o.spos = ComputeScreenPos(p.positionCS);
                o.fog = ComputeFogFactor(p.positionCS.z);
                return o;
            }

            half4 frag (V i) : SV_Target
            {
                float t = _Time.y;
                half n1 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.wpos.xz / _Scale1 + _Speed1.xy * t).r;
                half n2 = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, i.wpos.xz / _Scale2 + _Speed2.xy * t).r;
                half n = saturate(pow(saturate(n1 * 0.65 + n2 * 0.55), _Contrast));

                float2 suv = i.spos.xy / i.spos.w;
                float sceneZ = LinearEyeDepth(SampleSceneDepth(suv), _ZBufferParams);
                float myZ = LinearEyeDepth(i.pos.z, _ZBufferParams);
                half soft = saturate((sceneZ - myZ) / _SoftDepth);

                float2 e = min(i.uv, 1 - i.uv);
                half edge = saturate(min(e.x, e.y) / max(_EdgeFade, 1e-4));

                half a = n * _Alpha * soft * edge;
                half3 c = MixFog(_Color.rgb, i.fog);
                return half4(c, a);
            }
            ENDHLSL
        }
    }
}