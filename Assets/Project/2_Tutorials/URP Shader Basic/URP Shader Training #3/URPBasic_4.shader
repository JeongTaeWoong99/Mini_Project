// ============================================================================
// URP 기본 셰이더 #4 - UV 정보를 픽셀 셰이더에 출력 + 감마 보정
// ============================================================================
//
// ⚠️ 수정 안내 :
// PDF 4번 셰이더가 구 버전(약 4년 전) 기준으로 작성되어 있어서,
// 최신 URP 스타일로 수정한 버전입니다.
//
// 주요 변경 사항 :
// 1. pragma 지시문 현대화 (#pragma prefer_hlslcc gles 등 제거)
// 2. 코드 포맷팅 및 가독성 개선
// 3. 주석 추가
//
// 📝 이 셰이더의 특징 :
// - UV 좌표를 색상으로 시각화하는 디버그용 셰이더
// - U값 → Red, V값 → Green 채널로 출력
// - 텍스처 없이 UV 매핑 상태를 확인할 수 있음
// - [선택] 감마 보정 모드 : Y > 0.5 영역에 pow(x, 2.2) 적용
//
// ============================================================================

// [셰이더 경로] : Material에서 셰이더 선택 시 "URPTraining" 폴더 안에 "URPBasic_4"로 표시됨
Shader "URPTraining/URPBasic_4"
{
    // [Properties] : Material Inspector에 노출할 변수들
    Properties
    {
        // ─── 감마 보정 설정 ───
        [Toggle(_GAMMA_CORRECTION)] _GammaCorrection("Gamma Correction", Float) = 0  // 감마 보정 On/Off
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
            #pragma shader_feature_local _GAMMA_CORRECTION

            // [include] : URP 라이팅 함수들 가져오기
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

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

                o.vertex = TransformObjectToHClip(v.vertex.xyz);    // 로컬 좌표 → 클립 좌표 변환 (MVP 행렬 적용)

                o.uv = v.uv.xy;                                     // UV 좌표를 그대로 전달

                return o;
            }

            // ──────────────────────────────────────
            // [픽셀(프래그먼트) 셰이더] : 각 픽셀마다 실행
            // 역할 : UV 좌표를 색상으로 시각화
            // ──────────────────────────────────────
            half4 frag(VertexOutput i) : SV_Target
            {
                float4 color;

                // ─── 감마 보정 모드 ───
                // Y > 0.5 : 감마 보정 적용 (pow 2.2 - 더 어두움)
                // Y <= 0.5 : 선형 (Linear) 그대로 출력
                #if defined(_GAMMA_CORRECTION)
                    // 삼항연산자 버전 : 조건문 ? 참일때 값 : 거짓일때 값
                    color = i.uv.y > 0.5 ? pow(i.uv.x, 2.2) : i.uv.x;

                    // ↓ if-else 버전 (동일한 결과) ↓
                    // if (i.uv.y > 0.5)
                    // {
                    //     color = pow(i.uv.x, 2.2);  // 감마 보정 (2.2 지수)
                    // }
                    // else
                    // {
                    //     color = i.uv.x;            // 선형 그대로
                    // }

                // ─── 기본 UV 시각화 모드 ───
                #else
                    // U (가로) → Red 채널
                    // V (세로) → Green 채널
                    // Blue = 0, Alpha = 1
                    color = float4(i.uv.x, i.uv.y, 0, 1);
                #endif

                return color;
            }

            // ====== HLSL 코드 끝 ======
            ENDHLSL
        }
    }
}
