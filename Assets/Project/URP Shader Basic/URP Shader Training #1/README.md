# URP Shader Training #1 - 기본 구조 및 Properties

## 목차
- [학습 범위](#-학습-범위)
- [핵심 개념 정리](#-핵심-개념-정리)
- [Q&A 정리](#-qa-정리)

---

## 학습 범위

**YouTube :** URP Shader Training #1-1 ~ #1-5

**PDF :** 1 ~ 16 페이지

**학습 날짜 :** 2026.01.15

---

## 핵심 개념 정리

### 1. URP 셰이더 기본 구조

```hlsl
Shader "경로/셰이더명"
{
    Properties { }      // Material Inspector에 노출할 변수
    SubShader
    {
        Tags { }        // 렌더링 설정
        Pass
        {
            HLSLPROGRAM
            // 셰이더 코드
            ENDHLSL
        }
    }
}
```

### 2. Tags 설정

| Tag | 설명 | 예시 |
|-----|------|------|
| `RenderPipeline` | 사용할 파이프라인 | `"UniversalPipeline"` |
| `RenderType` | 렌더링 타입 | `"Opaque"`, `"Transparent"` |
| `Queue` | 렌더링 순서 | `"Geometry"` (2000), `"Transparent"` (3000) |
| `LightMode` | 라이팅 모드 | `"UniversalForward"` |

### 3. HLSL 코드 구조

```hlsl
#pragma vertex vert      // 버텍스 셰이더 함수 지정
#pragma fragment frag    // 프래그먼트 셰이더 함수 지정

struct VertexInput { };  // 입력 데이터 구조체
struct VertexOutput { }; // 출력 데이터 구조체

VertexOutput vert() { }  // 버텍스 셰이더
half4 frag() { }         // 프래그먼트 셰이더
```

### 4. Properties 타입

| 타입 | 설명 | 예시 |
|------|------|------|
| `Color` | RGBA 색상 | `_Color("Color", Color) = (1,1,1,1)` |
| `Float` | 실수 값 | `_Float("Float", Float) = 1.0` |
| `Range` | 슬라이더 | `_Range("Range", Range(0, 1)) = 0.5` |
| `2D` | 텍스처 | `_MainTex("Texture", 2D) = "white" {}` |

> **주의 :** 타입은 반드시 **대문자**로 시작! (`color` X → `Color` O)

### 5. 텍스처 사용법

```hlsl
// 변수 선언
Texture2D _MainTex;             // 텍스처 오브젝트
SamplerState sampler_MainTex;   // 샘플러 (sampler_ + 텍스처명)
float4 _MainTex_ST;             // Tiling/Offset (_ST 접미사)

// UV 변환
o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;

// 텍스처 샘플링
float4 color = _MainTex.Sample(sampler_MainTex, i.uv);
```

---

## Q&A 정리

### Q1. 투명 셰이더는 RenderType만 바꾸면 되나요?

**A :** 아니요. 투명 셰이더는 3가지를 모두 변경해야 합니다 :

```hlsl
Tags
{
    "RenderType" = "Transparent"  // 1. 렌더 타입
    "Queue"      = "Transparent"  // 2. 렌더링 순서 (3000)
}

// 3. Pass 안에 블렌딩 설정 추가
Blend SrcAlpha OneMinusSrcAlpha
ZWrite Off
```

---

### Q2. Queue는 픽셀/프래그먼트 셰이더를 지정하는 건가요?

**A :** 아니요. **Queue는 렌더링 순서**를 지정합니다.

| 구분 | 설정 방법 |
|------|-----------|
| 렌더링 순서 | `"Queue" = "Geometry"` (Tags) |
| 프래그먼트 셰이더 | `#pragma fragment frag` (HLSL) |

Queue 값 :
- Background : 1000 (스카이박스)
- Geometry : 2000 (불투명 오브젝트)
- Transparent : 3000 (투명 오브젝트)

---

### Q3. Deferred 라이팅은 LightMode만 바꾸면 되나요?

**A :** 아니요. 추가 설정이 필요합니다 :

1. `LightMode = "UniversalGBuffer"` 설정
2. GBuffer 출력용 추가 Pass 작성
3. URP Asset에서 Deferred 렌더링 활성화

> 일반적으로 Forward가 기본이며, Deferred는 고급 주제입니다.

---

### Q4. Frame Debugger에서 CopyDepth, BlitFinalToBackBuffer는 뭔가요?

**A :** URP 렌더링 파이프라인의 내부 단계입니다.

| 단계 | 역할 |
|------|------|
| **DrawOpaqueObjects** | 불투명 오브젝트 렌더링 |
| **DrawSkybox** | 스카이박스 렌더링 |
| **CopyDepth** | 깊이 버퍼를 텍스처로 복사 (포스트 프로세싱용) |
| **BlitFinalToBackBuffer** | 최종 이미지를 화면에 출력 |

---

### Q6. Material Inspector의 "양면 전역 조명"은 뭔가요?

**A :** Global Illumination (GI) 계산 시 메시의 양면을 사용할지 설정합니다.

| 설정 | 동작 |
|------|------|
| 체크 해제 | 앞면만 GI 계산 |
| 체크 | 앞면 + 뒷면 모두 GI 계산 |

주로 얇은 천, 나뭇잎 등에 사용됩니다.

> **참고 :** Baked GI에만 영향을 미치며, 현재 Unlit 셰이더에서는 효과 없음.

---

**학습 완료 :** 2026.01.15
