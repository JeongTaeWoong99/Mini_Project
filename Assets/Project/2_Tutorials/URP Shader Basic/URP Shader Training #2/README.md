# URP Shader Training #2 - Alpha Cutout 및 렌더 스테이트

## 목차
- [학습 범위](#-학습-범위)
- [핵심 개념 정리](#-핵심-개념-정리)
- [Q&A 정리](#-qa-정리)
- [제작한 셰이더 목록](#-제작한-셰이더-목록)

---

## 학습 범위

![캡쳐.png](%EC%BA%A1%EC%B3%90.png)

**YouTube :** URP Shader Training #2-1 ~ #2-5

**PDF :** 17 ~ 27 페이지

**학습 날짜 :** 2026.01.21

---

## 핵심 개념 정리

### 1. Alpha Cutout (알파 컷아웃)

텍스처의 알파값을 기준으로 픽셀을 **완전 투명 or 완전 불투명** 처리하는 방식

```hlsl
// 핵심 코드
clip(color.a - _Alpha);  // 알파가 임계값보다 작으면 픽셀 버림
```

**Tags 설정 :**
```hlsl
Tags
{
    "RenderType" = "TransparentCutout"  // 알파 컷아웃 타입
    "Queue"      = "AlphaTest"          // 렌더링 순서 (2450)
}
```

**용도 :** 나뭇잎, 풀, 철망 등 "뚫린 부분"이 있는 텍스처

---

### 2. Render State (렌더 스테이트)

Pass 내에서 GPU의 렌더링 동작을 제어하는 설정들

| 옵션 | 역할 | 주요 값 |
|------|------|---------|
| **Blend** | 색상 혼합 방식 | `SrcAlpha OneMinusSrcAlpha` (알파 블렌딩) |
| **Cull** | 면 컬링 | `Off` / `Front` / `Back` |
| **ZWrite** | 깊이 버퍼 쓰기 | `On` / `Off` |
| **ZTest** | 깊이 테스트 | `LEqual` / `Always` 등 |
| **Offset** | 깊이 오프셋 | Z-Fighting 방지용 |
| **AlphaToMask** | MSAA 알파 커버리지 | `On` / `Off` |

---

### 3. [Enum] 속성으로 Inspector 드롭다운 만들기

```hlsl
// Unity 내장 Enum 사용
[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend", Float) = 1
[Enum(UnityEngine.Rendering.CullMode)]  _Cull("Cull Mode", Float) = 2

// 커스텀 Enum 정의
[Enum(Off, 0, On, 1)] _ZWrite("ZWrite", Float) = 1
```

**Pass에서 사용 :**
```hlsl
Blend [_SrcBlend] [_DstBlend]
Cull [_Cull]
ZWrite [_ZWrite]
```

---

### 4. 자주 쓰는 Blend 조합

| 효과 | Src Blend | Dst Blend | 설명 |
|------|-----------|-----------|------|
| **불투명** | One | Zero | 덮어쓰기 |
| **알파 블렌딩** | SrcAlpha | OneMinusSrcAlpha | 투명 오브젝트 |
| **Additive** | One | One | 빛/불꽃 효과 |
| **Multiply** | DstColor | Zero | 어둡게 합성 |

---

## Q&A 정리

### Q1. sampler2D와 Texture2D의 차이점은?

**A :** 텍스처와 샘플러의 결합 방식이 다릅니다.

| 방식 | 설명 | 사용 환경 |
|------|------|----------|
| `sampler2D` | 텍스처 + 샘플러가 합쳐진 형태 | Built-in RP (구버전) |
| `Texture2D` + `SamplerState` | 텍스처와 샘플러가 분리된 형태 | URP / HDRP (권장) |

**분리하는 이유 :**
- GPU에는 **샘플러 슬롯 개수 제한** (대부분 최대 16개)
- 분리하면 하나의 샘플러를 여러 텍스처에서 공유 가능 → 슬롯 절약!

```hlsl
// 구버전 (Built-in)
sampler2D _MainTex;
fixed4 col = tex2D(_MainTex, uv);

// 신버전 (URP) - 권장
TEXTURE2D(_MainTex);
SAMPLER(sampler_MainTex);
float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
```

---

### Q2. Sampler(샘플러)란 정확히 무엇인가요?

**A :** 텍스처에서 색상을 읽어올 때의 **규칙/설정**입니다.

**비유 :**
- 텍스처 = 📷 사진 (원본 이미지 데이터)
- 샘플러 = 🔍 돋보기 설정 (어떻게 볼지 결정)

**샘플러가 결정하는 것 :**

| 설정 | 역할 | 옵션 |
|------|------|------|
| **Filtering** | 확대/축소 시 보간 방식 | Point (픽셀), Bilinear (부드럽게) |
| **Wrapping** | UV가 0~1 범위 벗어날 때 | Repeat (반복), Clamp (늘리기), Mirror (거울) |

```hlsl
// Unity 인라인 샘플러 예시
SAMPLER(sampler_linear_repeat);   // 부드럽게 + 반복
SAMPLER(sampler_point_clamp);     // 픽셀 그대로 + 늘리기
```

---

### Q3. Alpha Cutout에서 clip() 함수는 어떻게 동작하나요?

**A :** `clip(x)` 함수는 x가 0 미만이면 해당 픽셀을 버립니다 (렌더링 안 함).

```hlsl
// Alpha Cutout 핵심 코드
clip(color.a - _Alpha);

// 동작 원리
// _Alpha = 0.5일 때 :
// 알파 0.8 → 0.8 - 0.5 = 0.3  → 렌더링 O
// 알파 0.3 → 0.3 - 0.5 = -0.2 → 버림 X
```

**_Alpha 값에 따른 변화 :**

| _Alpha | 효과 |
|--------|------|
| 0.0 | 모든 픽셀 보임 (알파 0인 것만 제외) |
| 0.5 | 알파 0.5 이상만 보임 |
| 1.0 | 아무것도 안 보임 |

**활용 :** _Alpha를 0→1로 애니메이션하면 **디졸브(Dissolve) 효과** 구현 가능!

---

### Q4. 텍스처 Alpha Source 설정의 차이점은?

**A :** Unity 텍스처 Import Settings에서 알파 채널의 소스를 지정합니다.

| 설정 | 동작 | 용도 |
|------|------|------|
| **None** | A = 1.0 고정 (알파 없음) | 알파 필요 없을 때 |
| **Input Texture Alpha** | 원본 이미지의 알파 사용 | PNG 투명 영역 등 |
| **From Gray Scale** | RGB 밝기 → 알파 변환 | 디졸브, 마스크 효과 |

---

### Q5. From Gray Scale 설정 시 왜 어두운 부분이 먼저 사라지나요?

**A :** "From Gray Scale" 설정은 **RGB의 밝기를 알파값으로 변환**하기 때문입니다.

```
원본 RGB 밝기          →    알파 채널로 변환
─────────────────          ─────────────────
밝은 픽셀 (흰색)       →    A = 1.0 (불투명) → 나중에 사라짐
중간 픽셀 (회색)       →    A = 0.5 (반투명) → 중간에 사라짐
어두운 픽셀 (검정)     →    A = 0.0 (투명)   → 먼저 사라짐
```

**중요 :** 원래 RGB(밝기)와 A(투명도)는 독립적인 값이지만, "From Gray Scale" 설정을 하면 **밝기 = 알파**가 되어서 어두운 부분이 먼저 사라지는 것처럼 보입니다.

**원본 알파를 사용하려면 :**
- Alpha Source : **Input Texture Alpha** 로 변경
- 단, 원본 이미지가 알파 채널을 가지고 있어야 함 (PNG, TGA, PSD 등)

---

## 제작한 셰이더 목록

| 셰이더 | 설명 | 파일 |
|--------|------|------|
| URPBasic_2 | Alpha Cutout 셰이더 | `URPBasic_2.shader` |
| URPBasic_3 | Render State 커스터마이징 셰이더 | `URPBasic_3.shader` |

---

**학습 완료 :** 2026.01.21
