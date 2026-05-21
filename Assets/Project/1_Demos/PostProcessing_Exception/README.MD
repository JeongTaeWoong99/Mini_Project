# 🎨 PostProcessing Exception

**기간 :** 2026.05.21 ~ 2026.05.22

---

## 목차

- [개요](#개요)
- [방법 한눈에 비교](#방법-한눈에-비교)
- [시도한 방법 목록](#시도한-방법-목록)
- [방법 1 : 카메라 오버레이 (Camera Stacking)](#방법-1--카메라-오버레이-camera-stacking)
- [방법 2 : Renderer Feature — Render Objects](#방법-2--renderer-feature--render-objects)
- [방법 3 : Renderer Feature — Push Pop Layer](#방법-3--renderer-feature--push-pop-layer)
- [방법 상세 분석 및 실무 가이드](#방법-상세-분석-및-실무-가이드)

---

## 📋 개요

URP 환경에서 **특정 UI나 오브젝트에만 포스트프로세싱을 선택적으로 적용하거나 제외**하는 방법을 탐구한 프로젝트다.

게임에서 전체 씬에 블룸(Bloom)이나 색보정(Color Adjustments) 같은 PP 효과를 주면서,
특정 요소에는 그 효과를 다르게 적용하고 싶은 경우를 해결한다.

<img width="700" alt="기본 결과 비교" src="1.%20Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EA%B2%8C%EC%9E%84%EB%B7%B0%29.png">

> 위 : 포스트프로세싱(Bloom) 적용 / 아래 : 미적용

---

## 🔎 방법 한눈에 비교

| 항목 | 방법 1<br>Camera Stacking | 방법 2<br>Render Objects | 방법 3<br>Push Pop Layer |
|------|:---:|:---:|:---:|
| **Bloom halo 배제** | ✅ 완전 배제 | ❌ halo 잔존 | ✅ 완전 배제 |
| **국소적 PP 배제** (Color Adjustments 등) | ✅ | ✅ | ✅ |
| **레이어별 독립 PP 설정** | ❌ | ❌ | ✅ |
| **3D 오브젝트 그림자** | ⚠️ ShadowProxy 필요 | ✅ 정상 | ⚠️ ShadowProxy 필요 |
| **구현 복잡도** | 낮음 | 낮음 | 중간 |
| **권장 대상** | UI 전용 PP 분리 | 3D 오브젝트 PP 분리 | UI + 레이어별 독립 PP |

---

## 📑 시도한 방법 목록

| # | 방법 | 한 줄 요약 | 상태 |
|---|------|-----------|------|
| 1 | 카메라 오버레이 (Camera Stacking) | 두 카메라의 컬링 마스크를 나눠 PP 렌더링을 레이어별로 분리하는 방법으로, 3D 그림자가 불필요한 UI 오브젝트의 Bloom을 완전히 제외할 때 적합하다. | ⚠️ Bloom 완전 배제 / ⚠️ 3D 그림자 소실 |
| 2 | Renderer Feature — Render Objects | URP 기본 제공 Render Objects Feature를 이용해 PP 이후 특정 레이어 오브젝트를 다시 그려 덮어쓰는 방법으로, Color Adjustments 제외가 필요한 3D 오브젝트에 그림자를 유지하면서 적용할 때 적합하다. | ✅ 그림자 정상 / ⚠️ Bloom halo 배제 불가 |
| 3 | Renderer Feature — Push Pop Layer | 커스텀 Push Pop Feature를 이용해 Overlay Camera 렌더 결과를 별도 캔버스에서 독립적으로 PP 처리 후 합성하는 방법으로, 레이어별로 완전히 다른 PP 효과가 필요할 때 적합하다. | ✅ Bloom 완전 배제 / ✅ 독립 PP 가능 / ⚠️ 3D 그림자 소실 |

---

---

## 🔷 방법 1 : 카메라 오버레이 (Camera Stacking)

### 📺 참고 영상

| 항목 | 링크 |
|------|------|
| How to Avoid Post-Processing in URP | [바로가기](https://www.youtube.com/watch?v=MstjGkiVlQc) |

---

### 💡 방법 설명

URP의 카메라 스태킹(Camera Stacking) 기능을 활용한다.
**Base Camera** 하나와 **Overlay Camera** 하나를 조합해,
각각 서로 다른 레이어를 렌더링하도록 분리하는 방식이다.

---

### 🔄 렌더링 흐름

```
[레이어 분류]
  Default / UI     ──────────→  Main Camera (Base)   │ PP 적용
  NoPostProcessing ──────────→  Overlay Camera        │ PP 없음

[Pass 1 : Main Camera]
  (1) Shadow Caster Pass
        Default / UI     ✅ 포함  →  그림자 생성
        NoPostProcessing ❌ 제외  →  ⚠️ 그림자 없음!
  (2) Opaque / Transparent Render
        Default / UI 렌더링 (NoPostProcessing 건너뜀)
  (3) Post Processing
        Bloom, Color 전체 적용
        → Buffer A 완성 (Default/UI + PP)

[Pass 2 : Overlay Camera]
  (4) Opaque / Transparent Render
        NoPostProcessing 레이어만 렌더링 (PP 없음)
        Buffer A 위에 덧그림 (깊이 버퍼 공유)

[최종 출력]
  Buffer A (Default/UI + PP) + Overlay (NoPostProcessing, PP 없음)
```

> 핵심 : NoPostProcessing 레이어가 Shadow Caster Pass에서 제외되어 그림자가 사라진다.
> Overlay Camera는 독립 그림자 패스가 없어 Base Camera의 그림자 맵을 그대로 재사용한다.

---

### 🔧 적용 단계

**(1) 새 레이어 생성**
```
Project Settings → Tags and Layers → "NoPostProcessing" 추가
```

**(2) 오브젝트 레이어 할당**
PP를 제외할 오브젝트와 UI를 `NoPostProcessing` 레이어로 변경

**(3) Main Camera (Base) 설정**

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Base |
| 포스트 프로세싱 | ✅ 활성화 |
| 컬링 마스크 | `NoPostProcessing` 레이어 **제외** |
| Camera Stack | NoPost Camera 추가 |

**(4) NoPost Camera (Overlay) 생성** — Main Camera의 자식 오브젝트로 배치

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Overlay |
| 포스트 프로세싱 | ❌ 비활성화 |
| 컬링 마스크 | `NoPostProcessing` 레이어 **전용** |
| 뎁스 지우기 | ❌ 비활성화 |

---

### 🎯 적용 결과

<img width="700" alt="방법 1 게임 뷰" src="1.%20Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EA%B2%8C%EC%9E%84%EB%B7%B0%29.png">

- ✅ **UI** : Bloom 완전 제외. 포스트프로세싱 적용 / 미적용을 레이어로 분리 가능
- ⚠️ **3D 오브젝트** : PP 제외는 성공했지만 **그림자가 사라지는 문제** 발생

---

### ⚠️ 문제 발생 : 그림자 소실

> **이 문제는 URP 버전에 따라 발생 여부가 다르다.**
>
> | URP 버전 | Unity 버전 | 그림자 소실 여부 |
> |---------|-----------|----------------|
> | URP 12 이하 | Unity 2021.x 이전 | ❌ 소실 없음 (최적화 전 동작) |
> | URP 14 이상 | Unity 2022.x 이상 | ✅ 소실 발생 |
> | URP 17 (현재 프로젝트) | Unity 6.x | ✅ 소실 발생 |
>
> 구버전의 그림자 동작은 의도된 설계가 아닌 부산물이었다.
> Unity 2022 이후 Shadow Caster Pass 컬링을 Base Camera Culling Mask만 따르도록 최적화하면서 이 동작이 바뀌었다.

| 게임 뷰 (그림자 없음) | 에디터 씬 뷰 (그림자 있음) |
|---|---|
| <img width="380" alt="게임 뷰" src="1.%20Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EA%B2%8C%EC%9E%84%EB%B7%B0%29.png"> | <img width="380" alt="에디터 씬 뷰" src="1.%20Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EC%97%90%EB%94%94%ED%84%B0%EB%B7%B0%29.png"> |

---

### 🔧 임시 해결 : Shadow-Only 프록시

> 📌 **URP 14 이상 (Unity 2022.x ~)에서 그림자 소실 시에만 필요하다.**

NoPostProcessing 레이어 오브젝트와 동일한 위치에,
**화면에는 보이지 않고 그림자만 드리우는 복사본(프록시)** 을 Default 레이어에 자동 생성한다.

```
[Default 레이어]                         [NoPostProcessing 레이어]
  프록시 오브젝트                           원본 오브젝트 (화면에 보임)
  CastShadows = ShadowsOnly              (NoPost Camera가 PP 없이 렌더링)
  (화면에 렌더링 안 됨)
       ↓
  Main Camera Shadow Pass 포함 → 그림자 정상 생성
```

`ShadowProxy.cs`를 NoPostProcessing 레이어 오브젝트에 추가하면 런타임에 자동으로 동작한다.

| 항목 | 내용 |
|------|------|
| 프록시 레이어 | Default (0) |
| CastShadows | ShadowsOnly |
| 위치 동기화 | LateUpdate()에서 매 프레임 자동 동기화 |

<img width="700" alt="런타임 프록시 생성 확인" src="1.%20Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EB%9F%B0%ED%83%80%EC%9E%84%20%EC%8B%9C%2C%20%ED%94%84%EB%A1%9D%EC%8B%9C%20%EC%83%9D%EC%84%B1%29.png">

> 게임 실행 시 스크립트가 자동으로 `Sphere_ShadowProxy` 오브젝트를 생성해 그림자만 활성화한 모습

---

### 📊 구조적 한계와 결론

**카메라 스태킹의 구조적 문제**
- Overlay Camera는 독립된 렌더 패스를 가지지 않음
- Shadow Map, Depth Buffer, G-Buffer 등을 Base Camera와 공유
- 레이어로 렌더링을 분리할수록 공유 버퍼들이 뒤섞일 가능성이 있음

**Shadow-Only 프록시의 한계**
- 오브젝트마다 컴포넌트를 추가해야 함 (오브젝트 수 증가 시 관리 어려움)
- 동일 메시를 GPU에 두 번 제출 (Draw Call 증가)
- 복잡한 애니메이션이나 변형에 적용이 어려움

---

---

## 🔷 방법 2 : Renderer Feature — Render Objects

### 💡 방법 설명

카메라를 추가하는 대신, **기존 URP 렌더 파이프라인에 단계 하나를 끼워 넣는** 방식이다.
Main Camera 하나만 사용하므로 그림자 패스 분리 문제가 발생하지 않는다.

---

### 🔄 렌더링 흐름

```
[레이어 분류]
  모든 레이어 ──────────────→  Main Camera (단일)   │ 카메라 하나만 사용

[Pass 1 : Main Camera]
  (1) Shadow Caster Pass
        모든 레이어 ✅ 포함  →  ✅ 그림자 정상
  (2) Opaque / Transparent Render
        모든 레이어 렌더링 (NoPostProcessing 포함)
  (3) Post Processing
        Bloom, Color 전체 적용 (NoPostProcessing도 포함)
        → Buffer A 완성 (전체 오브젝트 + PP)

[Pass 2 : Render Objects Feature (AfterRenderingPostProcessing)]
  (4) NoPostProcessing 레이어 재렌더링
        원본 색상으로 해당 픽셀 덮어씌움
        ⚠️ 단, Bloom이 번진 배경 픽셀은 그대로 남음

[최종 출력]
  Buffer A (전체 + PP) 중 NoPostProcessing 오브젝트 픽셀만 원본으로 교체
  Bloom halo는 제거 불가 (이미 배경 픽셀에 번진 상태)
```

> 핵심 : 오브젝트 자체 픽셀은 교체되지만, Bloom처럼 주변으로 번진 효과는 되돌릴 수 없다.
> Color Adjustments처럼 픽셀 단위로 독립 작동하는 효과는 완전히 제거된다.

---

### 📌 포스트프로세싱 효과 종류와 동작 차이

이 방법의 특성을 이해하려면 PP 효과의 두 종류를 알아야 한다.

| 종류 | 예시 | 이 방법으로 배제 가능? |
|------|------|:---:|
| **국소적 효과** — 각 픽셀을 독립적으로 변환 | Color Adjustments, Tonemapping, Contrast | ✅ 가능 |
| **공간적 효과** — 밝은 픽셀이 주변으로 번짐 | Bloom, Depth of Field, Motion Blur | ❌ halo 잔존 |

- **Color Adjustments 배제 가능 이유** : 픽셀 자체를 교체하는 방식이라, Renderer Feature가 오브젝트 픽셀을 원본으로 덮어쓰면 효과가 사라짐
- **Bloom halo 배제 불가 이유** : Bloom은 밝은 픽셀에서 주변 배경 픽셀로 번지는 효과. 오브젝트 픽셀은 덮어씌울 수 있지만, 이미 배경 픽셀에 번진 halo는 건드릴 수 없음

---

### 🔧 적용 단계

**(1) 새 레이어 생성**
```
Project Settings → Tags and Layers → "NoPostProcessing" 추가
```

**(2) 오브젝트 레이어 할당**
PP를 제외할 오브젝트와 UI를 `NoPostProcessing` 레이어로 변경

**(3) Main Camera 설정**

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Base |
| 포스트 프로세싱 | ✅ 활성화 |
| 컬링 마스크 | **Everything** (모든 레이어 포함) |
| Camera Stack | 비워둠 |

**(4) Renderer Feature 추가**

본 프로젝트 렌더러 : `Assets/Settings/3D/3D Universal Render Pipeline Asset_Renderer.asset`
```
Inspector → Add Renderer Feature → Render Objects
```

World Space Canvas는 Transparent 큐, 3D 오브젝트는 Opaque 큐에서 렌더링되므로
**Feature를 2개** 만들어야 한다.

**(5) Feature 1 설정 — 불투명 오브젝트용 (3D Mesh)**

| 항목 | 설정값 | 역할 |
|------|--------|------|
| Name | CustomRenderObjectsOpaque | 식별 이름. 스크립트에서 이 이름으로 런타임 활성화/비활성화 가능 |
| Event | AfterRenderingPostProcessing | PP 완료 후 실행. 재렌더링 픽셀이 PP 없이 화면에 출력됨 |
| Queue | **불투명 (Opaque)** | 일반 3D 오브젝트 (Lit/Unlit 셰이더) |
| Layer Mask | **NoPostProcessing** | 해당 레이어만 재렌더링 |
| Override Mode | **없음 (None)** | 원본 머티리얼 그대로 사용 |

**(6) Feature 2 설정 — 투명/UI용 (World Space Canvas)**

| 항목 | 설정값 | 역할 |
|------|--------|------|
| Name | CustomRenderObjectsTransparent | 식별 이름 |
| Event | AfterRenderingPostProcessing | Feature 1과 동일 |
| Queue | **투명 (Transparent)** | World Space Canvas의 Image는 UI 셰이더로 Transparent 큐에 속함. 이 Feature 없이는 Canvas가 재렌더링되지 않음 |
| Layer Mask | **NoPostProcessing** | Feature 1과 동일 |
| Override Mode | **없음 (None)** | Feature 1과 동일 |

**(7) Renderer Data — 임시 텍스처 설정**

| 항목 | 설정값 | 역할 |
|------|--------|------|
| 임시 텍스처 (Intermediate Texture) | **항상 (Always)** | AfterRenderingPostProcessing Feature 출력이 최종 화면에 올바르게 반영되기 위해 필요 |

---

### 🎯 적용 결과

#### PP 없음 (기준)

<img width="700" alt="PP 없음 기준" src="2.%20Render%20Objects/%EC%95%84%EB%AC%B4%EA%B2%83%EB%8F%84%20%EC%A0%81%EC%9A%A9%20X.png">

#### Bloom만 활성화

<img width="700" alt="Bloom만 활성화" src="2.%20Render%20Objects/%EB%B8%94%EB%A3%B8.png">

- 위 : Bloom 적용 (빛 번짐 발생)
- 아래 : 오브젝트 자체 픽셀은 원본 유지. **주변 배경에 Bloom halo 잔존** ← 배경 픽셀이라 제거 불가

#### Color Adjustments만 활성화

<img width="700" alt="Color Adjustments만 활성화" src="2.%20Render%20Objects/%EC%BB%AC%EB%9F%AC.png">

- 위 : 초록 필터 적용
- 아래 : 원본 흰색 유지 ✅ — 픽셀을 개별 교체하는 방식이라 Renderer Feature가 덮어쓰면 효과 제거됨
- 주변 배경 : 초록색 유지 — 배경 픽셀은 Feature가 건드리지 않음

#### Bloom + Color Adjustments 동시 활성화

<img width="700" alt="Bloom + Color Adjustments 동시" src="2.%20Render%20Objects/%EB%B8%94%EB%A6%84%2B%EC%BB%AC%EB%9F%AC.png">

- 위 : 초록 필터 + Bloom 모두 적용
- 아래 : 색은 원본 흰색 ✅, Bloom halo는 주변 배경에 잔존 ⚠️
- 그림자 : 두 스피어 모두 정상 ✅

---

### 🔍 렌더 그래프 뷰어 — Renderer Feature 패스 확인

<img width="700" alt="Render Graph Viewer" src="2.%20Render%20Objects/%EB%A0%8C%EB%8D%94%20%EA%B7%B8%EB%9E%98%ED%94%84%20%EB%B7%B0%EC%96%B4%28%EC%BB%A4%EC%8A%A4%ED%85%80%20%EB%A0%8C%EB%8D%94%20%EC%98%A4%EB%B8%8C%EC%A0%9D%ED%8A%B8%20%ED%8C%A8%EC%8A%A4%EA%B0%80%20%EC%B6%94%EA%B0%80%EB%90%9C%20%EC%82%AC%EC%A7%84%29.png">

> 렌더 파이프라인에 CustomRenderObjects 패스가 AfterRenderingPostProcessing 시점에 추가된 모습

---

### 📊 구조적 한계와 결론

- ✅ 그림자 정상 (모든 URP 버전)
- ✅ 국소적 PP 효과 배제 (Color Adjustments, Tonemapping 등)
- ❌ Bloom halo 배제 불가 — 공간적 효과의 주변 번짐은 구조적으로 제거 불가
- 📌 비발광/어두운 3D 오브젝트에는 Bloom 기여도가 낮아 실용적으로 문제없는 경우가 많음

---

---

## 🔷 방법 3 : Renderer Feature — Push Pop Layer

### 📺 참고 영상 및 스크립트

| 항목 | 링크 |
|------|------|
| Post Processing with Camera Stacking — URP | [바로가기](https://www.youtube.com/watch?v=7_Vy0jDqjvM) |
| 원본 스크립트 제공 (Aljosha) | Unity Discussions 포럼 |

필요 파일 : `PushPopLayerRenderFeature.cs`, `BlendLayer.shader`, `PushPop Mat.mat`

---

### 💡 방법 설명

커스텀 스크립트 기반의 Renderer Feature를 사용하는 방식이다.
Overlay Camera에 전용 렌더러를 부여하고, Push Pop 기능으로 Overlay의 렌더 결과만
**별도의 투명 캔버스**에 그린 뒤 포스트프로세싱을 적용한다.
최종적으로 Base Camera의 결과물과 Blend Shader로 합성하여 출력한다.

방법 2와의 결정적 차이 : Overlay Camera의 PP가 **독립된 캔버스에서만 실행**되기 때문에,
Bloom도 Overlay 오브젝트 내에서만 계산되고 배경 픽셀로 번지지 않는다.

---

### 🔄 렌더링 흐름

```
[레이어 분류]
  Default / UI ──────────────→  Main Camera (Base)          │ 씬 렌더링
  Overlay       ──────────────→  Overlay Camera (별도 렌더러) │ 독립 PP

[Pass 1 : Main Camera]
  (1) Shadow Caster Pass
        Default / UI ✅ 포함  →  그림자 생성
        Overlay       ❌ 제외  →  ⚠️ 그림자 없음!
  (2) Opaque / Transparent Render
        Default / UI 렌더링 (Overlay 건너뜀)
  (3) Post Processing (선택적)
        별도 Volume Profile 또는 없음
        → Snapshot (Buffer A) 완성

[Pass 2 : Overlay Camera (Push Pop Feature)]

  -- [PUSH] 렌더링 시작 전 --
  (4) Buffer A를 Snapshot 텍스처로 저장
  (5) 빈 검정 캔버스 Buffer B 생성
        이후 모든 렌더링은 Buffer B에 기록됨

  (6) Opaque / Transparent Render
        Overlay 레이어 → Buffer B에 렌더링
  (7) Post Processing
        Bloom, Color → Buffer B에만 독립 적용 (배경과 완전히 격리!)

  -- [POP] PP 완료 후 --
  (8) Blend 합성
        Buffer B (Overlay + PP) + Buffer A (Snapshot)
        Additive 블렌딩 : Buffer B의 빈 영역(검정)은 Buffer A가 그대로 보임

[최종 출력]
  Buffer A (Main Camera 결과) + Buffer B (Overlay + 독립 PP)
  Bloom halo가 Overlay 오브젝트 주변에만 발생 ✅ (배경 픽셀 오염 없음)
```

> 핵심 : Push로 배경을 저장하고 새 캔버스를 만든 뒤, 그 위에서만 PP가 동작한다.
> Pop에서 새 캔버스를 저장된 배경에 합성하므로 배경과 Overlay의 PP가 완전히 격리된다.

---

### 🔧 적용 단계

**(1) 새 레이어 생성**
```
Project Settings → Tags and Layers → "Overlay" 추가
```

> 💡 레이어 이름 비교 :
> - **Default** 레이어 : Main Camera가 렌더링하는 일반 오브젝트
> - **Overlay** 레이어 : Overlay Camera가 독립된 PP로 렌더링할 오브젝트

**(2) 오브젝트 레이어 할당**
독립된 PP를 적용할 오브젝트와 UI를 `Overlay` 레이어로 변경

**(3) Overlay 전용 Renderer Data 생성**
```
Project 창 우클릭 → Create → Rendering → URP Universal Renderer Data
이름 : "Overlay Universal Render Pipeline Asset_Renderer"
```

**(4) 해당 Renderer를 URP Asset의 Renderer 목록에 추가**
```
Assets/Settings/3D/3D Universal Render Pipeline Asset.asset 선택
Inspector → Renderer List → "+" → 새 Renderer Data 추가
```
이 단계를 거쳐야 Overlay Camera에서 이 Renderer를 선택할 수 있다.

**(5) 새 Renderer에 Push Pop Layer Render Feature 추가**
```
Overlay Renderer Data 선택
Inspector → Add Renderer Feature → Push Pop Layer Render Feature
```

| 항목 | 설정값 |
|------|--------|
| Push | Before Rendering |
| Pop | After Rendering Post Processing |
| Blend Material | PushPop Mat |

**(6) 임시 텍스처 설정**

Overlay Renderer Data 인스펙터 → Compatibility → **임시 텍스처 : 항상**

**(7) Overlay Camera 생성 및 설정**

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Overlay |
| 렌더러 | Overlay Renderer ((4)에서 추가한 것) |
| 포스트 프로세싱 | ✅ 활성화 |
| 컬링 마스크 | Overlay |

**(8) Main Camera 설정**

| 항목 | 설정값 |
|------|--------|
| 포스트 프로세싱 | ❌ 비활성화 (또는 별도 Volume Profile 사용) |
| 컬링 마스크 | Overlay 레이어 **제외** |
| Camera Stack | Overlay Camera 추가 ← **누락 시 Overlay 출력이 화면에 합성되지 않음** |

**(9) PushPop Mat 블렌드 설정**

| 항목 | 설정값 | 설명 |
|------|--------|------|
| Source Blend Factor | SrcAlpha | Overlay 픽셀의 알파값만큼 기여 |
| Destination Blend Factor | **One** | 스냅샷을 100% 유지 (Additive 블렌딩) |
| Blend Operation | Add | Overlay를 스냅샷 위에 더함 |

> ⚠️ Destination을 `One`으로 설정해야 한다. `OneMinusSrcAlpha`(기본값)로 두면 배경이 검정으로 덮어씌워진다.

**(10) URP Asset — Alpha Processing 활성화**
```
URP Asset (Assets/Settings/3D/3D Universal Render Pipeline Asset.asset)
Post Processing 섹션 → Alpha Processing : ✅ 체크
```

---

### 🎯 적용 결과

#### Bloom 활성화

<img width="700" alt="Bloom 활성화" src="3.%20Push%20Pop%20Layer/%EB%B3%BC%EB%A5%A8.png">

- **Default 레이어** : Main Camera Volume의 PP 적용 (파란 틴트)
- **Overlay 레이어** : 독립된 Global Volume의 PP 적용 (초록 틴트 + Bloom)
- 그림자 : ⚠️ Overlay 레이어 그림자 소실 (Camera Stacking 구조 동일한 원인)

#### Bloom + Color Adjustments 동시 활성화

<img width="700" alt="Bloom + Color Adjustments 동시" src="3.%20Push%20Pop%20Layer/%EB%B3%BC%EB%A5%A8%20%2B%20%EC%BB%AC%EB%9F%AC.png">

- ✅ **Overlay 오브젝트에만** 초록 필터와 Bloom이 적용됨
- ✅ 배경과 Default 레이어에는 영향 없음 — Push Pop 스냅샷으로 격리됐기 때문
- ✅ Bloom halo도 Overlay 오브젝트 주변에만 발생 (배경 픽셀 오염 없음)

---

### 🔍 렌더 그래프 뷰어 — Push Pop 패스 확인

<img width="700" alt="Render Graph Viewer - Push Pop" src="3.%20Push%20Pop%20Layer/%EB%A0%8C%EB%8D%94%20%EA%B7%B8%EB%9E%98%ED%94%84%20%EB%B7%B0%EC%96%B4%28Push%20Pop%20%EB%8B%A8%EA%B3%84%EA%B0%80%20%EC%B6%94%EA%B0%80%EB%90%9C%20%EC%82%AC%EC%A7%84%29.png">

> 렌더 파이프라인에 Push(스냅샷 저장)와 Pop(합성) 단계가 추가된 모습

---

### 📊 구조적 한계와 결론

**작동하는 것**
- ✅ 레이어별 완전히 독립된 PP 적용 (각 카메라에 별도 Volume Profile 사용 가능)
- ✅ Bloom halo 완전 격리 — Overlay PP가 별도 캔버스에서 동작
- ✅ 국소적 PP 효과(Color Adjustments 등)도 완전 제어

**한계**
- ⚠️ 3D 오브젝트 그림자 소실 — Camera Stacking 구조로 인해 방법 1과 동일한 원인
  - 해결 : ShadowProxy.cs 재활용 가능 (1. Camera Stacking 폴더의 스크립트)
- ⚠️ 설정 복잡도가 방법 1/2보다 높음 (커스텀 스크립트, 별도 Renderer 필요)
- ⚠️ Alpha Processing 활성화 및 HDR 설정 주의 필요

---

---

## 📊 방법 상세 분석 및 실무 가이드

### 방법 1 vs 방법 3 : 무엇이 다른가

두 방법은 모두 Camera Stacking(Base + Overlay) 구조를 사용한다.
결정적인 차이는 **Overlay Camera가 어디에 렌더링하느냐**이다.

---

#### 핵심 차이 — 그림으로 비유

**방법 1 (Camera Stacking)**

```
1. 배경 그림을 완성하고 Bloom 필터를 적용한다        → [완성된 그림]
2. 완성된 그림 위에 Overlay 오브젝트를 그냥 얹는다   → [그림 위에 스티커 붙이기]
```

Overlay 오브젝트는 이미 완성된 그림 위에 붙여진다.
PP 효과는 이미 끝났으므로, Overlay 오브젝트에는 Bloom이 아예 없다.

---

**방법 3 (Push Pop Layer)**

```
1. 배경 그림을 완성하고 → 사진을 찍어 보관한다       → [Snapshot 저장]
2. 새로운 빈 도화지를 꺼낸다                       → [별도 Canvas 생성]
3. 도화지에 Overlay 오브젝트를 그린다
4. 그 도화지에만 Bloom 필터를 적용한다             → [독립 PP 처리]
5. 필터 입힌 도화지를 사진 위에 합성한다            → [Snapshot + Canvas 합성]
```

Overlay 오브젝트의 Bloom이 **도화지 안에서만** 계산된다.
배경으로 번지지 않고, Overlay도 자신만의 Bloom 효과를 가질 수 있다.

---

**결론**

| | 방법 1 | 방법 3 |
|--|--------|--------|
| Overlay 처리 위치 | 완성된 그림 위에 얹음 | 별도 도화지에서 독립 처리 |
| Overlay의 Bloom | ❌ Bloom 없음 | ✅ 자체 Bloom 가능 (배경 오염 없음) |
| 사용 목적 | PP를 **제거**하고 싶다 | 다른 독립적인 PP를 **적용**하고 싶다 |

---

#### 항목별 비교

| 비교 항목 | 방법 1 : Camera Stacking | 방법 3 : Push Pop Layer |
|----------|--------------------------|------------------------|
| **목적** | 특정 레이어의 PP를 **제거** | 레이어별로 **다른 PP**를 독립 적용 |
| **Volume Profile** | 두 카메라가 같은 Volume 공유 | 카메라별 별도 Volume 사용 가능 |
| **Overlay 렌더 위치** | Base Camera 버퍼 위에 직접 덧그림 | 별도 캔버스(Buffer B)에 격리 처리 |
| **Bloom 처리** | NoPP 오브젝트가 Bloom 연산 자체에서 제외 | Overlay 오브젝트가 독립 캔버스 안에서 자체 Bloom |
| **Bloom halo** | ✅ 완전 없음 | ✅ Overlay 내부에만 (배경 오염 없음) |
| **3D 그림자** | ⚠️ ShadowProxy 필요 | ⚠️ ShadowProxy 필요 |
| **구현 복잡도** | 낮음 | 중간 (커스텀 스크립트 필요) |

---

#### 렌더링 흐름 좌우 비교

```
방법 1 : Camera Stacking             방법 3 : Push Pop Layer
────────────────────────────────     ────────────────────────────────
레이어 분류                           레이어 분류
  Default → Main Camera (Base)          Default → Main Camera (Base)
  NoPP    → Overlay Camera              Overlay  → Overlay Camera (별도 렌더러)

Main Camera                           Main Camera
  Shadow : Default ✅  NoPP ❌        Shadow : Default ✅  Overlay ❌
  Render : Default 렌더링               Render : Default 렌더링
  PP     : Volume 적용                  PP     : 별도 Volume 또는 없음
             ↓                                     ↓
          [Buffer A]                           [Snapshot A 저장]

Overlay Camera                        Overlay Camera (Push Pop Feature)
  Render : NoPP 레이어                  [PUSH] Snapshot A 저장
           PP 없음                             새 Canvas B 생성
           Buffer A 위에 바로 덧그림
                                        Render : Overlay → Canvas B에 그림
  ← PP가 이미 적용된 Buffer A 위에         PP     : 독립 Bloom/Color
    PP 없는 오브젝트를 얹는 방식 →               Canvas B에만 적용! (완전 격리)

                                        [POP] Canvas B + Snapshot A 합성
                                               Additive 블렌딩

최종 화면                              최종 화면
  Buffer A (Default + PP)               Snapshot A (Default)
  + NoPP 오브젝트 (PP 없음)             + Canvas B (Overlay + 독립 PP)
                                         Bloom이 Overlay 밖으로 안 번짐 ✅
```

---

#### 어떤 방법을 써야 하나

```
"씬은 PP 유지하고 UI/특정 오브젝트에서 PP를 끄고 싶다"
  → 방법 1 : 설정 빠름, Bloom 완전 제거

"씬과 UI에 서로 다른 독립적인 PP 효과를 주고 싶다"
  → 방법 3 : 레이어별 독립 Volume 가능, Bloom도 각자 독립

"3D 오브젝트에서만 Color Adjustments를 끄고 그림자는 유지하고 싶다"
  → 방법 2 : 단일 카메라로 그림자 정상, Bloom halo는 감수
```

---

**실무 채택 기준 요약**

| 상황 | 권장 방법 |
|------|----------|
| UI에 Bloom을 끄거나 켜고 싶다 | **방법 1** (설정 빠르고 직관적) |
| UI에 씬과 다른 색보정/Bloom 효과를 주고 싶다 | **방법 3** (독립 PP) |
| 3D 오브젝트의 PP를 제외하되 그림자는 유지하고 싶다 | **방법 2** (Color Adjustments 등 국소적 효과에 적합) |
| Bloom까지 완전히 제외한 3D 오브젝트 + 그림자 유지 | **방법 1 + ShadowProxy** |