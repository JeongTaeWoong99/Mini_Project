# 🎮 Observer Pattern (옵저버 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [왜 Observer Pattern이 필요한가?](#-왜-observer-pattern이-필요한가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 사용 사례](#-실제-사용-사례)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**Observer Pattern**은 **행동 패턴(Behavioral Pattern)** 중 하나로, 객체의 상태 변화를 관찰하는 옵저버들에게 자동으로 알림을 보내는 패턴입니다.

### 📌 핵심 개념

```
하나의 객체(Subject)의 상태 변화를
여러 관찰자(Observer)에게 자동으로 알린다!
```

**일반적인 방법 :**
```csharp
// 버튼 클릭 시 여러 컴포넌트에 직접 알림
public class Button : MonoBehaviour
{
    public ParticleSystem particles;
    public AudioSource    audioSource;
    public Animation      animation;

    void OnClick()
    {
        particles.Play();      // 직접 호출
        audioSource.Play();    // 직접 호출
        animation.Play();      // 직접 호출
    }
}
// ❌ 문제 : 강한 결합, 확장 어려움, 의존성 많음
```

**Observer Pattern :**
```csharp
// 버튼은 이벤트만 발행
public class ButtonSubject : MonoBehaviour
{
    public event Action Clicked;

    void OnClick()
    {
        Clicked?.Invoke();  // 구독자들에게 알림
    }
}

// 각 컴포넌트가 독립적으로 구독
particles.Subscribe(button.Clicked);
audio.Subscribe(button.Clicked);
animation.Subscribe(button.Clicked);
// ✅ 장점 : 느슨한 결합, 쉬운 확장, 독립적!
```

---

## 🤔 왜 Observer Pattern이 필요한가?

### 문제 상황

게임에서 버튼을 클릭했을 때 파티클, 사운드, 애니메이션을 재생하려면, 일반적으로 이렇게 작성합니다 :

```csharp
public class Button : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public AudioSource    audioSource;
    public Animation      animation;

    void OnClick()
    {
        // 각각 직접 호출
        if (particleSystem != null)
            particleSystem.Play();

        if (audioSource != null)
            audioSource.Play();

        if (animation != null)
            animation.Play();
    }
}
```

**이 코드의 문제점 :**

❌ **강한 결합 (Tight Coupling)**
   - Button이 ParticleSystem, AudioSource, Animation을 모두 알아야 함
   - 한 컴포넌트가 다른 모든 컴포넌트에 의존

❌ **확장 어려움**
   - 새로운 효과를 추가하려면 Button 코드 수정 필요
   - OCP(개방-폐쇄 원칙) 위반

❌ **재사용 불가능**
   - Button이 특정 컴포넌트에 강하게 결합됨
   - 다른 프로젝트에서 재사용 어려움

❌ **단일 책임 위반**
   - Button이 클릭 감지뿐만 아니라 모든 효과 관리까지 담당
   - SRP(단일 책임 원칙) 위반

### Observer Pattern의 해결책

✅ **느슨한 결합 (Loose Coupling)**
   - Subject는 Observer의 구체적인 구현을 몰라도 됨
   - 인터페이스(이벤트)만 알면 됨

✅ **쉬운 확장**
   - 새로운 Observer 추가 시 기존 코드 수정 불필요
   - OCP 원칙 준수

✅ **높은 재사용성**
   - Subject와 Observer가 독립적
   - 각각 다른 프로젝트에서 재사용 가능

✅ **단일 책임 준수**
   - Subject는 이벤트 발행만 담당
   - Observer는 반응만 담당

---

## 🏗️ 핵심 구성요소

Observer Pattern은 다음 2가지 핵심 요소로 구성됩니다 :

### 1️⃣ Subject (주체/발행자)

**📁 파일 :** [Subject.cs](./Scripts/Pattern/Subject.cs)

```csharp
public class Subject : MonoBehaviour
{
    // 자신만의 델리게이트로 이벤트 정의하기
    //public delegate void ExampleDelegate();
    //public static event ExampleDelegate ExampleEvent;

    // ... 또는 그냥 System.Action 사용하기
    public event Action ThingHappened;

    // 이벤트를 호출하여 모든 리스너/옵저버에게 브로드캐스트
    public void DoThing()
    {
        ThingHappened?.Invoke();
    }
}
```

**역할 :**
- 상태 변화나 이벤트를 감지
- 이벤트(Event)를 정의하고 발행
- Observer들을 모르지만 이벤트를 통해 알림

**특징 :**
- `event Action` : C#의 이벤트 시스템 활용
- `?.Invoke()` : null 체크와 함께 안전하게 호출
- Observer의 구체적인 타입을 몰라도 됨

---

### 2️⃣ Observer (관찰자/구독자)

**📁 파일 :** [Observer.cs](./Scripts/Pattern/Observer.cs)

```csharp
public class ExampleObserver : MonoBehaviour
{
    // 관찰/리스닝할 Subject에 대한 참조
    [SerializeField] Subject subjectToObserve;

    // 이벤트 처리 메서드 : 함수 시그니처는 Subject의 이벤트와 일치해야 함
    private void OnThingHappened()
    {
        // 이벤트에 반응하는 로직을 여기에 작성
    }

    private void Awake()
    {
        // Subject의 이벤트에 구독/등록
        if (subjectToObserve != null)
        {
            subjectToObserve.ThingHappened += OnThingHappened;
        }
    }

    private void OnDestroy()
    {
        // 오브젝트가 파괴될 때 구독 해제/등록 취소
        if (subjectToObserve != null)
        {
            subjectToObserve.ThingHappened -= OnThingHappened;
        }
    }
}
```

**역할 :**
- Subject의 이벤트를 구독
- 이벤트 발생 시 자동으로 콜백 메서드 실행
- 파괴 시 구독 해제하여 메모리 누수 방지

**특징 :**
- `+=` : 이벤트 구독 (Subscribe)
- `-=` : 이벤트 구독 해제 (Unsubscribe)
- 여러 Observer가 하나의 Subject를 구독 가능

---

## 📊 코드 구조

### 폴더 구조

```
6_Observer/
├── Scripts/
│   ├── Pattern/                          (패턴 구현 프레임)
│   │   ├── Subject.cs                   ← Subject 기본 구조
│   │   └── Observer.cs                  ← Observer 기본 예제
│   │
│   └── ExampleUsage/                    (사용 예시)
│       ├── ButtonSubject.cs             ← 클릭 가능한 버튼 (Subject)
│       ├── ClickCollider.cs             ← 클릭 감지 컴포넌트 (분리형)
│       ├── ParticleSystemObserver.cs    ← 파티클 재생 Observer
│       ├── AudioObserver.cs             ← 사운드 재생 Observer
│       └── AnimObserver.cs              ← 애니메이션 재생 Observer
│
└── README.md                             ← 📍 현재 문서
```

### ExampleUsage 폴더 사용 예시 클래스 다이어그램

```
┌─────────────────────┐
│  ButtonSubject      │  ← 구체적인 Subject(입력 감지 포함 버전)
├─────────────────────┤
│ + event Clicked     │
├─────────────────────┤
│ + ClickButton()     │
│ - CheckCollider()   │
└─────────────────────┘
         │
         │ notifies (이벤트 발행)
         │
         ├──────────────┬──────────────┬──────────────┐
         ▼              ▼              ▼              ▼
┌────────────────┐ ┌────────────┐ ┌────────────┐ ┌────────────┐
│ParticleSystem  │ │AudioObserver│ │AnimObserver│ │ClickCollider│
│Observer        │ │            │ │            │ │            │
├────────────────┤ ├────────────┤ ├────────────┤ ├────────────┤
│+ OnThingHappened│ │+ OnThingHap│ │+ OnThingHap│ │- CheckCollider│
│  - Play()      │ │  pened()   │ │  pened()   │ │            │
└────────────────┘ └────────────┘ └────────────┘ └────────────┘
    Observer 1       Observer 2     Observer 3    입력 감지(따로 분리 버전)
```

---

## 🔄 실행 흐름

### 1️⃣ 초기화 흐름 (구독 설정)

```
[게임 시작]
    ⬇️
각 Observer의 Awake() 호출
    ⬇️
┌─────────────────────────────────┐
│ ParticleSystemObserver.Awake()  │
│   subjectToObserve.Clicked      │
│   += OnThingHappened            │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ AudioObserver.Awake()           │
│   subjectToObserve.Clicked      │
│   += OnThingHappened            │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ AnimObserver.Start()            │
│   subjectToObserve.Clicked      │
│   += OnThingHappened            │
└─────────────────────────────────┘
    ⬇️
✅ 모든 Observer가 Subject의 이벤트에 구독 완료!
```

---

### 2️⃣ 이벤트 발생 흐름

```
[사용자 입력]
    ⬇️
마우스 클릭
    ⬇️
┌─────────────────────────────────┐
│ ButtonSubject.CheckCollider()   │
│   - Raycast로 클릭 감지         │
│   - 콜라이더 히트 확인          │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ ButtonSubject.ClickButton()     │
│   Clicked?.Invoke()             │
│   → 이벤트 발행!                │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ ✅ 모든 구독자에게 자동 알림!   │
└─────────────────────────────────┘
    │
    ├──────────────┬──────────────┬──────────────┐
    ▼              ▼              ▼              ▼
┌────────────┐ ┌────────────┐ ┌────────────┐ (동시 호출)
│Particle    │ │Audio       │ │Animation   │
│Observer    │ │Observer    │ │Observer    │
│            │ │            │ │            │
│Play()      │ │Play()      │ │Play()      │
│- Stop()    │ │- Stop()    │ │- Stop()    │
│- Play()    │ │- Play()    │ │- Play()    │
└────────────┘ └────────────┘ └────────────┘
```

---

### 3️⃣ 구독 해제 흐름 (메모리 누수 방지)

```
[오브젝트 파괴]
    ⬇️
Observer의 OnDestroy() 호출
    ⬇️
┌─────────────────────────────────┐
│ ParticleSystemObserver.         │
│ OnDestroy()                     │
│   subjectToObserve.Clicked      │
│   -= OnThingHappened            │
│   → 구독 해제!                  │
└─────────────────────────────────┘
    ⬇️
✅ 메모리 누수 방지 완료!
```

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : 이벤트 정의 및 발행

**위치 :** Subject.cs:10-21

```csharp
public class Subject : MonoBehaviour
{
    // ✅ 핵심 1 : C# 이벤트 정의
    public event Action ThingHappened;

    // ✅ 핵심 2 : 이벤트 발행
    public void DoThing()
    {
        // null 체크와 함께 안전하게 호출
        ThingHappened?.Invoke();
    }
}
```

**이해 포인트 :**
- `event Action` : C#의 델리게이트 기반 이벤트
- `?.Invoke()` : null-conditional operator로 안전한 호출
- Observer가 하나도 없어도 에러 없음
- 여러 Observer가 있으면 모두에게 알림

---

### 📌 핵심 코드 2 : 이벤트 구독 및 해제

**위치 :** Observer.cs:18-34

```csharp
private void Awake()
{
    // ✅ 핵심 1 : 이벤트 구독 (+=)
    if (subjectToObserve != null)
    {
        subjectToObserve.ThingHappened += OnThingHappened;
    }
}

private void OnDestroy()
{
    // ✅ 핵심 2 : 이벤트 구독 해제 (-=)
    if (subjectToObserve != null)
    {
        subjectToObserve.ThingHappened -= OnThingHappened;
    }
}
```

**이해 포인트 :**
- `+=` : 이벤트에 메서드 등록 (구독)
- `-=` : 이벤트에서 메서드 제거 (구독 해제)
- **OnDestroy에서 반드시 구독 해제** : 메모리 누수 방지
- 구독 해제 안 하면 Subject가 파괴된 Observer 참조 유지

---

### 📌 핵심 코드 3 : 구체적인 Subject 구현

**위치 :** ButtonSubject.cs:10-46

```csharp
public class ButtonSubject : MonoBehaviour
{
    // ✅ 핵심 1 : 구체적인 이벤트 정의
    public event Action Clicked;

    private Collider m_Collider;

    // ✅ 핵심 2 : 외부에서 호출 가능한 이벤트 발행 메서드
    public void ClickButton()
    {
        Clicked?.Invoke();
    }

    // 내부 로직 : 클릭 감지
    void Update()
    {
        CheckCollider();
    }

    private void CheckCollider()
    {
        // 마우스 왼쪽 버튼이 콜라이더 위에서 눌렸는지 확인
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100f))
            {
                if (hitInfo.collider == this.m_Collider)
                {
                    ClickButton();  // 이벤트 발행!
                }
            }
        }
    }
}
```

**이해 포인트 :**
- Subject를 상속하지 않고 직접 구현 가능
- `public void ClickButton()` : 외부에서도 이벤트 발행 가능
- 입력 감지 로직과 이벤트 발행 분리 가능 (ClickCollider.cs 참고)

---

### 📌 핵심 코드 4 : Observer 구현 (파티클 예제)

**위치 :** ParticleSystemObserver.cs:9-39

```csharp
public class ParticleSystemObserver : MonoBehaviour
{
    [SerializeField] ButtonSubject  m_SubjectToObserve;
    [SerializeField] ParticleSystem m_ParticleSystem;

    private void Awake()
    {
        // ✅ 핵심 1 : 구독
        if (m_SubjectToObserve != null)
        {
            m_SubjectToObserve.Clicked += OnThingHappened;
        }
    }

    // ✅ 핵심 2 : 이벤트 핸들러
    private void OnThingHappened()
    {
        if (m_ParticleSystem != null)
        {
            m_ParticleSystem.Stop();   // 기존 파티클 정지
            m_ParticleSystem.Play();   // 새로 재생
        }
    }

    private void OnDestroy()
    {
        // ✅ 핵심 3 : 구독 해제
        if (m_SubjectToObserve != null)
        {
            m_SubjectToObserve.Clicked -= OnThingHappened;
        }
    }
}
```

**이해 포인트 :**
- Observer는 Subject를 참조만 함 (느슨한 결합)
- Subject는 Observer를 전혀 몰라도 됨
- 이벤트 핸들러 메서드 이름은 자유롭게 지정 가능
- Stop() 후 Play()로 파티클 재시작

---

### 📌 핵심 코드 5 : 관심사 분리 (ClickCollider)

**위치 :** ClickCollider.cs:7-41

```csharp
[RequireComponent(typeof(Collider), typeof(ButtonSubject))]
public class ClickCollider : MonoBehaviour
{
    private ButtonSubject m_ButtonSubject;
    private Collider      m_Collider;

    void Start()
    {
        m_ButtonSubject = GetComponent<ButtonSubject>();
        m_Collider      = GetComponent<Collider>();
    }

    void Update()
    {
        CheckCollider();
    }

    private void CheckCollider()
    {
        // ✅ 핵심 : 입력 감지만 담당
        // 마우스 왼쪽 버튼이 콜라이더 위에서 눌렸는지 확인
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, 100f))
            {
                if (hitInfo.collider == this.m_Collider)
                {
                    // Subject에게 위임
                    m_ButtonSubject.ClickButton();
                }
            }
        }
    }
}
```

**이해 포인트 :**
- **관심사의 분리** : 입력 감지 vs 이벤트 발행
- ClickCollider : 입력만 감지
- ButtonSubject : 이벤트만 발행
- 단일 책임 원칙(SRP) 준수

---

## ⚖️ 장단점

### ✅ 장점

**1. 느슨한 결합 (Loose Coupling)**
- Subject는 Observer의 구체적인 타입을 몰라도 됨
- 인터페이스(이벤트)를 통한 통신
- 독립적인 개발 및 테스트 가능

**2. 개방-폐쇄 원칙 (OCP) 준수**
- 새로운 Observer 추가 시 기존 코드 수정 불필요
- Subject 코드 변경 없이 확장 가능

**3. 다대다 관계 지원**
- 하나의 Subject를 여러 Observer가 구독 가능
- 하나의 Observer가 여러 Subject를 구독 가능

**4. 동적 구독/해제**
- 런타임에 Observer를 동적으로 추가/제거 가능
- 유연한 이벤트 시스템 구축

**5. 브로드캐스트 통신**
- 하나의 이벤트로 여러 Observer에게 동시 알림
- 효율적인 일대다 통신

**6. 재사용성**
- Subject와 Observer를 독립적으로 재사용 가능
- 다른 프로젝트에 쉽게 적용

### ❌ 단점

**1. 예측 불가능한 실행 순서**
- Observer들의 호출 순서가 보장되지 않음
- 순서에 의존하는 로직은 문제 발생 가능

**2. 메모리 누수 위험**
- 구독 해제를 잊으면 메모리 누수 발생
- OnDestroy에서 반드시 구독 해제 필요

**3. 디버깅 어려움**
- 이벤트 체인이 복잡해지면 추적 어려움
- 누가 누구를 구독하는지 파악 어려움

**4. 성능 오버헤드**
- 많은 Observer가 있으면 성능 저하 가능
- 이벤트 발생 시마다 모든 Observer 호출

**5. 순환 참조 위험**
- Subject와 Observer 간 순환 참조 발생 가능
- 주의 깊은 설계 필요

**6. 과도한 사용 시 복잡도 증가**
- 모든 곳에 이벤트를 쓰면 코드 흐름 파악 어려움
- 적절한 사용이 중요

---

## 🎮 실제 사용 사례

### 1️⃣ UI 시스템

**버튼 클릭 이벤트**
```csharp
public class UIButton : MonoBehaviour
{
    public event Action OnClicked;

    public void Click()
    {
        OnClicked?.Invoke();
    }
}

// 다양한 Observer 구독
public class SoundManager : MonoBehaviour
{
    void Start()
    {
        uiButton.OnClicked += PlayClickSound;
    }

    void PlayClickSound()
    {
        audioSource.Play();
    }
}

public class UIAnimator : MonoBehaviour
{
    void Start()
    {
        uiButton.OnClicked += PlayButtonAnimation;
    }

    void PlayButtonAnimation()
    {
        animator.SetTrigger("Click");
    }
}
```

---

### 2️⃣ 게임 이벤트 시스템

**플레이어 상태 변화 알림**
```csharp
public class Player : MonoBehaviour
{
    public event Action OnDeath;
    public event Action OnLevelUp;
    public event Action<int> OnScoreChanged;

    private int score;

    public void AddScore(int points)
    {
        score += points;
        OnScoreChanged?.Invoke(score);  // 점수 변화 알림
    }

    public void Die()
    {
        OnDeath?.Invoke();  // 사망 알림
    }
}

// UI Observer
public class ScoreUI : MonoBehaviour
{
    void Start()
    {
        player.OnScoreChanged += UpdateScoreText;
    }

    void UpdateScoreText(int newScore)
    {
        scoreText.text = $"Score : {newScore}";
    }
}

// Game Manager Observer
public class GameManager : MonoBehaviour
{
    void Start()
    {
        player.OnDeath += HandlePlayerDeath;
    }

    void HandlePlayerDeath()
    {
        ShowGameOverScreen();
    }
}
```

---

### 3️⃣ 체력 시스템

**체력 변화 알림**
```csharp
public class Health : MonoBehaviour
{
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;

    private float currentHealth;
    private float maxHealth = 100f;

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnHealthChanged?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}

// Health Bar Observer
public class HealthBarUI : MonoBehaviour
{
    void Start()
    {
        health.OnHealthChanged += UpdateHealthBar;
    }

    void UpdateHealthBar(float currentHealth)
    {
        healthBar.fillAmount = currentHealth / 100f;
    }
}

// Damage Effect Observer
public class DamageEffect : MonoBehaviour
{
    void Start()
    {
        health.OnHealthChanged += ShowDamageEffect;
    }

    void ShowDamageEffect(float currentHealth)
    {
        // 빨간색 화면 효과
        screenOverlay.color = Color.red;
    }
}
```

---

### 4️⃣ 업적/퀘스트 시스템

**게임 이벤트 감지**
```csharp
public class EnemyManager : MonoBehaviour
{
    public event Action<Enemy> OnEnemyKilled;

    public void KillEnemy(Enemy enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
        Destroy(enemy.gameObject);
    }
}

// Achievement Observer
public class AchievementManager : MonoBehaviour
{
    private int totalKills = 0;

    void Start()
    {
        enemyManager.OnEnemyKilled += TrackKills;
    }

    void TrackKills(Enemy enemy)
    {
        totalKills++;
        if (totalKills >= 100)
        {
            UnlockAchievement("Monster Hunter");
        }
    }
}

// Quest Observer
public class QuestManager : MonoBehaviour
{
    void Start()
    {
        enemyManager.OnEnemyKilled += CheckQuestProgress;
    }

    void CheckQuestProgress(Enemy enemy)
    {
        if (enemy.type == EnemyType.Boss)
        {
            CompleteQuest("Defeat the Boss");
        }
    }
}
```

---

### 5️⃣ 세이브 시스템

**데이터 변화 자동 저장**
```csharp
public class PlayerData : MonoBehaviour
{
    public event Action OnDataChanged;

    private int level;
    private int gold;

    public void SetLevel(int newLevel)
    {
        level = newLevel;
        OnDataChanged?.Invoke();  // 데이터 변화 알림
    }

    public void AddGold(int amount)
    {
        gold += amount;
        OnDataChanged?.Invoke();
    }
}

// Auto Save Observer
public class AutoSaveManager : MonoBehaviour
{
    void Start()
    {
        playerData.OnDataChanged += SaveGame;
    }

    void SaveGame()
    {
        // 자동 저장
        SaveSystem.Save(playerData);
    }
}
```

---

### 6️⃣ 멀티플레이어 동기화

**네트워크 이벤트**
```csharp
public class NetworkPlayer : MonoBehaviour
{
    public event Action<Vector3> OnPositionChanged;
    public event Action<string> OnActionPerformed;

    void Update()
    {
        if (transform.hasChanged)
        {
            OnPositionChanged?.Invoke(transform.position);
            transform.hasChanged = false;
        }
    }
}

// Network Sync Observer
public class NetworkSync : MonoBehaviour
{
    void Start()
    {
        networkPlayer.OnPositionChanged += SyncPosition;
        networkPlayer.OnActionPerformed += SyncAction;
    }

    void SyncPosition(Vector3 position)
    {
        // 서버에 위치 전송
        SendToServer(position);
    }
}
```

---

## 🎓 학습 정리

### 핵심 개념

**옵저버 패턴의 본질 :**
```
Subject(발행자)와 Observer(구독자)를 분리하여
느슨한 결합과 유연한 확장을 가능하게 하는 패턴
```

### 1 : N 관계

```
      Subject (1)
         │
    ┌────┼────┬────┐
    │    │    │    │
    ▼    ▼    ▼    ▼
 Obs1 Obs2 Obs3 Obs4 (N)

하나의 이벤트 → 여러 Observer에게 알림
```

### 주요 구성 요소

```
Subject (발행자)
  ↓
  이벤트 정의 : event Action
  이벤트 발행 : Invoke()

Observer (구독자)
  ↓
  이벤트 구독 : +=
  이벤트 해제 : -=
  핸들러 구현 : OnEvent()
```

### 언제 사용해야 할까?

**✅ 옵저버 패턴을 사용하면 좋은 경우 :**
- 하나의 이벤트에 **여러 객체가 반응**해야 할 때
- Subject와 Observer 간 **느슨한 결합**이 필요할 때
- **동적으로 구독/해제**가 필요할 때
- UI 업데이트, 이벤트 시스템, 알림 시스템 등

**❌ 옵저버 패턴을 피해야 하는 경우 :**
- Observer가 하나뿐인 경우 (직접 호출이 더 간단)
- 실행 순서가 중요한 경우
- 성능이 매우 중요한 경우 (많은 Observer)

### C# 이벤트 vs UnityEvent

**C# 이벤트 :**
```csharp
public event Action OnClick;  // 코드로만 구독 가능
OnClick?.Invoke();
```

**UnityEvent :**
```csharp
public UnityEvent OnClick;  // Inspector에서 구독 가능
OnClick?.Invoke();
```

**선택 기준 :**
- **C# 이벤트** : 코드 간 통신, 성능 중요, 타입 안정성
- **UnityEvent** : Inspector 연결 필요, 디자이너 친화적

### 메모리 누수 방지 필수!

```csharp
private void OnDestroy()
{
    // ✅ 반드시 구독 해제!
    subject.OnEvent -= Handler;
}
```

**왜 필요한가?**
- Observer가 파괴되어도 Subject가 참조 유지
- 메모리 누수 및 null 참조 오류 발생
- **항상 OnDestroy에서 구독 해제!**

### 마무리

옵저버 패턴은 **Unity에서 가장 많이 사용되는 패턴** 중 하나입니다.

**기억할 점 :**
- ✅ 느슨한 결합으로 유연한 시스템 구축
- ✅ 이벤트 기반 프로그래밍의 핵심
- ⚠️ 구독 해제를 잊지 말 것
- 🎯 적절한 상황에서 사용

---

**작성일 :** 2025.12.18
**참고 자료 :** Unity Korea - Level Up Your Code with Design Patterns and SOLID
