# URP Shader Training #4 - 라이팅 기초 및 Toon/Rim Light

## 목차
- [학습 범위](#-학습-범위)
- [핵심 개념 정리](#-핵심-개념-정리)
- [Q&A 정리](#-qa-정리)
- [제작한 셰이더 목록](#-제작한-셰이더-목록)

---

## 학습 범위

![캡쳐.png](%EC%BA%A1%EC%B3%90.png)

**YouTube :** URP Shader Training #4-1 ~ #4-5

**PDF :** 41 ~ 53 페이지

**학습 날짜 :** 2026.01.25

---

## 핵심 개념 정리

### 1. World Position을 색상으로 표현

버텍스의 월드 좌표를 RGB 색상으로 출력하여 공간 위치를 시각화

```hlsl
// 보간기에 color 추가
struct VertexOutput
{
    float4 vertex : SV_POSITION;
    float3 color  : COLOR;  // 월드 좌표 저장용
};

// 버텍스 셰이더에서 월드 좌표 계산
o.color = TransformObjectToWorld(v.vertex.xyz);

// 픽셀 셰이더에서 색상으로 출력
color.rgb *= i.color;
```

**시각적 결과 :**
```
X축 = Red, Y축 = Green, Z축 = Blue
→ 오브젝트 위치에 따라 색상이 변함
```

---

### 2. Light Vector를 활용한 Lambert 라이팅

노멀과 라이트 방향의 내적으로 기본 라이팅 계산

```hlsl
// VertexInput에 노멀 추가
float3 normal : NORMAL;

// 노멀 월드 변환 (균등/비균등 스케일 자동 처리)
o.normal = TransformObjectToWorldNormal(v.normal);

// Lambert 공식 : dot(N, L)
float NdotL = saturate(dot(i.normal, lightDir));
color.rgb *= NdotL * _MainLightColor.rgb;
```

**Lambert 공식 :**

| 각도(θ)  | 0°  | 45° | 90° | 180° |
|----------|-----|-----|-----|------|
| dot(N,L) | 1   | 0.7 | 0   | -1   |
| saturate | 1   | 0.7 | 0   | 0    |

---

### 3. Per-Vertex vs Per-Pixel Lighting

라이팅 계산 위치에 따른 품질 차이

**Per-Vertex (저품질) :**
```hlsl
// 버텍스 셰이더에서 라이팅 계산
o.light = saturate(dot(o.normal, lightDir)) * lightColor;

// 픽셀 셰이더는 보간된 "색상"을 그대로 출력
color.rgb *= i.light;
```

**Per-Pixel (고품질) :**
```hlsl
// 픽셀 셰이더에서 라이팅 계산
// "노멀"이 보간되어 픽셀마다 정확한 계산
color.rgb *= saturate(dot(i.normal, lightDir)) * lightColor;
```

**핵심 차이 :**

| 모드       | 보간 대상   | 계산 횟수  | 품질     |
|------------|-------------|------------|----------|
| Per-Vertex | 색상 (결과) | 버텍스 수  | 각짐     |
| Per-Pixel  | 노멀 (재료) | 픽셀 수    | 부드러움 |

---

### 4. Toon Lighting - 삼항연산자

카툰 스타일의 딱딱한 명암 경계 표현

```hlsl
// 기본 Toon : 빛 받으면 밝음, 아니면 Ambient
float3 toonLight = NdotL > 0 ? lightColor : _AmbientColor.rgb;
```

**시각적 효과 :**
```
Lambert (부드러운 그라데이션)    Toon (딱딱한 경계)

    ░░▒▒▓▓██                       ████░░░░
```

---

### 5. Toon Lighting - ceil() 함수 (Banding)

라이트 영역을 계단식으로 분할

```hlsl
// ceil(x) = 올림, floor(x) = 내림, round(x) = 반올림
float3 toonLight = ceil(NdotL * _LightWidth) / _LightStep * lightColor;
float3 ambient = NdotL > 0 ? float3(0,0,0) : _AmbientColor.rgb;
color.rgb *= toonLight + ambient;
```

**계산 예시 (width=3, step=3) :**
```
NdotL : 0.0  0.3  0.5  0.7  1.0
ceil  :  0    1    2    3    3
/step :  0   0.33 0.66  1    1
```

---

### 6. Toon Lighting - Ramp Texture

NdotL 값을 UV 좌표로 변환하여 Ramp 텍스처 샘플링

```hlsl
// -1~1 → 0~1 변환 (UV 좌표용)
float rawNdotL = dot(i.normal, lightDir);  // saturate 없이!
float halfNdotL = rawNdotL * 0.5 + 0.5;

// Ramp 텍스처 샘플링 (U = halfNdotL, V = 0 고정)
float3 ramp = SAMPLE_TEXTURE2D(_RampTex, sampler, float2(halfNdotL, 0)).rgb;
```

**Ramp 텍스처 구조 (512x32) :**
```
┌────────────────────────────────────────┐
│ 어두운색 ━━━━━━━━━━━━━━━━━━ 밝은색    │  ← 가로 방향 그라데이션!
└────────────────────────────────────────┘
   U=0                              U=1
   (뒷면)                          (정면)
```

**주의 :** Ramp 텍스처는 반드시 **가로 방향 그라데이션**이어야 함!

---

### 7. Rim Light - Camera Vector 활용

가장자리 발광 효과 (Fresnel 유사)

```hlsl
// 카메라 방향 벡터 계산
float3 worldPos = TransformObjectToWorld(v.vertex.xyz);
o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);

// Rim 계산
half face = saturate(dot(i.viewDir, i.normal));  // 정면=1, 측면=0
half3 rim = 1.0 - pow(face, _RimPower);          // 정면=0, 측면=1

// Emissive로 추가
color.rgb += rim * _RimIntensity * _RimColor.rgb;
```

**Rim Light 원리 :**
```
        카메라 👁️
          │
          ▼
     ╭─────────╮
    /│  정면   │\     ← 측면 : dot≈0 → rim=1 (밝음!)
   / │ (dot=1) │ \
  │  │  rim=0  │  │
   \ │(어두움) │ /
    \│         │/     ← 측면 : dot≈0 → rim=1 (밝음!)
     ╰─────────╯
```

**Power 값의 효과 :**
- Power 작음 → Rim 영역 넓음
- Power 큼 → Rim 영역 좁음 (날카로운 테두리)

---

## Q&A 정리

### Q1. Per-Vertex와 Per-Pixel의 품질 차이는 왜 발생하나요?

**A :** **보간되는 대상**이 다르기 때문입니다.

```
Per-Vertex :
  버텍스 3개에서 "색상" 계산 → 내부 픽셀은 색상 보간
  → 결과물이 보간되므로 라이팅 디테일 손실

Per-Pixel :
  버텍스 3개에서 "노멀" 전달 → 내부 픽셀은 노멀 보간 → 각 픽셀에서 계산
  → 재료가 보간되므로 픽셀마다 정확한 라이팅
```

**비유 :**
- Per-Vertex = 색칠된 종이를 섞기 (단순 혼합)
- Per-Pixel = 물감을 섞어 새로 색칠 (정밀 혼합)

---

### Q2. Ramp 텍스처가 왜 가로 방향이어야 하나요?

**A :** 셰이더 코드에서 **V 좌표가 0으로 고정**되어 있기 때문입니다.

```hlsl
float3 ramp = SAMPLE_TEXTURE2D(_RampTex, sampler, float2(halfNdotL, 0));
                                                              ↑       ↑
                                                            U좌표   V=0 (고정!)
```

→ 텍스처의 **맨 아래 한 줄**만 샘플링됨
→ 세로 밴드 텍스처를 사용하면 거의 단색으로 보임!

---

### Q3. HDR Color는 무엇인가요?

**A :** High Dynamic Range Color로, **색상 값이 1을 초과**할 수 있습니다.

```
일반 Color : RGB 범위 0~1
HDR Color  : RGB 범위 0~∞ (예: RGB(2, 2, 2))
```

**활용 :** Post Processing의 Bloom 효과와 함께 사용하면 발광 효과!

---

## 제작한 셰이더 목록

**URPBasic_8** - World Position 색상화
- 주요 기능 : TransformObjectToWorld, 보간기 COLOR
- 파일 : `URPBasic_8.shader`

**URPBasic_9** - Lambert 라이팅
- 주요 기능 : TransformObjectToWorldNormal, dot(N,L), Per-Vertex/Per-Pixel 토글
- 파일 : `URPBasic_9.shader`

**URPBasic_10** - Toon Lighting
- 주요 기능 : 삼항연산자, ceil(), Ramp Texture, SampleSH()
- 파일 : `URPBasic_10.shader`

**URPBasic_11** - Rim Light
- 주요 기능 : _WorldSpaceCameraPos, View Direction, pow(), HDR Color
- 파일 : `URPBasic_11.shader`

---

**학습 완료 :** 2026.01.25
