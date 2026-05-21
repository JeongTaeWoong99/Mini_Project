# PostProcessing Exception

**기간 :** 2026.05.21 ~

## 목차

- [개요](#개요)
- [시도한 방법 목록](#시도한-방법-목록)
- [방법 1 : 카메라 오버레이 (Camera Stacking)](#방법-1--카메라-오버레이-camera-stacking)
- [방법 2 : Renderer Feature (Render Objects)](#방법-2--renderer-feature-render-objects)
- [방법 비교](#방법-비교)

---

## 개요

게임에서 전체 씬에는 블룸(Bloom) 같은 포스트프로세싱 효과를 주면서,
**특정 UI나 오브젝트에는 그 효과를 제외**하고 싶은 경우가 있다.

URP 환경에서 이 문제를 해결하는 방법들을 직접 시도해보고, 각 방법의 동작 원리, 결과, 한계를 정리한 프로젝트다.

<img width="700" alt="포스트프로세싱 적용 / 미적용 비교" src="Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EA%B2%8C%EC%9E%84%EB%B7%B0%29.png">

> 위 : 포스트프로세싱(Bloom) 적용 / 아래 : 미적용

---

## 시도한 방법 목록

| # | 방법 | 요약 | 상태 |
|---|------|------|------|
| 1 | 카메라 오버레이 (Camera Stacking) | Base + Overlay 카메라로 PP 렌더링 분리 | ⚠️ Bloom 완전 배제 가능, 그림자 소실 문제 |
| 2 | Renderer Feature (Render Objects) | PP 이후 단계에서 특정 레이어 재렌더링 | ⚠️ 그림자 정상, 공간적 PP(Bloom halo) 배제 불가 |

---

## 방법 1 : 카메라 오버레이 (Camera Stacking)

### 참고 영상

| 항목 | 링크 |
|------|------|
| How to Avoid Post-Processing in URP | [바로가기](https://www.youtube.com/watch?v=MstjGkiVlQc) |

---

### 방법 설명

URP의 카메라 스태킹(Camera Stacking) 기능을 활용한다.
**Base Camera** 하나와 **Overlay Camera** 하나를 조합해,
각각 서로 다른 레이어를 렌더링하도록 분리하는 방식이다.

```
[Main Camera - Base]
  1. NoPostProcessing 레이어 제외하고 씬 렌더링
  2. 포스트프로세싱(Bloom 등) 적용

[NoPost Camera - Overlay]
  3. NoPostProcessing 레이어만 위에 덧그림
  4. 포스트프로세싱 미적용
```

---

### 적용 단계

**① 새 레이어 생성**

```
Project Settings → Tags and Layers → Layers
→ 빈 슬롯에 "NoPostProcessing" 추가
```

**② 오브젝트 레이어 할당**

포스트프로세싱을 제외할 오브젝트와 UI를 `NoPostProcessing` 레이어로 변경한다.

**③ Main Camera (Base) 설정**

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Base |
| 포스트 프로세싱 | ✅ 활성화 |
| 컬링 마스크 | `NoPostProcessing` 레이어 **제외** |
| Camera Stack | NoPost Camera 추가 |

**④ NoPost Camera (Overlay) 생성**

씬에 새 카메라를 추가하고 Main Camera의 **자식 오브젝트**로 배치한다.

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Overlay |
| 포스트 프로세싱 | ❌ 비활성화 |
| 컬링 마스크 | `NoPostProcessing` 레이어 **전용** |
| 뎁스 지우기 | ❌ 비활성화 |

---

### 적용 결과

<img width="700" alt="방법 1 게임 뷰" src="Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EA%B2%8C%EC%9E%84%EB%B7%B0%29.png">

- **UI** : 의도대로 동작. 포스트프로세싱 적용 / 미적용을 레이어로 분리 가능
- **3D 오브젝트** : 포스트프로세싱 제외는 성공했지만, **그림자가 사라지는 문제** 발생

---

### 문제 발생 : 그림자 소실

> ⚠️ **이 문제는 URP 버전에 따라 발생 여부가 다르다.**
>
> | URP 버전 | Unity 버전 | 그림자 소실 여부 |
> |---------|-----------|----------------|
> | URP 12 이하 | Unity 2021.x 이전 | ❌ 소실 없음 |
> | URP 14 이상 | Unity 2022.x 이상 | ✅ 소실 발생 |
> | URP 17 (현재 프로젝트) | Unity 6.x | ✅ 소실 발생 |
>
> 구버전에서 그림자가 나왔던 것은 의도된 설계가 아니라, Shadow Caster Pass 컬링이 덜 최적화된 상태의 부산물이었다. Unity 2022 이후 Base Camera 컬링 마스크만 따르도록 변경되면서 이 동작이 사라졌다.

NoPostProcessing 레이어의 3D 오브젝트 그림자가 게임 뷰에서 사라진다.
씬 뷰에서는 정상적으로 보이는데, 게임 뷰에서만 사라진다.

| 게임 뷰 (그림자 없음) | 에디터 씬 뷰 (그림자 있음) |
|---|---|
| <img width="400" alt="게임 뷰" src="Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EA%B2%8C%EC%9E%84%EB%B7%B0%29.png"> | <img width="400" alt="에디터 씬 뷰" src="Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EC%97%90%EB%94%94%ED%84%B0%EB%B7%B0%29.png"> |

---

### 원인 분석

URP에서 **그림자 맵(Shadow Map)은 Base Camera의 렌더 패스에서만 생성된다.**
Overlay Camera는 자체 그림자 패스가 없고, Base Camera의 그림자 맵을 재사용한다.

```
[Main Camera - Base]                     [NoPost Camera - Overlay]
Culling Mask : NoPostProcessing 제외      Culling Mask : NoPostProcessing 전용

  (1) Shadow Caster Pass  ← NoPostProcessing 오브젝트 미포함!
  (2) Opaque 렌더링
  (3) Post Processing 적용
                                           (4) NoPostProcessing 오브젝트 덧그림
                                               (그림자 맵 재사용)
```

Main Camera가 `NoPostProcessing` 레이어를 컬링에서 제외하기 때문에,
해당 오브젝트는 **그림자 맵 생성 단계(1)에서 존재하지 않는 것으로 처리된다.**
씬 뷰는 카메라 스태킹 없이 단일 패스로 모든 오브젝트를 처리하므로 그림자가 정상적으로 보인다.

---

### 임시 해결 : Shadow-Only 프록시

> 📌 **URP 14 이상 (Unity 2022.x ~)에서 그림자 소실이 발생하는 경우에만 필요하다.**

NoPostProcessing 레이어 오브젝트와 동일 위치에,
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

<img width="700" alt="런타임 시 프록시 자동 생성" src="Camera%20Stacking/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EB%9F%B0%ED%83%80%EC%9E%84%20%EC%8B%9C%2C%20%ED%94%84%EB%A1%9D%EC%8B%9C%20%EC%83%9D%EC%84%B1%29.png">

> 게임 실행 시 스크립트가 자동으로 `Sphere_ShadowProxy` 오브젝트를 생성해 그림자만 활성화한 모습

---

### 구조적 한계와 결론

**카메라 스태킹의 구조적 문제**

Overlay Camera는 독립된 렌더 패스를 가지지 않는다.
Shadow Map 외에도 Depth Buffer, G-Buffer 등 Base Camera의 렌더 결과 전체를 공유하며 동작하기 때문에,
레이어로 분리할수록 공유 버퍼들이 뒤섞이면서 의도치 않은 렌더링 결과가 나올 가능성이 있다.

**Shadow-Only 프록시의 한계**

- 오브젝트마다 컴포넌트를 추가해야 하므로 오브젝트 수가 많아질수록 관리가 어렵다
- 동일 메시를 GPU에 두 번 제출하는 구조 (Draw Call 증가)
- 복잡한 애니메이션이나 변형이 있는 오브젝트에는 적용이 어렵다
- URP 버전에 따라 동작이 달라지는 불안정한 기반 위에 있다

---

---

## 방법 2 : Renderer Feature (Render Objects)

### 방법 설명

카메라를 추가하는 대신, **기존 URP 렌더 파이프라인에 단계 하나를 끼워 넣는** 방식이다.
Main Camera 하나만 사용하며, 모든 오브젝트가 같은 카메라에 있기 때문에
그림자 패스 분리 문제가 발생하지 않는다.

```
[Main Camera — 단일 카메라, 모든 레이어 포함]
  (1) Shadow Caster Pass    ← 모든 오브젝트 포함 → 그림자 정상
  (2) Opaque / Transparent  ← 모든 오브젝트 렌더링
  (3) Post Processing 적용  (Bloom, Color Adjustments 등)

[Render Objects — Renderer Feature]
  (4) AfterRenderingPostProcessing 시점에 실행
      → NoPostProcessing 레이어 오브젝트를 다시 그림
      → 해당 픽셀을 PP 없는 원본 버전으로 덮어씌움
```

---

### 포스트프로세싱 효과 종류와 동작 차이

이 방법을 이해하려면 포스트프로세싱 효과가 두 가지 종류로 나뉜다는 것을 알아야 한다.

| 종류 | 예시 | 배제 가능 여부 |
|------|------|--------------|
| **국소적 효과** : 각 픽셀을 독립적으로 변환 | Color Adjustments, Contrast, Tonemapping | ✅ 가능 |
| **공간적 효과** : 밝은 픽셀이 주변 픽셀로 번짐 | Bloom, Depth of Field, Motion Blur | ❌ 불가 |

**국소적 효과 (Color Adjustments) 가 잘 배제되는 이유 :**

```
(3) Color Adjustments : 각 픽셀의 색을 독립적으로 변환 (초록 필터 → 픽셀이 초록색이 됨)
(4) Renderer Feature  : NoPostProcessing 오브젝트 픽셀을 원본 색으로 덮어씌움
    → 오브젝트 픽셀이 원본 흰색으로 교체됨 ✅
```

**공간적 효과 (Bloom) 가 완전히 배제되지 않는 이유 :**

```
(3) Bloom : 밝은 픽셀 감지 → 주변 배경 픽셀로 번짐 → 배경에 halo 고정됨
(4) Renderer Feature : NoPostProcessing 오브젝트 자체 픽셀만 덮어씌움
    → 오브젝트 픽셀은 원본으로 교체됨 ✅
    → 배경에 이미 번진 halo는 배경 픽셀이라 건드릴 수 없음 ❌
```

---

### 적용 단계

**① 새 레이어 생성**

```
Project Settings → Tags and Layers → Layers
→ 빈 슬롯에 "NoPostProcessing" 추가
```

**② 오브젝트 레이어 할당**

포스트프로세싱을 제외할 오브젝트와 UI를 `NoPostProcessing` 레이어로 변경한다.

**③ Main Camera 설정**

| 항목 | 설정값 |
|------|--------|
| 렌더 타입 | Base |
| 포스트 프로세싱 | ✅ 활성화 |
| 컬링 마스크 | **Everything** (모든 레이어 포함) |
| Camera Stack | 비워둠 (Overlay 카메라 불필요) |

**④ Renderer Feature 추가**

사용 중인 URP Renderer Data 에셋을 선택한다.
본 프로젝트의 경우 `Assets/Settings/3D/3D Universal Render Pipeline Asset_Renderer.asset`

```
Inspector → Add Renderer Feature → Render Objects
```

World Space Canvas(UI)는 Transparent 큐, 3D 오브젝트는 Opaque 큐에서 렌더링된다.
두 종류 모두 처리하려면 **Feature를 2개** 만들어야 한다.

**⑤ Feature 1 설정 — 불투명 오브젝트용 (3D Mesh)**

| 항목 | 설정값 | 역할 |
|------|--------|------|
| Name | CustomRenderObjectsOpaque | Inspector 식별 이름. 스크립트에서 이 이름으로 Feature를 찾아 런타임에 활성화/비활성화 가능 |
| Event | AfterRenderingPostProcessing | 이 Feature가 실행될 시점. PP가 모두 끝난 뒤 실행되어야 재렌더링된 픽셀이 화면에 반영됨 |
| Queue | **불투명 (Opaque)** | 처리할 셰이더 렌더 큐 범위. Lit/Unlit 셰이더의 일반 3D 오브젝트 |
| Layer Mask | **NoPostProcessing** | 이 Feature가 렌더링할 레이어 필터. 해당 레이어만 재렌더링 |
| Override Mode | **없음 (None)** | 원본 오브젝트의 머티리얼 그대로 사용. Material로 설정 시 지정 머티리얼로 덮어씀 |

**⑥ Feature 2 설정 — 투명/UI용 (World Space Canvas)**

| 항목 | 설정값 | 역할 |
|------|--------|------|
| Name | CustomRenderObjectsTransparent | Inspector 식별 이름 |
| Event | AfterRenderingPostProcessing | Feature 1과 동일 |
| Queue | **투명 (Transparent)** | World Space Canvas의 Image는 UI 셰이더를 사용하며 Transparent 큐에 속함. 이 Feature 없이 Opaque만으로는 Canvas가 재렌더링되지 않음 |
| Layer Mask | **NoPostProcessing** | Feature 1과 동일 |
| Override Mode | **없음 (None)** | Feature 1과 동일 |

**⑦ Renderer Data — 임시 텍스처 설정**

Renderer Data 인스펙터 상단에서 아래 항목을 변경한다.

| 항목 | 설정값 | 역할 |
|------|--------|------|
| 임시 텍스처 (Intermediate Texture) | **항상 (Always)** | AfterRenderingPostProcessing Feature의 출력이 최종 화면에 올바르게 반영되기 위해 필요. Auto 상태에서는 Feature 결과가 화면에 반영되지 않을 수 있음 |

---

### 적용 결과

#### Bloom만 활성화

<img width="700" alt="Bloom만 활성화" src="Renderer%20Feature/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EB%B8%94%EB%A3%B8%20on%29.png">

- 위 오브젝트 : Bloom 적용
- 아래 오브젝트 : 오브젝트 자체 픽셀은 원본 색 유지. **주변 배경에 Bloom halo 잔존** — Bloom이 배경 픽셀로 이미 번진 상태라 제거 불가

---

#### Color Adjustments만 활성화

<img width="700" alt="Color Adjustments만 활성화" src="Renderer%20Feature/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EC%BB%AC%EB%9F%AC%20on%29.png">

- 위 오브젝트 : 초록 필터 적용
- 아래 오브젝트 : 원본 흰색 유지 ✅ — 각 픽셀을 개별 교체하는 방식이라 Renderer Feature가 덮어쓰면 효과가 사라짐
- 주변 배경 : 초록색 유지 — 배경 픽셀은 이 Feature가 건드리지 않음

---

#### Bloom + Color Adjustments 동시 활성화

<img width="700" alt="Bloom + Color Adjustments 동시 활성화" src="Renderer%20Feature/%EA%B2%B0%EA%B3%BC%20%EC%82%AC%EC%A7%84%201%28%EB%B8%94%EB%A3%B8%20%EC%BB%AC%EB%9F%AC%20on%29.png">

- 위 오브젝트 : 초록 필터 + Bloom 모두 적용
- 아래 오브젝트 : 색은 원본 흰색 ✅, Bloom halo는 주변 배경에 잔존 ⚠️
- 그림자 : 두 스피어 모두 정상 ✅

---

### 구조적 한계와 결론

**작동하는 것**
- 그림자 정상 (모든 URP 버전)
- 국소적 PP 효과 배제 (Color Adjustments, Contrast, Saturation, Tonemapping 등)
- 단일 카메라 구조 — 추가 오브젝트, 추가 카메라 불필요

**작동하지 않는 것**
- 공간적 PP 효과의 halo 배제 불가 (Bloom, Depth of Field, Motion Blur 등)
- Bloom이 강하거나 오브젝트가 밝을수록 주변 halo가 눈에 띔

**용도 정리**
- Color Adjustments / Color Grading 제외가 주 목적인 경우 : 이 방법으로 충분
- Bloom halo까지 완전히 제거해야 하는 경우 : 방법 1 (Camera Stacking + ShadowProxy) 사용

---

---

## 방법 비교

| 항목 | 방법 1 : Camera Stacking | 방법 2 : Renderer Feature |
|------|--------------------------|---------------------------|
| **국소적 PP 배제** (Color Adjustments 등) | ✅ 완전 배제 | ✅ 완전 배제 |
| **공간적 PP 배제** (Bloom halo) | ✅ 완전 배제 | ❌ 오브젝트 픽셀만 배제, halo 잔존 |
| **3D 오브젝트 그림자** | ⚠️ URP 14+ 에서 소실 → ShadowProxy로 임시 해결 | ✅ 항상 정상 |
| **카메라 수** | 2개 (Base + Overlay) | 1개 |
| **추가 작업** | ShadowProxy 컴포넌트 (URP 14+) | 없음 |
| **URP 버전 의존성** | 있음 | 없음 |
| **권장 사용처** | 밝은 UI, 발광 오브젝트, Bloom 완전 배제 필요 시 | 일반 3D 오브젝트, Color Grading 제외, 그림자 우선 시 |