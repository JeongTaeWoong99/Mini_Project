// ============================================================================
// URP 기본 셰이더 - Toon Lighting 기초
// ============================================================================
//
// 📖 학습 내용 (PDF 48-51p) :
// - 삼항연산자를 이용한 Toon 라이팅
// - ceil() 함수로 라이트 계단화 (Banding)
// - Ramp Texture를 활용한 Toon Shading
//
// 💡 핵심 개념 :
// - NdotL > 0 ? Light : Ambient → 명암 경계를 딱딱하게
// - ceil(NdotL * width) / step → 계단식 명암 표현
// - halfNdotL = NdotL * 0.5 + 0.5 → -1~1을 0~1로 변환 (UV용)
//
// ============================================================================

Shader "URPTraining/URPBasic_10"
{
    Properties
    {
        [Header(Toon Mode)]
        [KeywordEnum(Basic, Banding, Ramp)] _ToonMode("Toon Mode", Float) = 0

        [Header(Basic and Banding Settings)]
        _AmbientColor("Ambient Color (그림자 색상)", Color) = (0.2, 0.2, 0.3, 1)

        [Header(Banding Settings)]
        _LightWidth("Light Width (밴딩 너비)", Range(1, 10)) = 3
        _LightStep ("Light Step (밴딩 단계)", Range(1, 10)) = 3

        [Header(Ramp Settings)]
        _MainTex ("Main Texture", 2D)  = "white" {}
        _RampTex ("Ramp Texture", 2D)  = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
        }

        Pass
        {
            Name "Universal Forward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma target   3.5
            #pragma vertex   vert
            #pragma fragment frag

            // Toon 모드 키워드
            #pragma shader_feature_local _TOONMODE_BASIC _TOONMODE_BANDING _TOONMODE_RAMP

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ──────────────────────────────────────
            // [텍스처 & 샘플러]
            // ──────────────────────────────────────
            TEXTURE2D(_MainTex);
            TEXTURE2D(_RampTex);
            SAMPLER(sampler_MainTex);

            // ──────────────────────────────────────
            // [CBUFFER] : SRP Batcher 지원
            // ──────────────────────────────────────
            CBUFFER_START(UnityPerMaterial)
                half4  _AmbientColor;
                float  _LightWidth;
                float  _LightStep;
                float4 _MainTex_ST;
                float4 _RampTex_ST;
            CBUFFER_END

            // ──────────────────────────────────────
            // [VertexInput]
            // ──────────────────────────────────────
            struct VertexInput
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            // ──────────────────────────────────────
            // [VertexOutput]
            // ──────────────────────────────────────
            struct VertexOutput
            {
                float4 vertex : SV_POSITION;
                float3 normal : NORMAL;
                float2 uv     : TEXCOORD0;
            };

            // ──────────────────────────────────────
            // [버텍스 셰이더]
            // ──────────────────────────────────────
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.normal = TransformObjectToWorldNormal(v.normal);
                o.uv     = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            // ──────────────────────────────────────
            // [픽셀 셰이더] : Toon Lighting
            // ──────────────────────────────────────
            half4 frag(VertexOutput i) : SV_Target
            {
                // 라이트 정보
                float3 lightColor = _MainLightColor.rgb;
                float3 lightDir   = _MainLightPosition.xyz;

                // NdotL 계산 (노멀과 라이트의 내적)
                float NdotL = saturate(dot(i.normal, lightDir));

                float4 color = float4(1, 1, 1, 1);

                // ────────────────────────────────
                // Mode 1 : Basic Toon (삼항연산자)
                // NdotL > 0 이면 라이트 색상, 아니면 Ambient 색상
                // ────────────────────────────────
                #if defined(_TOONMODE_BASIC)
                    float3 toonLight = NdotL > 0 ? lightColor : _AmbientColor.rgb;
                    color.rgb *= toonLight;

                // ────────────────────────────────
                // Mode 2 : Banding Toon (ceil 함수)
                // ceil() = 올림 → 계단식 명암 표현
                // ────────────────────────────────
                #elif defined(_TOONMODE_BANDING)
                    // ceil(NdotL * width) / step → 0, 0.33, 0.66, 1 같은 계단값
                    float3 toonLight = ceil(NdotL * _LightWidth) / _LightStep * lightColor;
                    float3 ambient   = NdotL > 0 ? float3(0, 0, 0) : _AmbientColor.rgb;
                    color.rgb *= toonLight + ambient;

                // ────────────────────────────────
                // Mode 3 : Ramp Texture
                // NdotL을 UV로 변환하여 Ramp 텍스처 샘플링
                // ────────────────────────────────
                #elif defined(_TOONMODE_RAMP)
                    // -1~1 → 0~1 변환 (UV 좌표용)
                    float rawNdotL   = dot(i.normal, lightDir); // saturate 없이!
                    float halfNdotL  = rawNdotL * 0.5 + 0.5;

                    // 메인 텍스처 샘플링
                    float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                    // 램프 텍스처 샘플링 (U = halfNdotL, V = 0)
                    float3 ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_MainTex, float2(halfNdotL, 0)).rgb;

                    // Ambient (SH 라이트 프로브)
                    float3 ambient = SampleSH(i.normal);

                    color.rgb = mainTex.rgb * ramp + ambient;

                // ────────────────────────────────
                // Fallback : Basic
                // ────────────────────────────────
                #else
                    float3 toonLight = NdotL > 0 ? lightColor : _AmbientColor.rgb;
                    color.rgb *= toonLight;
                #endif

                return color;
            }

            ENDHLSL
        }
    }
}

// ============================================================================
// 📝 함수 설명
// ============================================================================
//
// ceil(x)  = 올림 (ceil(0.1) = 1, ceil(1.5) = 2)
// floor(x) = 내림 (floor(1.9) = 1)
// round(x) = 반올림
//
// SampleSH(normal) = Spherical Harmonics 라이트 프로브에서 Ambient 색상 가져오기
//
// ============================================================================
// 📝 Ramp Texture 만드는 법
// ============================================================================
//
// 1. Photoshop/GIMP에서 512x32 크기의 가로 그라데이션 이미지 생성
// 2. 왼쪽(어두운 영역) → 오른쪽(밝은 영역) 색상 배치
// 3. Unity에서 Wrap Mode = Clamp로 설정
//
// 예시 :
// [어두운 파랑] ━━━━━━━━━━━━━━━━ [밝은 노랑]
//      U=0                           U=1
//
// ============================================================================
