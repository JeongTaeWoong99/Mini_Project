// ============================================================================
// URP 기본 셰이더 #3 - Render State 커스터마이징 셰이더
// ============================================================================
//
// ⚠️ 수정 안내 :
// PDF 3번 셰이더가 구 버전(약 4년 전) 기준으로 작성되어 있어서,
// 최신 URP 스타일로 수정한 버전입니다.
//
// 주요 변경 사항 :
// 1. pragma 지시문 현대화 (#pragma prefer_hlslcc gles 등 제거)
// 2. TEXTURE2D / SAMPLER 매크로 사용
// 3. SAMPLE_TEXTURE2D 매크로 사용
// 4. CBUFFER 추가 (SRP Batcher 지원)
//
// 📝 이 셰이더의 특징 :
// - Blend, Cull, ZWrite, ZTest 등 렌더 스테이트를 Inspector에서 조절 가능
// - [Enum] 속성을 사용하여 드롭다운 메뉴로 옵션 선택
// - 투명(Transparent) 오브젝트용 셰이더
// - AlphaToMask, Offset 등 고급 옵션도 지원
//
// ============================================================================

// [셰이더 경로] : Material에서 셰이더 선택 시 "URPTraining" 폴더 안에 "URPBasic_3"으로 표시됨
Shader "URPTraining/URPBasic_3"
{
    // [Properties] : Material Inspector에 노출할 변수들
    Properties
    {
        // ─── 기본 설정 ───
        _TintColor("Tint Color" , Color)       = (1, 1, 1, 1)  // 틴트 색상 (RGBA)
        _Intensity("Intensity"  , Range(0, 1)) = 0.5              // 강도 슬라이더 (0~1)
        _MainTex("Main Texture" , 2D)          = "white" {}       // 메인 텍스처 (기본값 : 흰색)
        _Alpha("Alpha"          , Range(0, 1)) = 0.5              // 전체 알파 
        
        // ─── 렌더 스테이트 설정 (Enum 드롭다운) ───
        [Header(Blend Mode)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1  // 소스 블렌드 (One)
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend", Float) = 0  // 대상 블렌드 (Zero)

        [Header(Culling and Depth)]
        [Enum(UnityEngine.Rendering.CullMode)]        _Cull("Cull Mode", Float) = 1  // 컬링 (Front)
        [Enum(Off, 0, On, 1)]                         _ZWrite("ZWrite", Float)  = 0  // 깊이 쓰기 (Off)
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float)    = 0  // 깊이 테스트 (Disabled)

        [Header(Advanced)]
        _Factor("Offset Factor",                         Int)   = 0  // 깊이 오프셋 Factor
        _Units("Offset Units",                           Int)   = 0  // 깊이 오프셋 Units
        [Enum(Off, 0, On, 1)] _Mask("Alpha to Coverage", Float) = 0  // MSAA 알파 커버리지
    }

    // [SubShader] : 실제 렌더링 방법을 정의하는 블록
    SubShader
    {
        // [Tags] : 렌더링 설정
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"  // URP 사용 선언
            "RenderType"     = "Transparent"        // 투명 타입
            "Queue"          = "Transparent"        // 렌더링 순서 (3000) - 불투명 이후
        }

        // [Pass] : 한 번의 드로우콜로 실행되는 렌더링 단위
        Pass
        {
            // ─── 렌더 스테이트 (Properties의 값을 사용) ───
            Blend       [_SrcBlend] [_DstBlend]  // 블렌딩 모드 (소스 * Src + 대상 * Dst)
            Cull        [_Cull]                  // 컬링 모드 (Off/Front/Back)
            ZWrite      [_ZWrite]                // 깊이 버퍼 쓰기 (On/Off)
            ZTest       [_ZTest]                 // 깊이 테스트 (Less/LEqual/Greater 등)
            Offset      [_Factor], [_Units]      // 깊이 오프셋 (Z-Fighting 방지)
            AlphaToMask [_Mask]                  // MSAA 알파 커버리지

            Name "Universal Forward"
            Tags { "LightMode" = "UniversalForward" }

            // ====== HLSL 코드 시작 ======
            HLSLPROGRAM

            // [pragma] : 컴파일러 지시문
            #pragma target   3.5     // Shader Model 3.5 이상
            #pragma vertex   vert    // 버텍스 셰이더 함수명 지정
            #pragma fragment frag    // 픽셀 셰이더 함수명 지정

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
                float  _Alpha;              // 전체 알파값
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
            // 역할 : 최종 색상 결정 + 알파 적용
            // ──────────────────────────────────────
            half4 frag(VertexOutput i) : SV_Target
            {
                // 텍스처 샘플링 : UV 좌표로 텍스처에서 색상 추출 (매크로 사용)
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                // RGB에 틴트 색상과 강도 적용
                color.rgb *= _TintColor.rgb * _Intensity;

                // 알파값 조절 : 텍스처 알파 × 전체 알파
                color.a = color.a * _Alpha;

                return color;
            }

            // ====== HLSL 코드 끝 ======
            ENDHLSL
        }
    }
}
