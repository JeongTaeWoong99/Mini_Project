# PixelArtPack-Vol.2

Unity-chan 공식에서 배포하는 2D 픽셀 아트 캐릭터/애니메이션 샘플 패키지다.
9종 캐릭터의 기본 동작·전투·감정 표현 애니메이션과, 이를 시연하는 데모 씬·스크립트로 구성되어 있다.

- **출처 :** Unity-chan 공식 다운로드 페이지 (<https://unity-chan.com/download/releaseNote.php?id=UnityChanPixelArtPackVol2>)
- **참고 영상 :** <https://www.youtube.com/watch?v=1y-b743ofOI>
- **용도 :** 2D 픽셀 캐릭터 애니메이션 파이프라인(스프라이트 / Aseprite) 학습 및 데모

> 본 README 는 패키지 구조 파악과 학습을 위해 작성되었으며, 내부 C# 스크립트는 프로젝트 코딩 스타일에 맞춰 정렬 및 한국어 주석으로 정리되어 있다. (로직/직렬화 필드명은 원본 그대로 보존)

---

## 1. 폴더 구조

| 폴더 | 역할 |
|------|------|
| `AnimationController/` | 베이스 AnimatorController 3종 + 캐릭터별 Override Controller |
| `Animations/` | PNG 스프라이트로 손수 제작한 `.anim` 클립 (캐릭터별, 153개) |
| `Aseprite/` | 애니메이션 원본 `.ase` 파일 (캐릭터별, 153개) |
| `Sprites/` | PNG 스프라이트 시트 이미지 (811개) |
| `Prefabs/` | 캐릭터 Prefab 및 지면(Ground) Prefab |
| `Scenes/` | 카테고리별 데모 씬 (AnimClips 판 / Aseprite 판) |
| `Scripts/` | 입력·물리·애니메이터 제어 C# 스크립트 |

### 캐릭터 목록 (9종)
Cindy · Holger · Jack · Mariabell · Marie · Misaki · Toko · Unitychan · Yuji · Yuko

---

## 2. Overrides vs OverridesAseprite (가장 헷갈리는 부분)

두 폴더 모두 **AnimatorOverrideController** 모음으로, 동일한 베이스 컨트롤러의 애니메이션 클립만 캐릭터별로 교체한다.

**차이는 "교체에 사용하는 클립이 어디서 왔는가"** 뿐이며, 최종 동작 결과는 동등하다.

| 구분 | `Overrides/` | `OverridesAseprite/` |
|------|-------------|----------------------|
| 파일 | `*.overrideController` | `*_Aseprite.overrideController` |
| 참조하는 클립 | 독립 `.anim` 파일 (`Animations/`) | `.ase` 임포트 시 **자동 생성된 sub-asset 클립** |
| 클립의 원본 | `Sprites/` 의 PNG 시트로 **수작업 제작** | `Aseprite/` 의 `.ase` 원본을 임포터가 **자동 변환** |

> 베이스 컨트롤러는 `AnimationController/` 루트의 `BaseStandardController` · `BaseBattleController` · `BaseEmotionController` 3종이다.
> Override 들은 이 베이스의 상태 머신(전이 조건 등)은 그대로 두고, 각 상태에 들어가는 클립만 갈아끼운다.

### Aseprite 와 Sprite, 무엇이 다른가?

같은 캐릭터의 같은 애니메이션이지만 **제작 파이프라인(워크플로)이 다르다.**

**🔹 Sprite 워크플로 (`Overrides/`, `Animations/`, `Sprites/`)**
1. `.png` 스프라이트 시트를 Unity 에 임포트해 스프라이트로 슬라이스
2. 사람이 직접 프레임을 배치해 `.anim` 클립을 제작
3. 그 `.anim` 클립을 Override Controller 가 참조

**🔹 Aseprite 워크플로 (`OverridesAseprite/`, `Aseprite/`)**
1. 픽셀 아트 툴 **Aseprite** 의 원본 `.ase` 파일을 그대로 프로젝트에 둠
2. Unity 의 **Aseprite Importer**(`com.unity.2d.aseprite` 패키지)가 `.ase` 를 읽어
   스프라이트 + 애니메이션 클립을 **자동 생성**(`.ase` 내부의 sub-asset)
3. 자동 생성된 클립을 Override Controller 가 참조

| 비교 | Sprite (PNG + .anim) | Aseprite (.ase) |
|------|----------------------|-----------------|
| 원본 파일 | PNG 시트 | `.ase` |
| 클립 생성 | 수작업 | 임포터가 자동 |
| 수정 편의성 | 그림 수정 후 클립 재작업 필요 | Aseprite 에서 저장만 하면 자동 리임포트 |
| 의존성 | 없음 | `com.unity.2d.aseprite` 패키지 필요 |

#### ⭐ 핵심 한 문장 정리

> - **Sprite :** PNG → 사람이 유니티 에디터에서 클립을 **직접 제작**
> - **Aseprite :** `.ase` 원본 → 유니티 임포터가 클립을 **자동 생성**

#### 흔한 오해 바로잡기

"Aseprite 쪽은 다른 사람이 클립을 미리 만들어 둔 것"이라고 오해하기 쉽지만, 정확히는 다음과 같다.

- Aseprite(프로그램)에서 만든 것은 클립이 아니라 **그림 + 프레임 구성 + 태그(애니메이션 구간 정보)** 가 담긴 `.ase` **원본 파일**이다.
- 그 `.ase` 를 유니티에 넣으면 **Aseprite Importer 가 파일 내용을 해석해 스프라이트와 클립을 자동 생성**한다.
  → 즉 클립을 만든 주체는 사람이 아니라 **유니티 임포터**다.
- 그래서 `Aseprite/` 폴더에는 `.anim` 파일이 따로 없다. 클립이 `.ase` **내부의 sub-asset** 으로 들어 있기 때문. (반면 Sprite 쪽은 `Animations/` 폴더에 `.anim` 이 실제 파일로 존재)
- 작업자가 Aseprite 에서 그림만 수정해 저장하면 유니티가 다시 읽어 클립까지 갱신한다. (수정이 편한 이유)

> 비유하면 — Sprite 는 "재료(PNG)를 사놓고 요리(클립)는 내가 직접", Aseprite 는 "밀키트(`.ase`)를 넣으면 기계(임포터)가 알아서 요리"하는 셈이다.

> **정리 :** "Aseprite" 쪽이 픽셀 아트 원본을 그대로 다루며 수정이 편한 최신 방식이고,
> "Sprite" 쪽은 PNG 기반의 전통적인 수작업 방식이다. 둘은 **같은 애니메이션의 두 가지 제작 경로**일 뿐이다.

---

## 3. 스크립트 구조 (`Scripts/`)

```
BaseSpriteController (추상 베이스)
  │  · Update() 에서 입력 리더 null 체크 후 UpdateInternalV() 호출
  │  · GetInputReader() 로 입력 리더 제공
  │
  ├─ StandardActionsSceneSpriteController  : 기본 동작 (이동/점프/웅크리기)
  ├─ BattleActionsSceneSpriteController    : 기본 동작 + 공격 1/2/3 트리거
  ├─ StageSceneSpriteController            : 실제 이동 속도 제어 + 트리거 피격
  └─ EmotionActionsSceneSpriteController   : 감정 표현(긍정/부정/워밍업) 재생

보조 클래스
  ├─ PixelArtPackInputReader   : 입력 추상화 (Input System ↔ 레거시 Input)
  ├─ PixelArtPackConstants     : Animator 파라미터 해시·공용 상수 캐싱
  └─ Physics2DUtility          : 발밑 지면 거리 레이캐스트 계산
```

| 스크립트 | 설명 |
|----------|------|
| `BaseSpriteController.cs` | 모든 컨트롤러의 공통 베이스. 입력 리더 보유 및 매 프레임 위임 |
| `PixelArtPackInputReader.cs` | `Move` / `Jump` / `Attack0~2` 입력을 읽음. `ENABLE_INPUT_SYSTEM` 분기로 신·구 입력 모두 지원 |
| `PixelArtPackConstants.cs` | `Speed`, `FallSpeed`, `GroundDistance`, `IsCrouch`, `Damage`, `Attack1~3`, 감정 상태 해시 등 |
| `Physics2DUtility.cs` | 특정 방향으로 레이캐스트해 레이어까지의 거리 반환 (착지 판별용) |
| `Scenes/*SceneSpriteController.cs` | 위 표의 씬별 동작 컨트롤러 4종 |

### 동작 흐름
입력(`InputReader`) → 컨트롤러가 읽어 가공 → **AnimatorController 파라미터**에 반영 → Override Controller 가 캐릭터별 클립으로 표현.
즉 컨트롤러는 캐릭터를 직접 그리지 않고 **애니메이터 파라미터만 갱신**하며, 실제 표현은 애니메이터/오버라이드가 담당한다.

---

## 4. 데모 씬 (`Scenes/`)

각 카테고리마다 **AnimClips 판**(Sprite 워크플로)과 **Aseprite 판**(Aseprite 워크플로)이 한 쌍씩 있다. 결과는 같고 제작 경로만 다르다.

| 카테고리 | 씬 | 내용 |
|----------|-----|------|
| Standard Actions | `Standard Actions (AnimClips/Aseprite)` | 이동·점프·웅크리기 기본 동작 |
| Battle Actions | `Battle Actions (AnimClips/Aseprite)` | 기본 동작 + 공격 콤보 |
| Emotion Actions | `Emotion Actions (AnimClips/Aseprite)` | 긍정/부정/워밍업 감정 표현 |
| Stage | `Stage (AnimClips/Aseprite)` | 실제 이동 + 장애물 트리거 피격 데모 |

---

## 5. 사용법

1. 원하는 카테고리의 데모 씬을 연다. (Sprite 방식 학습이면 `(AnimClips)`, Aseprite 방식이면 `(Aseprite)`)
2. 캐릭터 오브젝트에는 다음이 함께 붙는다.
   - 씬에 맞는 `*SceneSpriteController`
   - `PixelArtPackInputReader` (+ Input System 사용 시 `PlayerInput`)
   - `Animator` (베이스 Controller 지정) → 캐릭터별 **Override Controller** 로 클립 교체
3. 컨트롤러의 `groundMask` 에 지면 레이어를 지정한다. (착지 판별용)
4. 입력
   - 이동 : 방향키 / `Move`
   - 점프 : `Jump`
   - 공격·감정 : `Attack0~2` (레거시는 `Z` / `X` / `C`)

### 의존성
- `com.unity.2d.aseprite` 패키지 — Aseprite 워크플로(`.ase`, `OverridesAseprite/`, `(Aseprite)` 씬) 사용 시 필요
- 신형 Input System 사용 시 `ENABLE_INPUT_SYSTEM` 활성화 (미사용 시 레거시 Input 매니저로 동작)
