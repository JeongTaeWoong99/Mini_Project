# 🎮 Singleton Pattern (싱글톤 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [왜 Singleton Pattern이 필요한가?](#-왜-singleton-pattern이-필요한가)
- [핵심 구성요소](#-핵심-구성요소)
- [3가지 싱글톤 구현 비교](#-3가지-싱글톤-구현-비교)
- [코드 구조](#-코드-구조)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 사용 사례](#-실제-사용-사례)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**Singleton Pattern**은 **생성 패턴(Creational Pattern)** 중 하나로, 클래스의 인스턴스가 **단 하나만** 존재하도록 보장하고, 이에 대한 전역 접근을 제공하는 패턴입니다.

### 📌 핵심 개념

```
클래스의 인스턴스를 "단 하나"만 생성하고, 어디서든 접근 가능하게!
```

**일반적인 방법 :**
```csharp
// 여러 GameManager 인스턴스가 생성될 수 있음
public class GameManager : MonoBehaviour
{
    // 어떻게 접근하지?
}
// ❌ 문제 : 중복 인스턴스 가능, 접근 방법 복잡
```

**Singleton Pattern :**
```csharp
// 단 하나의 GameManager만 존재
public class GameManager : Singleton<GameManager>
{
    // 어디서든 접근 가능
}

// 사용 : 어디서든 접근
GameManager.Instance.DoSomething();
// ✅ 장점 : 단일 인스턴스 보장, 전역 접근 가능!
```

---

## 🤔 왜 Singleton Pattern이 필요한가?

### 문제 상황

게임에서 AudioManager를 만들 때, 일반적으로 이렇게 작성합니다 :

```csharp
public class AudioManager : MonoBehaviour
{
    public void PlaySound(AudioClip clip)
    {
        // 사운드 재생 로직
    }
}

// 다른 스크립트에서 사용하려면?
public class Player : MonoBehaviour
{
    void OnAttack()
    {
        // ❌ AudioManager를 어떻게 찾지?
        AudioManager audio = FindFirstObjectByType<AudioManager>();
        audio.PlaySound(attackSound);
    }
}
```

**이 코드의 문제점 :**

❌ **접근이 복잡함**
   - 매번 FindFirstObjectByType 호출 필요
   - 성능 저하 (Find 계열은 느림)

❌ **중복 인스턴스 가능**
   - 실수로 여러 AudioManager 생성 가능
   - 어떤 인스턴스를 사용해야 할지 모호함

❌ **씬 전환 시 사라짐**
   - 새 씬 로드 시 AudioManager 파괴
   - 지속적인 관리가 어려움

❌ **null 참조 위험**
   - AudioManager가 없으면 NullReferenceException
   - 방어 코드 필요

### Singleton Pattern의 해결책

✅ **전역 접근 제공**
   - `AudioManager.Instance`로 간단히 접근
   - Find 계열 함수 불필요

✅ **단일 인스턴스 보장**
   - 자동으로 중복 인스턴스 제거
   - 항상 하나만 존재

✅ **씬 전환에도 유지 가능**
   - DontDestroyOnLoad로 영구 보존
   - 게임 전체에서 일관성 유지

✅ **자동 생성**
   - 인스턴스가 없으면 자동 생성
   - null 체크 불필요

---

## 🏗️ 핵심 구성요소

Singleton Pattern은 다음 핵심 요소들로 구성됩니다 :

### 1️⃣ Static Instance (정적 인스턴스)

```csharp
private static T s_Instance;

public static T Instance
{
    get
    {
        if (s_Instance == null)
        {
            // 인스턴스가 없으면 찾거나 생성
            s_Instance = FindOrCreate();
        }
        return s_Instance;
    }
}
```

**역할 :**
- 전역에서 접근 가능한 유일한 인스턴스
- Lazy Initialization (지연 초기화) 지원

---

### 2️⃣ Private Constructor (비공개 생성자) 개념

**일반 클래스의 경우 :**
```csharp
public class NormalManager
{
    // 누구나 new로 인스턴스 생성 가능
    NormalManager manager1 = new NormalManager();
    NormalManager manager2 = new NormalManager();
    // ❌ 여러 인스턴스 생성 가능!
}
```

**싱글톤의 경우 :**
```csharp
// MonoBehaviour는 생성자를 private으로 만들 수 없음
// 대신 Awake에서 중복 체크
protected virtual void Awake()
{
    if (s_Instance != null && s_Instance != this)
    {
        Destroy(gameObject);  // 중복 인스턴스 파괴
    }
}
```

---

### 3️⃣ Duplicate Prevention (중복 방지)

```csharp
public void RemoveDuplicates()
{
    if (s_Instance == null)
    {
        s_Instance = this as T;
    }
    else if (s_Instance != this)
    {
        Destroy(gameObject);  // 이미 인스턴스가 있으면 파괴
    }
}
```

**역할 :**
- 씬에 여러 싱글톤이 있으면 중복 제거
- 첫 번째 인스턴스만 유지

---

### 4️⃣ Persistence (영구성) - Optional

```csharp
protected virtual void Awake()
{
    if (s_Instance == null)
    {
        s_Instance = this as T;
        DontDestroyOnLoad(gameObject);  // 씬 전환 시에도 유지
    }
}
```

**역할 :**
- 씬 전환 시에도 싱글톤 유지
- 게임 전체에서 일관성 보장

---

## 🔀 3가지 싱글톤 구현 비교

이 프로젝트에는 3가지 싱글톤 구현이 포함되어 있습니다 :

### 📊 비교표

| 특징 | SimpleSingleton | Singleton<T> | PersistentSingleton<T> |
|------|----------------|--------------|----------------------|
| **📁 파일** | [SimpleSingleton.cs](./Scripts/Pattern/SimpleSingleton.cs) | [Singleton.cs](./Scripts/Pattern/Singleton.cs) | [PersistentSingleton.cs](./Scripts/Pattern/PersistentSingleton.cs) |
| **제네릭 재사용** | ❌ | ✅ | ✅ |
| **자동 생성** | ❌ | ✅ | ✅ |
| **씬 전환 시 유지** | ❌ | ❌ (데모용 파괴) | ✅ (DontDestroyOnLoad) |
| **씬 언로드 이벤트** | ❌ | ✅ (자동 정리) | ❌ |
| **복잡도** | 낮음 | 중간 | 중간 |
| **추천 용도** | 학습/프로토타입 | 씬별 독립 매니저 | 전역 영구 매니저 |

---

### 1️⃣ SimpleSingleton

**📁 파일 :** [SimpleSingleton.cs](./Scripts/Pattern/SimpleSingleton.cs)

**특징 :**
```csharp
public class SimpleSingleton : MonoBehaviour
{
    public static SimpleSingleton Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);  // 중복 파괴
        }
        else
        {
            Instance = this;      // 첫 인스턴스 저장
        }
    }
}
```

**장점 :**
- ✅ 가장 단순한 구현
- ✅ 이해하기 쉬움
- ✅ 학습에 적합

**단점 :**
- ❌ 제네릭 아님 (재사용 불가)
- ❌ 자동 생성 없음
- ❌ 씬 전환 시 파괴됨

**사용 시점 :**
- 프로토타입 개발
- 패턴 학습
- 단일 씬 게임

---

### 2️⃣ Singleton<T> (제네릭 싱글톤)

**📁 파일 :** [Singleton.cs](./Scripts/Pattern/Singleton.cs)

**특징 :**
```csharp
public class Singleton<T> : MonoBehaviour where T : Component
{
    private static T s_Instance;

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<T>();
                if (s_Instance == null)
                {
                    // 자동 생성
                    GameObject obj = new GameObject(typeof(T).Name);
                    s_Instance = obj.AddComponent<T>();
                }
            }
            return s_Instance;
        }
    }

    private void OnEnable()
    {
        // 씬 언로드 시 인스턴스 정리 (데모용)
        SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
    }

    private void SceneManager_SceneUnloaded(Scene scene)
    {
        if (s_Instance != null)
            Destroy(s_Instance.gameObject);

        s_Instance = null;
    }
}
```

**장점 :**
- ✅ 제네릭으로 재사용 가능
- ✅ 자동 생성 지원
- ✅ 씬별 독립적 인스턴스
- ✅ 자동 정리 (메모리 관리)

**단점 :**
- ❌ 씬 전환 시 파괴됨
- ❌ 영구 매니저에는 부적합

**사용 시점 :**
- 씬별로 다른 설정이 필요한 매니저
- 데모/테스트 환경
- 씬 전환 시 리셋이 필요한 경우

**사용 예시 :**
```csharp
// GameManager.cs
public class GameManager : Singleton<GameManager>
{
    public int score;
    // 각 씬마다 새로운 GameManager 생성
}
```

---

### 3️⃣ PersistentSingleton<T> (영구 싱글톤)

**📁 파일 :** [PersistentSingleton.cs](./Scripts/Pattern/PersistentSingleton.cs)

**특징 :**
```csharp
public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
    private static T s_Instance;

    public static T Instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindFirstObjectByType<T>();
                if (s_Instance == null)
                {
                    GameObject obj    = new GameObject(typeof(T).Name);
                    s_Instance        = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);  // 씬 전환에도 유지!
                }
            }
            return s_Instance;
        }
    }

    protected virtual void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this as T;
            DontDestroyOnLoad(gameObject);  // 영구 보존
        }
        else if (s_Instance != this)
        {
            Destroy(gameObject);  // 중복 제거
        }
    }
}
```

**장점 :**
- ✅ 제네릭으로 재사용 가능
- ✅ 자동 생성 지원
- ✅ 씬 전환에도 유지 (DontDestroyOnLoad)
- ✅ 게임 전체에서 일관성 보장

**단점 :**
- ❌ 수동으로 파괴해야 함
- ❌ 메모리에 계속 남음

**사용 시점 :**
- 게임 전체에서 유지되어야 하는 매니저
- 데이터 관리 (SaveManager, DataManager)
- 사운드 관리 (AudioManager, SoundManager)
- 네트워크 관리 (NetworkManager)

**사용 예시 :**
```csharp
// AudioManager.cs
public class AudioManager : PersistentSingleton<AudioManager>
{
    public AudioSource audioSource;

    public void PlaySound(AudioClip clip)
    {
        // 모든 씬에서 동일한 AudioManager 사용
        audioSource.PlayOneShot(clip);
    }
}

// 다른 씬에서도 동일하게 사용
AudioManager.Instance.PlaySound(clip);
```

---

### 🎯 어떤 싱글톤을 사용해야 할까?

```
질문 1 : 제네릭 재사용이 필요한가?
  ├─ NO  → SimpleSingleton (학습용)
  └─ YES → 질문 2로

질문 2 : 씬 전환 시에도 유지되어야 하는가?
  ├─ NO  → Singleton<T> (씬별 독립)
  └─ YES → PersistentSingleton<T> (영구 보존)
```

**예시 :**
- **SimpleSingleton** : 프로토타입, 단일 씬 게임
- **Singleton<T>** : 레벨별 GameManager, 씬별 UIManager
- **PersistentSingleton<T>** : AudioManager, SaveManager, NetworkManager

---

## 📊 코드 구조

### 폴더 구조

```
3_Singleton/
├── Scripts/
│   ├── Pattern/                        (핵심 패턴 구현)
│   │   ├── SimpleSingleton.cs         ← 기본 싱글톤
│   │   ├── Singleton.cs               ← 제네릭 싱글톤 (씬별)
│   │   └── PersistentSingleton.cs     ← 영구 싱글톤
│   │
│   └── ExampleUsage/                  (사용 예시)
│       ├── GameManager.cs             ← Singleton<T> 사용 예시
│       ├── AudioManager.cs            ← Singleton<T> 사용 예시
│       ├── ClickToPlaySound.cs        ← 싱글톤 접근 예시
│       └── EnableTextOnStart.cs       ← UI 헬퍼
│
└── README.md                           ← 📍 현재 문서
```

### 클래스 다이어그램

```
┌─────────────────────────┐
│   SimpleSingleton       │  ← 기본 싱글톤
├─────────────────────────┤
│ + static Instance       │
├─────────────────────────┤
│ - Awake()               │
└─────────────────────────┘


┌─────────────────────────┐
│   Singleton<T>          │  ← 제네릭 싱글톤 (씬별)
├─────────────────────────┤
│ - static s_Instance     │
│ + static Instance {get} │
│ - m_DelayDuplicateRemoval│
├─────────────────────────┤
│ + Awake()               │
│ + RemoveDuplicates()    │
│ - OnEnable()            │
│ - OnDisable()           │
│ - SceneManager_SceneUnloaded()│
└─────────────────────────┘
         △
         │ inherits
         │
┌─────────────────────────┐
│   GameManager           │  ← 사용 예시
└─────────────────────────┘


┌─────────────────────────┐
│ PersistentSingleton<T>  │  ← 영구 싱글톤
├─────────────────────────┤
│ - static s_Instance     │
│ + static Instance {get} │
├─────────────────────────┤
│ # Awake()               │
└─────────────────────────┘
         △
         │ inherits
         │
┌─────────────────────────┐
│   AudioManager          │  ← 사용 예시
├─────────────────────────┤
│ + audioSource           │
│ + volume                │
│ + pitch                 │
├─────────────────────────┤
│ + PlaySoundEffect()     │
└─────────────────────────┘
```

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : Lazy Initialization (지연 초기화)

**위치 :** Singleton.cs:23-46

```csharp
public static T Instance
{
    get
    {
        // ✅ 핵심 : 접근 시점에 인스턴스 확인 및 생성
        if (s_Instance == null)
        {
            // 1. 먼저 씬에서 찾아봄
            s_Instance = (T)FindFirstObjectByType(typeof(T));

            if (s_Instance == null)
            {
                // 2. 없으면 자동 생성
                SetupInstance();
            }
            else
            {
                Debug.Log("[Singleton] " + typeof(T).Name +
                         " instance already created: " +
                         s_Instance.gameObject.name);
            }
        }

        return s_Instance;
    }
}
```

**이해 포인트 :**
- **Lazy Initialization** : 실제로 필요할 때 생성
- 게임 시작 시가 아닌 **첫 접근 시** 생성
- 불필요한 인스턴스 생성 방지 (성능 최적화)
- 자동 생성으로 null 체크 불필요

---

### 📌 핵심 코드 2 : 중복 인스턴스 제거

**위치 :** Singleton.cs:84-97

```csharp
public void RemoveDuplicates()
{
    if (s_Instance == null)
    {
        // ✅ 첫 번째 인스턴스 : 등록
        s_Instance = this as T;
    }
    else if (s_Instance != this)
    {
        // ✅ 중복 인스턴스 : 파괴
        Destroy(gameObject);
    }
}
```

**이해 포인트 :**
- 씬에 여러 싱글톤이 있으면 자동 제거
- 첫 번째 인스턴스만 유지
- Awake에서 호출되어 자동 처리

---

### 📌 핵심 코드 3 : 씬 언로드 처리 (Singleton<T>)

**위치 :** Singleton.cs:55-67, 99-108

```csharp
private void OnEnable()
{
    // ✅ 씬 언로드 이벤트 구독
    SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
}

private void OnDisable()
{
    if (s_Instance == this as T)
    {
        // ✅ 이벤트 구독 해제
        SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;
    }
}

// 씬 언로드 시 싱글톤 파괴 (데모 용도 전용)
private void SceneManager_SceneUnloaded(Scene scene)
{
    if (s_Instance != null)
        Destroy(s_Instance.gameObject);

    s_Instance = null;  // 인스턴스 초기화
}
```

**이해 포인트 :**
- 씬 전환 시 자동으로 인스턴스 정리
- 메모리 누수 방지
- 각 씬마다 새로운 인스턴스 생성 가능
- 데모/테스트 환경에 적합

---

### 📌 핵심 코드 4 : DontDestroyOnLoad (PersistentSingleton<T>)

**위치 :** PersistentSingleton.cs:39-51

```csharp
protected virtual void Awake()
{
    if (s_Instance == null)
    {
        s_Instance = this as T;
        // ✅ 핵심 : 씬 전환에도 파괴되지 않음
        DontDestroyOnLoad(this.gameObject);
    }
    else if (s_Instance != this)
    {
        // 중복 인스턴스는 파괴
        Destroy(gameObject);
    }
}
```

**이해 포인트 :**
- `DontDestroyOnLoad` : 씬 전환에도 유지
- 게임 전체에서 동일한 인스턴스 사용
- AudioManager, SaveManager 등에 적합

---

### 📌 핵심 코드 5 : 싱글톤 사용 (AudioManager 예시)

**위치 :** AudioManager.cs:12-35

```csharp
public class AudioManager : Singleton<AudioManager>
{
    public AudioSource audioSource;
    public Vector2     volume = new Vector2(0.5f, 0.9f);
    public Vector2     pitch  = new Vector2(0.8f, 1.2f);

    // 지정된 AudioSource에서 클립 재생
    public void PlaySoundEffect(AudioClip clip)
    {
        if (audioSource == null)
            return;

        // 볼륨과 피치 무작위화
        audioSource.volume = Random.Range(volume.x, volume.y);
        audioSource.pitch  = Random.Range(pitch.x, pitch.y);

        // 클립 업데이트
        audioSource.clip = clip;
        // 다시 재생하기 전에 오디오 소스가 중지되었는지 확인
        audioSource.Stop();
        audioSource.Play();
    }
}

// ✅ 다른 스크립트에서 사용
AudioManager.Instance.PlaySoundEffect(myClip);
```

---

### 📌 핵심 코드 6 : 싱글톤 접근 (ClickToPlaySound 예시)

**위치 :** ClickToPlaySound.cs:7-35

```csharp
// 정적 싱글톤 인스턴스에 접근하는 방법을 보여주는 예제
public class ClickToPlaySound : MonoBehaviour
{
    [SerializeField] private AudioClip m_Clip;
    [SerializeField] private LayerMask m_LayerToClick;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 레이캐스트로 콜라이더를 클릭했는지 확인
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, Mathf.Infinity, m_LayerToClick))
            {
                PlaySoundFromAudioManager();
            }
        }
    }

    // 전역 싱글톤 인스턴스에서 오디오 클립 재생
    private void PlaySoundFromAudioManager()
    {
        if (m_Clip != null)
        {
            // ✅ 핵심 : 싱글톤 Instance를 통한 전역 접근
            AudioManager.Instance.PlaySoundEffect(m_Clip);
        }
    }
}
```

**이해 포인트 :**
- `AudioManager.Instance`로 간단히 접근
- Find 계열 함수 불필요
- null 체크 불필요 (자동 생성)
- 어디서든 동일하게 사용 가능

---

## ⚖️ 장단점

### ✅ 장점

**1. 전역 접근 편의성**
- 어디서든 `ClassName.Instance`로 접근
- FindFirstObjectByType, GetComponent 불필요
- 코드 간결화

**2. 단일 인스턴스 보장**
- 자동으로 중복 제거
- 실수로 여러 인스턴스 생성 방지
- 일관성 보장

**3. Lazy Initialization**
- 실제 필요할 때 생성
- 초기 로딩 시간 단축
- 메모리 효율적

**4. 생명주기 제어**
- DontDestroyOnLoad로 영구 보존 가능
- 씬 전환에도 유지
- 게임 전체에서 일관성

**5. 구현의 간편함**
- 제네릭 클래스로 재사용 가능
- 상속만으로 싱글톤 구현
- 보일러플레이트 코드 최소화

### ❌ 단점

**1. 전역 상태 (Global State)**
- 암묵적 의존성 증가
- 테스트 어려움
- 디버깅 복잡

**2. 강한 결합 (Tight Coupling)**
- 싱글톤에 직접 의존
- 코드 재사용성 감소
- 리팩토링 어려움

**3. 멀티스레드 안전하지 않음**
- Unity는 단일 스레드이지만
- 멀티스레드 환경에서는 추가 처리 필요

**4. 생명주기 관리 어려움**
- 언제 파괴되어야 하는지 불명확
- 메모리 누수 가능성
- 수동 정리 필요

**5. 과도한 사용 시 안티패턴**
- 모든 것을 싱글톤으로 만들면 문제
- "전역 변수"와 유사한 문제 발생
- 의존성 주입(DI) 등 대안 고려 필요

**6. 상속 제약**
- 이미 싱글톤을 상속받으면
- 다른 클래스 상속 불가 (C# 단일 상속)

---

## 🎮 실제 사용 사례

### 1️⃣ 게임 개발 - 매니저 시스템

**AudioManager (오디오 관리)**
```csharp
public class AudioManager : PersistentSingleton<AudioManager>
{
    public void PlayBGM(AudioClip clip) { }
    public void PlaySFX(AudioClip clip) { }
    public void SetVolume(float volume) { }
}

// 어디서든 사용
AudioManager.Instance.PlaySFX(coinSound);
```

**GameManager (게임 상태 관리)**
```csharp
public class GameManager : Singleton<GameManager>
{
    public int  score;
    public int  lives;
    public bool isPaused;

    public void AddScore(int points) { }
    public void GameOver() { }
}

// 어디서든 사용
GameManager.Instance.AddScore(100);
```

**SaveManager (저장/로드)**
```csharp
public class SaveManager : PersistentSingleton<SaveManager>
{
    public void SaveGame() { }
    public void LoadGame() { }
}

SaveManager.Instance.SaveGame();
```

---

### 2️⃣ UI 관리

**UIManager**
```csharp
public class UIManager : Singleton<UIManager>
{
    public void ShowPopup(string message) { }
    public void HidePopup() { }
    public void UpdateHealthBar(float health) { }
}

// 플레이어가 데미지를 받으면
UIManager.Instance.UpdateHealthBar(currentHealth);
```

---

### 3️⃣ 네트워크 게임

**NetworkManager**
```csharp
public class NetworkManager : PersistentSingleton<NetworkManager>
{
    public void Connect() { }
    public void Disconnect() { }
    public void SendData(byte[] data) { }
}

// 어디서든 접근
NetworkManager.Instance.SendData(playerPosition);
```

---

### 4️⃣ 입력 관리

**InputManager**
```csharp
public class InputManager : Singleton<InputManager>
{
    public Vector2 GetMovementInput() { }
    public bool    GetJumpInput() { }
}

// 플레이어 스크립트에서
Vector2 input = InputManager.Instance.GetMovementInput();
```

---

### 5️⃣ 데이터 관리

**DataManager**
```csharp
public class DataManager : PersistentSingleton<DataManager>
{
    public PlayerData  playerData;
    public SettingData settingData;

    public void LoadAllData() { }
}

// 어디서든 플레이어 데이터 접근
int gold = DataManager.Instance.playerData.gold;
```

---

## 🎓 학습 정리

### 핵심 개념

**싱글톤 패턴의 본질 :**
```
클래스의 인스턴스를 단 하나만 생성하고,
전역에서 접근 가능하게 만드는 패턴
```

### 3가지 구현의 선택 기준

```
SimpleSingleton
  ↓
  단순, 학습용, 재사용 불가

Singleton<T>
  ↓
  제네릭, 자동 생성, 씬별 독립

PersistentSingleton<T>
  ↓
  제네릭, 자동 생성, 영구 보존
```

### 언제 사용해야 할까?

**✅ 싱글톤을 사용하면 좋은 경우 :**
- 게임 전체에서 **단 하나**만 있어야 하는 객체
- **전역 접근**이 필요한 매니저 클래스
- AudioManager, SaveManager, NetworkManager 등

**❌ 싱글톤을 피해야 하는 경우 :**
- 여러 인스턴스가 필요한 경우
- 테스트가 중요한 경우
- 의존성 주입(DI)을 사용하는 경우

### 대안 고려

**의존성 주입 (Dependency Injection) :**
```csharp
public class Player : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;  // Inspector에서 할당

    void OnAttack()
    {
        audioManager.PlaySFX(attackSound);  // 싱글톤 대신 주입된 인스턴스 사용
    }
}
```

**ScriptableObject 기반 아키텍처 :**
```csharp
[CreateAssetMenu]
public class GameSettings : ScriptableObject
{
    public float volume;
    public int   quality;
}
```

### 마무리

싱글톤 패턴은 **강력하지만 신중하게 사용**해야 하는 패턴입니다.

**기억할 점 :**
- ✅ 편리함과 전역 접근성 제공
- ⚠️ 과도한 사용은 안티패턴
- 🎯 적절한 상황에서만 사용
- 🔄 대안(DI, ScriptableObject)도 고려

---

**작성일 :** 2025.12.08
**참고 자료 :** Unity Korea - Level Up Your Code with Design Patterns and SOLID
