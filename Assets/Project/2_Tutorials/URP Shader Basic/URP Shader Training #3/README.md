# URP Shader Training #3 - UV 활용 및 버텍스 애니메이션

## 목차
- [학습 범위](#-학습-범위)
- [참고 자료](#-참고-자료)
- [핵심 개념 정리](#-핵심-개념-정리)
- [Q&A 정리](#-qa-정리)
- [제작한 셰이더 목록](#-제작한-셰이더-목록)

---

## 학습 범위

![캡쳐.png](%EC%BA%A1%EC%B3%90.png)

**YouTube :** URP Shader Training #3-1 ~ #3-5

**PDF :** 28 ~ 40 페이지

**학습 날짜 :** 2026.01.24

---

## 추천 참고 자료

**감마 보정 관련 영상 :**
- [감마란 무엇인가? (YouTube)](https://www.youtube.com/watch?v=Xwlm5V-bnBc)

---

## 핵심 개념 정리

### 1. UV 좌표를 색상으로 시각화

UV 좌표를 그대로 색상으로 출력하여 UV 매핑 상태를 디버깅할 수 있음

```hlsl
// UV.x → Red, UV.y → Green
float4 color = float4(i.uv.x, i.uv.y, 0, 1);
```

**시각적 결과 :**
```
┌─────────────────────────┐
│ 검정(0,0) ──→ 빨강(1,0) │  X축 : Red (0→1)
│   │                     │
│   ↓                     │
│ 초록(0,1) ──→ 노랑(1,1) │  Y축 : Green (0→1)
└─────────────────────────┘
```

---

### 2. 감마 보정 (Gamma Correction)

인간의 눈은 밝기를 **비선형**으로 인식하기 때문에, 모니터는 감마 보정을 적용함

```hlsl
// 감마 보정 적용 (Linear → sRGB)
color = pow(i.uv.x, 2.2);  // 더 어두워짐

// 삼항연산자로 비교
color = i.uv.y > 0.5 ? pow(i.uv.x, 2.2) : i.uv.x;
```

**2.2 지수의 의미 :**
- sRGB 색공간의 표준 감마 값
- Linear 색공간을 모니터에 올바르게 표시하기 위한 변환

---

### 3. 텍스처 블렌딩 - lerp()

두 개의 텍스처를 비율에 따라 부드럽게 섞는 함수

```hlsl
// lerp(A, B, t) : A와 B를 t 비율로 보간
// t = 0 → A, t = 1 → B
float4 color = lerp(tex01, tex02, i.uv.x);  // 왼쪽→tex01, 오른쪽→tex02
```

**Splatting 기법 :**
```
┌─────────────────────────────────────┐
│ ← tex01 (x=0)    tex02 (x=1) →     │
│         부드럽게 블렌딩              │
└─────────────────────────────────────┘
```

---

### 4. 마스크 텍스처 블렌딩

마스크 텍스처의 특정 채널(R, G, B 등)을 사용하여 블렌딩 영역 지정

```hlsl
// 마스크 R채널로 블렌딩
float4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MainTex, i.uv);
float4 color = lerp(tex01, tex02, mask.r);

// Add 방식 (각 채널이 중복되지 않아야 깔끔)
// float4 color = tex01 * mask.r + tex02 * mask.g;
```

**용도 :** 지형 텍스처 블렌딩, 캐릭터 커스터마이징 등

---

### 5. UV Scroll - _Time 내장 변수

Unity의 `_Time` 변수를 사용하여 UV를 시간에 따라 이동

```hlsl
// _Time 컴포넌트
// _Time.x = t/20 (느림)
// _Time.y = t (1배속)
// _Time.z = t*2 (2배속)
// _Time.w = t*3 (3배속)

o.uv.x += _Time.x * _ScrollSpeedX;  // X축 스크롤
o.uv.y += _Time.x * _ScrollSpeedY;  // Y축 스크롤
```

**용도 :** 물, 용암, 컨베이어 벨트, 구름 등

---

### 6. Flow Map - UV 왜곡 애니메이션

Flow Map 텍스처의 RG 채널을 사용하여 UV를 왜곡하는 기법

```hlsl
// Flow Map 샘플링 (R=X방향, G=Y방향)
float4 flow = SAMPLE_TEXTURE2D(_Flowmap, sampler_MainTex, i.uv);

// frac() : 소수점 부분만 반환 (0~1 반복)
uv += frac(_Time.x * _FlowTime) + flow.rg * _FlowIntensity;
```

**Flow Map 텍스처 색상별 의미 :**
- 회색 (128, 128) → (0.5, 0.5) → 흐름 없음
- 빨강 (255, 128) → (1.0, 0.5) → 오른쪽으로 흐름
- 초록 (128, 255) → (0.5, 1.0) → 위쪽으로 흐름

**용도 :** 물결, 연기, 불꽃 등 자연스러운 흐름 표현

---

### 7. Vertex Shader Animation

버텍스 셰이더에서 정점 위치를 변형하여 메시 애니메이션 구현

```hlsl
VertexOutput vert(VertexInput v)
{
    VertexOutput o;

    // 먼저 클립 좌표로 변환
    o.vertex = TransformObjectToHClip(v.vertex.xyz);

    // 클립 좌표의 y값 변경 (sin 파동)
    o.vertex.y += sin(v.vertex.x + v.vertex.z + _Time.y) * _WaveIntensity;

    return o;
}
```

**중요 :** `TransformObjectToHClip` **이후에** `o.vertex.y`를 변경!

---

### 8. tex2Dlod - 버텍스 셰이더에서 텍스처 샘플링

버텍스 셰이더에서는 일반 텍스처 샘플링이 불가능하여 `tex2Dlod` 사용

```hlsl
// sampler2D로 선언 (tex2Dlod용)
sampler2D _NoiseTex;

// 버텍스 셰이더에서 샘플링
// float4(uv, 0, 0) : uv좌표, 미사용, LOD레벨(0=최고해상도)
half4 noise = tex2Dlod(_NoiseTex, float4(o.uv, 0, 0));

// 노이즈로 불규칙한 파동 생성
o.vertex.y += sin(_Time.y + v.vertex.x + v.vertex.z) * noise.r * _WaveIntensity;
```

**tex2Dlod vs tex2D 차이점 :**
- `tex2D` : 픽셀 셰이더 전용, LOD 자동 계산
- `tex2Dlod` : 버텍스 셰이더 사용 가능, LOD 수동 지정 필수

**용도 :** 깃발, 물결, 풀 흔들림, 천 등

---

## Q&A 정리

### Q1. Flow Map이란 무엇인가요?

**A :** Flow Map은 **흐름 방향을 저장한 텍스처**입니다.

```
일반 텍스처 : RGB = 색상 정보
Flow Map   : R = X방향 흐름, G = Y방향 흐름
```

**비유 :**
- 일반 텍스처 = 📷 사진 (색상 데이터)
- Flow Map = 🧭 방향 지도 (어느 쪽으로 흐를지 지정)

**색상별 의미 :**
- 회색 (128, 128) → 중립, 흐름 없음 (정지)
- 빨강 (255, 128) → 오른쪽으로 흐름
- 초록 (128, 255) → 위쪽으로 흐름
- 검정 (0, 0) → 왼쪽 아래로 흐름

**활용 :** 물 표면, 연기, 용암 등 유체의 흐름 방향을 지정하여 자연스러운 애니메이션 구현

---

### Q2. frac() 사용 시 0.999→0 점프할 때 파핑 현상이 없나요?

**A :** 실제로 **PDF 코드는 파핑 현상이 있는 단순화된 버전**입니다.

```
frac() 값의 변화 :

1.0 │      ╱│      ╱│
    │     ╱ │     ╱ │
0.5 │    ╱  │    ╱  │
    │   ╱   │   ╱   │
0.0 ┼──╱────┴──╱────┴──→ 시간
         ↑        ↑
      여기서 0.999→0 점프! (파핑 발생)
```

**지금 잘 안 보이는 이유 :**
- `_FlowIntensity`가 낮아서 (0.5) 점프가 덜 눈에 띔
- 텍스처가 타일링되어 있으면 UV가 1→0이 되어도 연속적으로 보임
- `_FlowTime`이 느리면 점프 순간을 놓칠 수 있음

**실제 게임에서의 해결법 - Two-Layer Blending :**

```hlsl
// 위상차가 있는 두 개의 샘플을 블렌딩
float phase0 = frac(_Time.x * _FlowTime);
float phase1 = frac(_Time.x * _FlowTime + 0.5);  // 0.5 위상차

float2 uv0 = i.uv + flow.rg * phase0 * _FlowIntensity;
float2 uv1 = i.uv + flow.rg * phase1 * _FlowIntensity;

float4 tex0 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv0);
float4 tex1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv1);

// 블렌드 가중치 (삼각파)
float blend = abs((phase0 - 0.5) * 2.0);  // 0→1→0 반복

float4 color = lerp(tex0, tex1, blend);
```

**원리 :**
```
Layer 0 :  ╱│  ╱│  ╱│
Layer 1 :    ╱│  ╱│  ╱│  (0.5 위상차)
           ──────────────
Blend   :  ∧  ∧  ∧  (삼각파로 부드럽게 전환)

→ 한 레이어가 점프할 때 다른 레이어가 보완!
```

**결론 :**
- PDF 코드 : 학습용 단순화 버전 (파핑 있음)
- 실제 게임 : Two-Layer Blending 사용 (파핑 없음)

---

## 제작한 셰이더 목록

**URPBasic_4** - UV 시각화 + 감마 보정
- 주요 기능 : UV→색상, pow(2.2)
- 파일 : `URPBasic_4.shader`

**URPBasic_5** - 텍스처 블렌딩
- 주요 기능 : lerp(), 마스크 블렌딩
- 파일 : `URPBasic_5.shader`

**URPBasic_6** - UV Scroll + Flow Map
- 주요 기능 : _Time, frac(), flow.rg
- 파일 : `URPBasic_6.shader`

**URPBasic_7** - 버텍스 애니메이션
- 주요 기능 : sin(), tex2Dlod
- 파일 : `URPBasic_7.shader`

---

**학습 완료 :** 2026.01.24
