// ============================================================================
// URP 기본 셰이더 - Light Vector를 활용한 Lambert 라이팅
// ============================================================================
//
// 📖 학습 내용 (PDF 43-47p) :
// - Normal 벡터를 활용한 라이팅 구현
// - TransformObjectToWorldNormal()로 노멀 월드 변환
// - Lambert 라이팅 : saturate(dot(normal, light)) => 균일한 난반사
// - Per-Vertex vs Per-Pixel 라이팅 비교
//
// 💡 Lambert 공식 :
// - dot(N, L) = cos(θ) → 두 벡터가 같은 방향이면 1, 수직이면 0
// - saturate() → 0~1 사이로 클램프 (음수 방지)
//
// 🔄 Per-Vertex vs Per-Pixel :
// - Per-Vertex : 버텍스 단위로 계산 → 빠르지만 품질 낮음 (로우폴리 티남)
// - Per-Pixel  : 픽셀 단위로 계산 → 느리지만 품질 높음 (부드러운 그라데이션)
//
// ============================================================================

Shader "URPTraining/URPBasic_9"
{
    Properties
    {
        [Toggle(_PER_PIXEL_LIGHTING)] _PerPixelLighting("Per-Pixel Lighting (고품질)", Float) = 1
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

            // 토글 키워드 정의
            #pragma shader_feature_local _PER_PIXEL_LIGHTING

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ──────────────────────────────────────
            // [VertexInput] : 버텍스 버퍼에서 읽어올 데이터
            // ──────────────────────────────────────
            struct VertexInput
            {
                float4 vertex : POSITION;  // 버텍스 위치 (로컬 좌표)
                float3 normal : NORMAL;    // 노멀 벡터 (면의 방향)
            };

            // ──────────────────────────────────────
            // [VertexOutput] : 버텍스 → 픽셀 셰이더로 전달할 데이터
            // ──────────────────────────────────────
            struct VertexOutput
            {
                float4 vertex : SV_POSITION; // 화면상 위치 (클립 좌표)
                float3 normal : NORMAL;      // 월드 공간 노멀 (Per-Pixel용)
                float3 light  : COLOR;       // 라이팅 결과 (Per-Vertex용)
            };

            // ──────────────────────────────────────
            // [버텍스 셰이더]
            // ──────────────────────────────────────
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                // 로컬 좌표 → 클립 좌표 변환
                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                // 로컬 노멀 → 월드 노멀 변환
                o.normal = TransformObjectToWorldNormal(v.normal);
                
                // Per-Pixel 모드 : 픽셀 셰이더에서 계산하므로 버텍스 단계는 패스
                #if defined(_PER_PIXEL_LIGHTING)
                    o.light = float3(0, 0, 0);
                // Per-Vertex 모드 : 버텍스에서 라이팅 계산 (저품질)
                // ★ o.normal(버텍스 노멀) 사용 → 결과 "색상"이 보간되어 각짐
                #else
                    float3 lightDir = _MainLightPosition.xyz;
                    o.light = saturate(dot(o.normal, lightDir)) * _MainLightColor.rgb;
                #endif

                return o;
            }

            // ──────────────────────────────────────
            // [픽셀 셰이더]
            // ──────────────────────────────────────
            half4 frag(VertexOutput i) : SV_Target
            {
                float4 color = float4(1, 1, 1, 1);
                
                // Per-Pixel 모드 : 픽셀에서 라이팅 계산 (고품질)
                // ★ i.normal(보간된 노멀) 사용 → "노멀"이 부드럽게 보간되어 정밀 계산
                #if defined(_PER_PIXEL_LIGHTING)
                    float3 lightDir = _MainLightPosition.xyz;
                    color.rgb *= saturate(dot(i.normal, lightDir)) * _MainLightColor.rgb;
                // Per-Vertex 모드 : 보간된 라이팅 적용 (저품질)
                #else
                    color.rgb *= i.light;
                #endif

                return color;
            }

            ENDHLSL
        }
    }
}
