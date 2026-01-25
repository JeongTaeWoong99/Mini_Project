// ============================================================================
// URP 기본 셰이더 - 가장 단순한 형태의 Unlit 셰이더
// ============================================================================
//
// ⚠️ 수정 안내 :
// PDF 1번 셰이더가 구 버전(약 4년 전) 기준으로 작성되어 있어서,
// 최신 URP 스타일로 수정한 버전입니다.
//
// 주요 변경 사항 :
// 1. pragma 지시문 현대화 (#pragma prefer_hlslcc gles 등 제거)
// 2. TEXTURE2D / SAMPLER 매크로 사용
// 3. SAMPLE_TEXTURE2D 매크로 사용
// 4. CBUFFER 추가 (SRP Batcher 지원)
//
// ============================================================================

// [셰이더 경로] : Material에서 셰이더 선택 시 "URPTraining" 폴더 안에 "URPBasic_1"으로 표시됨
Shader "URPTraining/URPBasic_1"
{
    // [Properties] : Material Inspector에 노출할 변수들
    Properties
    {
        _TintColor("Test Color"  , Color)       = (1, 1, 1, 1)  // 틴트 색상 (RGBA)
        _Intensity("Range Sample", Range(0, 1)) = 0.5              // 강도 슬라이더 (0~1)
        _MainTex("Main Texture"  , 2D)          = "white" {}       // 메인 텍스처 (기본값 : 흰색)
    }

    // [SubShader] : 실제 렌더링 방법을 정의하는 블록
    SubShader
    {
        // [Tags] : 렌더링 설정
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"  // URP 사용 선언
            "RenderType"     = "Opaque"             // 불투명 오브젝트
            "Queue"          = "Geometry"           // 렌더링 순서 (기본값 2000)
        }
    
        // [Pass] : 한 번의 드로우콜로 실행되는 렌더링 단위
        Pass
        {
            Name "Universal Forward"
            Tags { "LightMode" = "UniversalForward" }

            // ====== HLSL 코드 시작 ======
            HLSLPROGRAM

            // [pragma] : 컴파일러 지시문
            #pragma target   3.5                    // Shader Model 3.5 이상
            #pragma vertex   vert                   // 버텍스 셰이더 함수명 지정
            #pragma fragment frag                   // 픽셀 셰이더 함수명 지정

            // [include] : URP 라이팅 함수들 가져오기
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ──────────────────────────────────────
            // [변수 선언] : Properties에서 선언한 변수를 HLSL에서 사용하기 위해 선언
            // ──────────────────────────────────────

            // 텍스처 & 샘플러 (CBUFFER 밖에 선언!)
            TEXTURE2D(_MainTex);            // 텍스처 오브젝트 (매크로 사용)
            SAMPLER(sampler_MainTex);       // 텍스처 샘플러 (매크로 사용)

            // CBUFFER : SRP Batcher 지원을 위해 필수!
            // 같은 CBUFFER를 사용하는 머티리얼들은 배칭되어 성능 향상
            CBUFFER_START(UnityPerMaterial)
                half4  _TintColor;          // 틴트 색상 (Properties와 이름 동일!)
                float  _Intensity;          // 강도 값 (Range → float)
                float4 _MainTex_ST;         // 텍스처 Tiling(xy) & Offset(zw) - "_ST" 접미사 필수!
            CBUFFER_END

            // ──────────────────────────────────────
            // [VertexInput] : GPU가 메시에서 읽어올 데이터
            // ──────────────────────────────────────
            struct VertexInput
            {
                float4 vertex : POSITION;   // 버텍스 위치 (로컬 좌표)
                float2 uv     : TEXCOORD0;  // UV 좌표 (텍스처 매핑용)
            };

            // ──────────────────────────────────────
            // [VertexOutput] : 버텍스 → 픽셀 셰이더로 전달할 데이터
            // ──────────────────────────────────────
            struct VertexOutput
            {
                float4 vertex : SV_POSITION;  // 화면상 위치 (클립 좌표)
                float2 uv     : TEXCOORD0;    // UV 좌표 (프래그먼트로 전달)
            };

            // ──────────────────────────────────────
            // [버텍스 셰이더] : 각 정점마다 실행
            // 역할 : 3D 좌표 → 2D 화면 좌표로 변환
            // ──────────────────────────────────────
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                // 로컬 좌표 → 클립 좌표 변환 (MVP 행렬 적용)
                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                // UV 변환 : Tiling(xy) * UV + Offset(zw)
                // Material의 Tiling/Offset 값이 적용됨
                o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            // ──────────────────────────────────────
            // [픽셀(프래그먼트) 셰이더] : 각 픽셀마다 실행
            // 역할 : 최종 색상 결정
            // ──────────────────────────────────────
            half4 frag(VertexOutput i) : SV_Target
            {
                // 텍스처 샘플링 : UV 좌표로 텍스처에서 색상 추출 (매크로 사용)
                float4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // 최종 색상 = 텍스처 × 틴트 색상 × 강도
                float4 color = texColor * _TintColor * _Intensity;

                return color;
            }

            // ====== HLSL 코드 끝 ======
            ENDHLSL
        }
    }
}