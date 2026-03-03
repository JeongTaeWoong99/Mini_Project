# 🚩 Dirty Flag Pattern (더티 플래그 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [왜 Dirty Flag Pattern이 필요한가?](#-왜-dirty-flag-pattern이-필요한가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [Additive Scene Loading과의 연관성](#-additive-scene-loading과의-연관성)
- [장단점](#-장단점)
- [실제 사용 사례](#-실제-사용-사례)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**Dirty Flag Pattern**은 **최적화 패턴(Optimization Pattern)** 중 하나로, **변경이 발생했을 때만 비용이 큰 연산을 수행**하도록 제어하는 패턴입니다.

"더티(Dirty)"는 "오염된" 즉, **"업데이트가 필요한 상태"** 를 의미합니다. 

플래그를 통해 실제로 상태 변화가 있을 때만 비싼 작업을 실행하고, 변화가 없으면 건너뜁니다.

### 📌 핵심 개념

```
매 프레임 비싼 연산을 실행하는 것이 아니라,
"변화가 생겼을 때(Dirty)" 만 연산을 수행한다!
```

**핵심 원리 :**

| 상태 | 설명 | 처리 |
|------|------|------|
| **Clean (클린)** | 변화 없음, 최신 상태 | 연산 건너뜀 |
| **Dirty (더티)** | 변화 발생, 업데이트 필요 | 연산 수행 후 Clean으로 초기화 |

**일반적인 방법 (비효율) :**
```csharp
// 매 프레임 거리 체크 + 씬 로드/언로드 반복 실행
void Update()
{
    foreach (Sector sector in m_Sectors)
    {
        // ❌ 조건과 무관하게 매 프레임 비싼 연산 실행
        bool isClose = sector.IsPlayerClose(m_Player.transform.position);
        if (isClose) sector.LoadContent();
        else         sector.UnloadContent();
    }
}
```

**Dirty Flag Pattern (효율적) :**
```csharp
void Update()
{
    foreach (Sector sector in m_Sectors)
    {
        bool isClose = sector.IsPlayerClose(m_Player.transform.position);

        // 상태가 바뀔 때만 더티로 표시
        if (isClose != sector.IsLoaded)
            sector.MarkDirty();

        // ✅ 더티한 경우에만 비싼 연산(씬 로드/언로드) 실행
        if (sector.IsDirty)
        {
            if (isClose) sector.LoadContent();
            else         sector.UnloadContent();

            sector.Clean(); // 처리 후 클린으로 초기화
        }
    }
}
```

---

## 🤔 왜 Dirty Flag Pattern이 필요한가?

### 문제 상황

씬 기반 레벨 스트리밍에서 플레이어 근접도에 따라 구역을 로드/언로드하는 시스템을 구현할 때, 이런 문제가 발생합니다 :

```
게임은 초당 60프레임 실행
    ↓
Update()가 초당 60번 호출
    ↓
씬 로드/언로드는 디스크 I/O + 메모리 할당을 수반하는 매우 비싼 연산
    ↓
플레이어가 움직이지 않아도 매 프레임 씬 로드/언로드 시도?
    ↓ ❌ 심각한 성능 문제 발생!
```

### Dirty Flag Pattern의 해결책

✅ **조건 체크와 실제 처리를 분리**
   - 거리 비교(가벼운 연산)는 매 프레임 실행
   - 씬 로드/언로드(비싼 연산)는 상태 변화 시에만 실행

✅ **불필요한 반복 연산 방지**
   - 플레이어가 이미 로드된 구역 안에 머물면 → 아무것도 하지 않음
   - 경계를 넘는 순간에만 → `MarkDirty()` → 씬 로드/언로드

✅ **예측 가능한 실행 타이밍**
   - 더티 플래그를 통해 언제 비싼 연산이 수행될지 명확히 제어 가능

---

## 🏗️ 핵심 구성요소

Dirty Flag Pattern은 다음 2가지 핵심 요소로 구성됩니다 :

### 1️⃣ Sector (더티 플래그 보유 객체)

**📁 파일 :** [Scripts/Sector.cs](./Scripts/Sector.cs)

```csharp
public class Sector : MonoBehaviour
{
    // 프로퍼티 - 더티 플래그와 로드 상태
    public bool IsLoaded { get; private set; } = false;
    public bool IsDirty  { get; private set; } = false;

    // 섹터를 업데이트가 필요한 상태(더티)로 표시
    public void MarkDirty()
    {
        IsDirty = true;
    }

    // 업데이트 후 더티 플래그 초기화
    public void Clean()
    {
        IsDirty = false;
    }

    // 플레이어가 충분히 가까운지 확인 (가벼운 연산)
    public bool IsPlayerClose(Vector3 playerPosition)
    {
        return Vector3.Distance(playerPosition, transform.position + m_CenterOffset) <= m_LoadRadius;
    }

    // 씬 로드 (비싼 연산)
    public void LoadContent()
    {
        IsLoaded = true;
        m_SceneLoader.LoadSceneAdditivelyByPath(m_ScenePath);
    }

    // 씬 언로드 (비싼 연산)
    public void UnloadContent()
    {
        IsLoaded = false;
        m_SceneLoader.UnloadSceneByPath(m_ScenePath);
    }
}
```

**역할 :**
- `IsDirty` 플래그로 업데이트 필요 여부를 추적
- `IsLoaded`로 현재 씬 로드 상태를 추적
- `MarkDirty()` / `Clean()`으로 플래그 상태 전환
- `LoadContent()` / `UnloadContent()`로 실제 씬 로드/언로드 수행

---

### 2️⃣ GameSectors (더티 플래그 관리자)

**📁 파일 :** [Scripts/GameSectors.cs](./Scripts/GameSectors.cs)

```csharp
public class GameSectors : MonoBehaviour
{
    public PlayerController m_Player;
    public Sector[]         m_Sectors;

    private void Update()
    {
        foreach (Sector sector in m_Sectors)
        {
            bool isPlayerClose = sector.IsPlayerClose(m_Player.transform.position);

            // 1단계 : 상태 변화 감지 → 더티 표시 (가벼운 연산)
            if (isPlayerClose != sector.IsLoaded)
                sector.MarkDirty();

            // 2단계 : 더티할 때만 실제 처리 (비싼 연산)
            if (sector.IsDirty)
            {
                if (isPlayerClose) sector.LoadContent();
                else               sector.UnloadContent();

                // 3단계 : 처리 완료 후 클린으로 초기화
                sector.Clean();
            }
        }
    }
}
```

**역할 :**
- 매 프레임 플레이어와 각 섹터의 거리를 체크
- 상태 변화를 감지하여 `MarkDirty()` 호출
- `IsDirty == true`인 섹터만 선별하여 씬 로드/언로드 수행
- 처리 후 `Clean()` 호출로 플래그 초기화

---

## 📊 코드 구조

### 폴더 구조

```
10_DirtyFlag/
├── Scripts/
│   ├── Sector.cs          ← 더티 플래그 보유 객체 : 씬 로드/언로드 + 플래그 관리
│   └── GameSectors.cs     ← 더티 플래그 관리자    : 변화 감지 + 선별 처리
│
├── SubScenes/             ← Additive로 로드될 16개의 서브 씬 배경
│   ├── Sector0-0.unity
│   ├── Sector0-1.unity
│   │   ... (총 16개)
│   └── Sector3-3.unity
│
├── Prefabs/
│   └── DirtyFlagAssets.prefab  ← 베이스 프리팹
│
├── DirtyFlag.unity             ← 베이스 씬 (플레이어, 카메라 등)
└── README.md                   ← 📍 현재 문서
```

### 클래스 다이어그램

```
┌──────────────────────────────────────┐
│            GameSectors               │  ← 더티 플래그 관리자
├──────────────────────────────────────┤
│  + m_Player  : PlayerController      │
│  + m_Sectors : Sector[]              │
├──────────────────────────────────────┤
│  - Update()                          │  ← 매 프레임 : 변화 감지 + 선별 처리
└──────────────────────────────────────┘
                  │ 참조 및 제어
                  ▼
┌──────────────────────────────────────┐
│              Sector                  │  ← 더티 플래그 보유 객체
├──────────────────────────────────────┤
│  + IsLoaded : bool                   │  ← 씬 로드 상태
│  + IsDirty  : bool                   │  ← 더티 플래그
├──────────────────────────────────────┤
│  + MarkDirty()                       │  ← 플래그 ON
│  + Clean()                           │  ← 플래그 OFF
│  + IsPlayerClose(playerPos) : bool   │  ← 거리 체크 (가벼운 연산)
│  + LoadContent()                     │  ← 씬 로드  (비싼 연산)
│  + UnloadContent()                   │  ← 씬 언로드 (비싼 연산)
└──────────────────────────────────────┘
                  │ 호출
                  ▼
┌──────────────────────────────────────┐
│           SceneLoader                │  ← 유틸리티 : Additive 씬 로드/언로드
├──────────────────────────────────────┤
│  + LoadSceneAdditivelyByPath(path)   │  ← LoadSceneAsync(Additive) 비동기
│  + UnloadSceneByPath(path)           │  ← UnloadSceneAsync 비동기
│  + UnloadSceneImmediately(path)      │  ← UnloadScene 동기 (OnDestroy용)
└──────────────────────────────────────┘
```

### 씬 구성 개요

```
[DirtyFlag.unity]  ← 베이스 씬 (항상 유지)
│  - 플레이어
│  - 카메라
│  - GameSectors
│  - 16개 Sector 오브젝트 (트리거 영역)
│
├── [Sector0-0.unity]  ← 플레이어가 가까울 때만 Additive 로드
├── [Sector1-2.unity]  ← 플레이어가 가까울 때만 Additive 로드
└── ...                ← 멀어지면 Unload
```

---

## 🔄 실행 흐름

### 1️⃣ 씬 로드 흐름

```
[플레이어가 Sector에 접근]
    ⬇️
GameSectors.Update() (매 프레임)
    ⬇️
sector.IsPlayerClose() → true
sector.IsLoaded        → false  (아직 로드 안 됨)
    ⬇️
isPlayerClose != sector.IsLoaded
    ⬇️
sector.MarkDirty()   → IsDirty = true
    ⬇️
sector.IsDirty == true
    ⬇️
sector.LoadContent()
  → IsLoaded = true
  → SceneLoader.LoadSceneAdditivelyByPath()
     → LoadSceneAsync(path, Additive) 비동기 실행
     → 베이스 씬 유지 + 서브 씬 추가 로드
    ⬇️
sector.Clean()       → IsDirty = false
    ⬇️
✅ 서브 씬 로드 완료 (다음 프레임부터 IsDirty == false → 연산 건너뜀)
```

### 2️⃣ 씬 언로드 흐름

```
[플레이어가 Sector에서 멀어짐]
    ⬇️
sector.IsPlayerClose() → false
sector.IsLoaded        → true   (이미 로드된 상태)
    ⬇️
isPlayerClose != sector.IsLoaded
    ⬇️
sector.MarkDirty()   → IsDirty = true
    ⬇️
sector.IsDirty == true
    ⬇️
sector.UnloadContent()
  → IsLoaded = false
  → SceneLoader.UnloadSceneByPath()
     → UnloadSceneAsync(scene) 비동기 실행
     → GameObject 제거 (에셋은 메모리 잔류)
    ⬇️
sector.Clean()       → IsDirty = false
    ⬇️
✅ 서브 씬 언로드 완료
```

### 3️⃣ 변화 없음 (플래그 체크만)

```
[플레이어가 이미 로드된 섹터 안에 머묾]
    ⬇️
sector.IsPlayerClose() → true
sector.IsLoaded        → true
    ⬇️
isPlayerClose == sector.IsLoaded  (변화 없음)
    ⬇️
MarkDirty() 호출 안 됨 → IsDirty == false
    ⬇️
연산 건너뜀 ✅ (씬 로드/언로드 미실행)
```

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : 더티 플래그의 핵심 로직 (GameSectors)

**위치 :** GameSectors.cs:23-51

```csharp
foreach (Sector sector in m_Sectors)
{
    bool isPlayerClose = sector.IsPlayerClose(m_Player.transform.position);

    // 핵심 1 : 상태가 달라질 때만 더티 표시
    if (isPlayerClose != sector.IsLoaded)
    {
        sector.MarkDirty();
    }

    // 핵심 2 : 더티할 때만 비싼 연산 실행
    if (sector.IsDirty)
    {
        if (isPlayerClose) sector.LoadContent();
        else               sector.UnloadContent();

        // 핵심 3 : 처리 후 반드시 클린으로 초기화
        sector.Clean();
    }
}
```

**이해 포인트 :**
- `isPlayerClose != sector.IsLoaded` 조건이 핵심 - **"현재 상태"와 "원하는 상태"가 다를 때만** 더티 표시
- `IsDirty` 체크로 비싼 연산을 선별 실행 → 대부분의 프레임은 플래그 체크만으로 끝남
- `Clean()` 호출을 빠뜨리면 매 프레임 씬 로드/언로드가 반복되는 버그 발생!

---

### 📌 핵심 코드 2 : 더티 플래그 상태 전환 (Sector)

**위치 :** Sector.cs:57-101

```csharp
// 플래그 ON : 업데이트가 필요한 상태로 표시
public void MarkDirty()
{
    IsDirty = true;
}

// 플래그 OFF : 처리 완료 후 클린으로 초기화
public void Clean()
{
    IsDirty = false;
}
```

**이해 포인트 :**
- `MarkDirty()`와 `Clean()`의 짝이 더티 플래그 패턴의 전부
- 단순하지만, 이 두 메서드가 비싼 연산의 실행 여부를 완전히 제어

---

### ★ 📌 핵심 코드 3 : LoadSceneAdditivelyByPath 내부 동작

**위치 :** SceneLoader.cs:151-167

```csharp
public void LoadSceneAdditivelyByPath(string scenePath)
{
    // 1. 이미 로드된 씬인지 확인 (중복 방지)
    Scene sceneToLoad = SceneManager.GetSceneByPath(scenePath);
    if (!sceneToLoad.IsValid())
    {
        // 2. m_AdditiveScenes 리스트에 추가 (추적)
        if (!m_AdditiveScenes.Contains(sceneToLoad))
            m_AdditiveScenes.Add(sceneToLoad);

        // 3. 비동기 Additive 로드 (기존 씬 유지)
        StartCoroutine(LoadAdditiveScene(scenePath));
    }
}

// 내부 코루틴 : LoadSceneAsync로 비동기 로드
private IEnumerator LoadAdditiveScene(string scenePath)
{
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

    while (!asyncLoad.isDone)
        yield return null; // 로드 완료까지 대기

    m_LastLoadedScene = SceneManager.GetSceneByPath(scenePath);
    SceneManager.SetActiveScene(m_LastLoadedScene); // 로드된 씬을 ActiveScene으로
}
```

---

### 📌 핵심 코드 4 : UnloadSceneByPath vs UnloadSceneImmediately

**위치 :** SceneLoader.cs:199-230

```csharp
// 게임 중 언로드 : 비동기 (UnloadSceneAsync)
public void UnloadSceneByPath(string scenePath)
{
    Scene sceneToUnload = SceneManager.GetSceneByPath(scenePath);
    if (sceneToUnload.IsValid())
        StartCoroutine(UnloadScene(sceneToUnload)); // 코루틴으로 비동기
}

// OnDestroy 시 언로드 : 동기 (UnloadScene)
public void UnloadSceneImmediately(string scenePath)
{
    Scene sceneToUnload = SceneManager.GetSceneByPath(scenePath);
    if (sceneToUnload.IsValid())
    {
        SceneManager.UnloadScene(sceneToUnload); // 즉시 동기 처리
        m_AdditiveScenes.Remove(sceneToUnload);
    }
}
```

**이해 포인트 :**
- 게임 중 → 비동기(`UnloadSceneAsync`) : 프레임 드랍 없이 부드럽게 처리
- 오브젝트 소멸 시 → 동기(`UnloadScene`) : 코루틴 실행 불가 타이밍이므로 즉시 처리
- 둘 다 에셋(Texture, Mesh 등)은 메모리에 잔류 → `Resources.UnloadUnusedAssets()` 추가 호출 필요

---

## 🌐 Additive Scene Loading과의 연관성

더티 플래그 패턴이 이 예제에서 특히 빛나는 이유는 **Additive Scene Loading** 때문입니다.

### Additive Scene Loading이란?

```
일반 로드 (Single) :  기존 씬 파괴 → 새 씬 로드
                      로딩 화면 필요, 플레이어 끊김

Additive 로드       :  기존 씬 유지 + 새 씬을 위에 추가
                      여러 프레임에 분산 처리, 심리스(seamless) 전환
```

Unity에서 제공하는 `SceneManager.LoadSceneAsync()` API를 사용하며, **비동기(Async)** 로 동작합니다.

### ⚠️ 중요 : "비동기 = 별도 스레드"가 아니다

`LoadSceneAsync`가 렉 없이 동작하는 이유를 흔히 **"별도 스레드에서 돌아가서"** 라고 오해하기 쉽지만, 실제 내부 동작은 더 정확한 이해가 필요합니다.

**Unity의 스레드 모델 :**

```
[메인 스레드] - Unity 게임 루프 전체가 여기서 동작
   Update(), FixedUpdate(), 렌더링, 물리 등 모든 게임 로직

[워커 스레드] - Unity가 내부적으로 관리하는 백그라운드 스레드
   파일 I/O (디스크에서 에셋 데이터 읽기)
   에셋 압축 해제 (Texture, Audio 등 디코딩)
   일부 에셋 역직렬화(Deserialization)

[절대 불가] - 메인 스레드에서만 실행 가능
   GameObject 생성/파괴
   컴포넌트의 Awake(), OnEnable(), Start() 호출
   Transform, 물리, 렌더링 관련 모든 Unity API
```

**`LoadSceneAsync`의 실제 처리 방식 :**

```
코루틴 : StartCoroutine(LoadAdditiveScene(path))
    ⬇️
AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(path, Additive)
    ⬇️
while (!asyncLoad.isDone)
{
    yield return null; ← 매 프레임 여기서 잠시 멈추고 게임에 제어권 반환
                          → 이 프레임의 Update, 렌더링이 정상 실행됨
                          → 다음 프레임에 이어서 로딩 진행
}

핵심 : "별도 스레드라서 렉이 없는 게 아니라,
        여러 프레임에 걸쳐 조금씩 나눠 처리하기 때문에 렉이 없다!"
```

**단계별 실제 동작 :**

| 단계 | 처리 위치 | 설명 |
|------|-----------|------|
| 파일 읽기 (I/O) | 워커 스레드 (일부) | 디스크에서 에셋 데이터 읽기 |
| 압축 해제 / 디코딩 | 워커 스레드 (일부) | Texture, Audio 등 디코딩 |
| GameObject 생성 | **메인 스레드** | Instantiate, Awake, Start 등 |
| 전체 분산 방식 | **여러 프레임** | `yield return null`로 프레임마다 조금씩 |

**비유 :**
```
❌ 잘못된 이해 : "다른 방에서 동시에 일하니 우리 방이 안 막힘"  (별도 스레드)
✅ 올바른 이해 : "일을 잘게 쪼개서 매일 조금씩 하니 하루 일과에 지장 없음" (프레임 분산)
```

### 왜 이 조합이 강력한가?

| 기술 | 역할 |
|------|------|
| **Additive Scene Loading** | 베이스 씬 유지하면서 구역 씬 동적 추가/제거 |
| **Dirty Flag** | 씬 로드/언로드(비싼 연산)를 꼭 필요한 때만 실행 |

```
오픈월드 전체 맵을 씬 단위로 분리
    + Dirty Flag로 변화 감지 시에만 로드/언로드
    = 부드러운 오픈월드 스트리밍
```

### 현업에서 Additive Scene의 실제 이점

```
✅ 협업 효율
   - 구역별로 씬이 분리 → 여러 아티스트/레벨 디자이너가 동시 작업 가능
   - 같은 파일을 건드리지 않으므로 Git 충돌 최소화

✅ 메모리 효율
   - 전체 맵을 한 번에 메모리에 올리지 않음
   - 플레이어 주변 구역만 올려두어 메모리 절약

✅ 로딩 화면 없는 심리스 전환
   - LoadSceneAsync(Additive)로 백그라운드 로드
   - 플레이어는 이음새 없이 자연스럽게 이동
```

---

## ⚖️ 장단점

### ✅ 장점

**1. 성능 최적화**
- 비싼 연산을 꼭 필요한 순간에만 실행
- 대부분의 프레임에서 가벼운 플래그 체크만 수행

**2. 구현 단순성**
- `bool IsDirty` 하나와 `MarkDirty()` / `Clean()` 두 메서드가 전부
- 기존 코드 구조를 크게 바꾸지 않고 도입 가능

**3. 예측 가능한 실행 제어**
- 언제 비싼 연산이 수행되는지 명확히 파악 가능
- 디버깅 시 `IsDirty` 값으로 상태 추적 용이

**4. 범용성**
- 씬 로딩뿐만 아니라 UI 갱신, 물리 계산, 파일 저장 등 어디서나 활용 가능

### ❌ 단점

**1. 프레임 지연 가능성**
- 더티 표시와 실제 처리 사이에 1프레임 이상의 지연 발생 가능
- 즉각적인 반응이 필요한 경우 부적합

**2. Clean() 누락 시 버그**
- `Clean()` 호출을 빠뜨리면 매 프레임 비싼 연산이 반복 실행되는 치명적 버그
- 플래그 관리를 철저히 해야 함

**3. 멀티스레드 환경에서 주의**
- 여러 스레드에서 `IsDirty`를 동시에 읽고 쓸 경우 동기화 필요
- Unity 단일 스레드 환경에서는 해당 없음

**4. 단일 상태만 추적**
- `bool` 플래그 하나로는 "무엇이 변했는지" 까지는 추적 불가
- 세밀한 변화 감지가 필요하다면 버전 번호나 해시 비교가 더 적합

---

## 🎮 실제 사용 사례

### 1️⃣ 오픈월드 씬 스트리밍 (이 예제)

```
플레이어 위치 기반으로 주변 구역 씬만 Additive 로드
더티 플래그로 경계를 넘는 순간에만 로드/언로드 실행
→ 배틀그라운드, 엘든링 등 오픈월드 게임의 핵심 기술
```

---

### 2️⃣ UI 갱신 최적화

```csharp
// 체력이 바뀔 때만 UI를 갱신 (매 프레임 갱신 방지)
public class HealthSystem
{
    private float m_Health;
    private bool  m_IsDirty;

    public float Health
    {
        get => m_Health;
        set
        {
            if (m_Health != value)
            {
                m_Health  = value;
                m_IsDirty = true; // 변화 발생 → 더티 표시
            }
        }
    }

    public void UpdateUI(HealthBar bar)
    {
        if (!m_IsDirty) return; // 변화 없으면 건너뜀

        bar.SetValue(m_Health); // UI 갱신 (비싼 연산)
        m_IsDirty = false;      // 클린으로 초기화
    }
}
```

---

### 3️⃣ 파일 저장 시스템

```csharp
// 게임 데이터가 변경될 때만 파일 저장 실행
public class SaveSystem
{
    private bool m_IsDirty = false;

    public void OnDataChanged()
    {
        m_IsDirty = true; // 데이터 변경 시 더티 표시
    }

    void Update()
    {
        if (!m_IsDirty) return;

        SaveToFile(); // 비싼 파일 I/O는 변화가 있을 때만
        m_IsDirty = false;
    }
}
```

---

### 4️⃣ 물리 시뮬레이션

```csharp
// 입력이나 충돌이 있을 때만 물리 재계산
public class PhysicsObject
{
    private bool m_IsDirty = false;

    public void OnCollision()   => m_IsDirty = true;
    public void OnInputChange() => m_IsDirty = true;

    void FixedUpdate()
    {
        if (!m_IsDirty) return;

        RecalculatePhysics(); // 비싼 물리 계산
        m_IsDirty = false;
    }
}
```

---

## 🎓 학습 정리

### 핵심 개념

**더티 플래그 패턴의 본질 :**
```
"비싼 연산은 꼭 필요할 때만 실행한다"

1. 변화 감지 → MarkDirty()  (가벼운 연산)
2. 더티 확인 → if (IsDirty) (플래그 체크)
3. 비싼 연산 → 실제 처리   (비싼 연산)
4. 클린 초기화 → Clean()    (플래그 초기화)
```

### 주요 구성 요소

```
더티 플래그 보유 객체 (Sector)
  ↓
  IsDirty 플래그 + MarkDirty() + Clean() + 비싼 연산(Load/Unload)

더티 플래그 관리자 (GameSectors)
  ↓
  매 프레임 : 변화 감지 → MarkDirty → IsDirty 체크 → 비싼 연산 → Clean
```

### 설계 기준

```
이 연산이 비싼가? (씬 로드, 파일 I/O, UI 갱신, 물리 계산 등)
  ↓ YES → 더티 플래그 패턴 도입 고려
  ↓
이 연산이 매번 필요한가?
  ↓ NO  → 더티 플래그로 "변화가 있을 때만" 실행
  ↓ YES → 더티 플래그 불필요
```

### 언제 사용해야 할까?

**✅ Dirty Flag Pattern을 사용하면 좋은 경우 :**
- 비싼 연산(씬 로드, 파일 I/O, 렌더링 갱신 등)을 매 프레임 실행하고 있을 때
- 입력이나 상태 변화가 간헐적으로 발생하는 경우
- 오픈월드 레벨 스트리밍, UI 갱신 최적화, 자동 저장 시스템 등

**❌ Dirty Flag Pattern을 피해야 하는 경우 :**
- 연산이 가볍고 매 프레임 실행해도 성능에 영향 없는 경우
- 즉각적인 응답이 필요한 시스템 (1프레임 지연도 허용 불가)
- 상태 변화가 거의 매 프레임 발생하는 경우 (플래그 오버헤드만 증가)

### 관련 패턴

| 패턴 | 관계 |
|------|------|
| **Observer** | 변화 감지 방법이 유사, Observer는 즉각 실행 / Dirty Flag는 지연 실행 |
| **Flyweight** | 둘 다 최적화 패턴, Flyweight는 메모리 최적화 / Dirty Flag는 연산 최적화 |
| **Object Pool** | 함께 사용 가능, Pool로 씬 오브젝트 재사용 + Dirty Flag로 갱신 최소화 |

### 마무리

**기억할 점 :**
- ✅ `MarkDirty()` + `Clean()` 짝을 항상 유지 (Clean 누락 = 치명적 버그)
- ✅ 더티 플래그는 "언제 비싼 연산을 할지"를 제어하는 게이트
- ✅ Additive Scene Loading과 조합하면 오픈월드 스트리밍의 핵심 기술
- ⚠️ 플래그 체크 자체도 연산 비용이므로, 연산이 충분히 비쌀 때만 도입
- 🎯 씬 스트리밍, UI 갱신, 자동 저장, 물리 계산 최적화에 특히 유용

---

**작성일 :** 2026.03.03
**참고 자료 :** Unity Korea - Level Up Your Code with Design Patterns and SOLID
**블로그 정리 :** [tae-woong.tistory.com/171](https://tae-woong.tistory.com/171)
