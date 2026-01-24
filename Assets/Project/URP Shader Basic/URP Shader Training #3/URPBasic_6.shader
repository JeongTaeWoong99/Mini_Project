// ============================================================================
// URP 기본 셰이더 #6 - UV Scroll + Flow Map (텍스처 애니메이션)
// ============================================================================
//
// ⚠️ 수정 안내 :
// PDF 6번 셰이더가 구 버전(약 4년 전) 기준으로 작성되어 있어서,
// 최신 URP 스타일로 수정한 버전입니다.
//
// 주요 변경 사항 :
// 1. pragma 지시문 현대화 (#pragma prefer_hlslcc gles 등 제거)
// 2. TEXTURE2D / SAMPLER 매크로 사용
// 3. SAMPLE_TEXTURE2D 매크로 사용
// 4. CBUFFER 추가 (SRP Batcher 지원)
// 5. 스크롤 속도 조절 속성 추가 (_ScrollSpeedX, _ScrollSpeedY)
// 6. Flow Map 기능 추가 (토글로 선택 가능)
//
// 📝 이 셰이더의 특징 :
// - [기본] UV Scroll : _Time을 사용하여 UV를 이동시켜 텍스처 스크롤
// - [선택] Flow Map : Flow 텍스처의 RG 채널로 UV를 왜곡하여 자연스러운 흐름 표현
// - 물, 용암, 연기, 불 등 다양한 이펙트에 활용
//
// ============================================================================

// [셰이더 경로] : Material에서 셰이더 선택 시 "URPTraining" 폴더 안에 "URPBasic_6"로 표시됨
Shader "URPTraining/URPBasic_6"
{
    // [Properties] : Material Inspector에 노출할 변수들
    Properties
    {
        // ─── 텍스처 설정 ───
        _MainTex("Main Texture", 2D)          = "white" {}  // 메인 텍스처

        // ─── 색상 설정 ───
        _TintColor("Tint Color", Color)       = (1, 1, 1, 1)  // 틴트 색상
        _Intensity("Intensity", Range(0, 1))  = 1.0           // 강도

        // ─── UV 스크롤 설정 ───
        [Header(UV Scroll Settings)]
        _ScrollSpeedX("Scroll Speed X", Float) = 1.0  // X축 스크롤 속도
        _ScrollSpeedY("Scroll Speed Y", Float) = 0.0  // Y축 스크롤 속도

        // ─── Flow Map 설정 ───
        [Header(Flow Map Settings)]
        [Toggle(_USE_FLOWMAP)] _UseFlowmap("Use Flow Map", Float)    = 0          // Flow Map On/Off
        [NoScaleOffset] _Flowmap("Flow Map", 2D)                     = "gray" {}  // Flow Map 텍스처 (RG채널 사용)
        _FlowIntensity("Flow Intensity", Range(0, 1))                = 0.5        // Flow 강도
        _FlowTime("Flow Time", Range(0, 10))                         = 1.0        // Flow 속도
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
            #pragma target   3.5     // Shader Model 3.5 이상
            #pragma vertex   vert    // 버텍스 셰이더 함수명 지정
            #pragma fragment frag    // 픽셀 셰이더 함수명 지정

            // [shader_feature] : 키워드 기반 분기 (Material마다 다른 셰이더 변형 생성)
            #pragma shader_feature_local _USE_FLOWMAP

            // [include] : URP 라이팅 함수들 가져오기
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ──────────────────────────────────────
            // [변수 선언] : Properties에서 선언한 변수를 HLSL에서 사용하기 위해 선언
            // ──────────────────────────────────────

            // 텍스처 & 샘플러 (CBUFFER 밖에 선언!)
            TEXTURE2D(_MainTex);            // 메인 텍스처 오브젝트
            TEXTURE2D(_Flowmap);            // Flow Map 텍스처 오브젝트
            SAMPLER(sampler_MainTex);       // 텍스처 샘플러 (공유)

            // CBUFFER : SRP Batcher 지원을 위해 필수!
            // 같은 CBUFFER를 사용하는 머티리얼들은 배칭되어 성능 향상
            CBUFFER_START(UnityPerMaterial)
                half4  _TintColor;          // 틴트 색상
                float  _Intensity;          // 강도 값
                float4 _MainTex_ST;         // 텍스처 Tiling(xy) & Offset(zw)
                float  _ScrollSpeedX;       // X축 스크롤 속도
                float  _ScrollSpeedY;       // Y축 스크롤 속도
                float  _FlowIntensity;      // Flow 강도
                float  _FlowTime;           // Flow 속도
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
            // 역할 : 3D 좌표 → 2D 화면 좌표로 변환 + UV 변환
            // ──────────────────────────────────────
            VertexOutput vert(VertexInput v)
            {
                VertexOutput o;

                // 로컬 좌표 → 클립 좌표 변환 (MVP 행렬 적용)
                o.vertex = TransformObjectToHClip(v.vertex.xyz);

                // UV 변환 : Tiling(xy) * UV + Offset(zw)
                o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                // ─── 기본 UV 스크롤 (Flow Map 미사용 시) ───
                #if !defined(_USE_FLOWMAP)
                    // _Time : Unity 내장 변수 (시간)
                    // _Time.x = t/20 (느림), _Time.y = t, _Time.z = t*2, _Time.w = t*3 (빠름)
                    o.uv.x += _Time.x * _ScrollSpeedX;  // X축 스크롤
                    o.uv.y += _Time.x * _ScrollSpeedY;  // Y축 스크롤
                #endif

                return o;
            }

            // ──────────────────────────────────────
            // [픽셀(프래그먼트) 셰이더] : 각 픽셀마다 실행
            // 역할 : 최종 색상 결정
            // ──────────────────────────────────────
            half4 frag(VertexOutput i) : SV_Target
            {
                float2 uv = i.uv;

                // ─── Flow Map 모드 ───
                // Flow Map의 RG 채널을 사용하여 UV를 왜곡
                // 연기, 물, 불 등 자연스러운 흐름 효과에 사용
                #if defined(_USE_FLOWMAP)
                    // Flow Map 샘플링 (RG 채널이 UV 왜곡 방향)
                    float4 flow = SAMPLE_TEXTURE2D(_Flowmap, sampler_MainTex, i.uv);

                    // frac() : 소수점 부분만 반환 (0~1 반복)
                    // flow.rg : R=X방향, G=Y방향 왜곡량
                    uv += frac(_Time.x * _FlowTime) + flow.rg * _FlowIntensity;
                #endif

                // 텍스처 샘플링 (변형된 UV 사용)
                float4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // 틴트 색상 및 강도 적용
                color.rgb *= _TintColor.rgb * _Intensity;

                return color;
            }

            // ====== HLSL 코드 끝 ======
            ENDHLSL
        }
    }
}
