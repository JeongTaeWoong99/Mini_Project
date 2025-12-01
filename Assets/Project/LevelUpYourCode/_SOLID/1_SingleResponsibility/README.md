# 🎯 Single Responsibility Principle (단일 책임 원칙)

## 📋 목차
- [풀어서 설명](#-풀어서-설명)
- [원칙 개요](#-원칙-개요)
- [왜 SRP가 필요한가?](#-왜-srp가-필요한가)
- [핵심 개념](#-핵심-개념)
- [코드 구조](#-코드-구조)
- [Before & After 비교](#-before--after-비교)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 적용 사례](#-실제-적용-사례)
- [학습 정리](#-학습-정리)

---

## 💡 풀어서 설명

### 한 문장으로 이해하기

```
하나의 클래스에 과도한 책임을 주면 안 된다.
한 클래스는 딱 하나의 책임만 가져야 한다.
```

### 실생활 비유

**❌ 나쁜 예 : 한 사람이 모든 일을 다 함**
```
레스토랑에서 한 사람이...
- 요리사 (요리)
- 웨이터 (서빙)
- 계산원 (계산)
- 청소부 (청소)
...모든 역할을 담당

문제점 :
- 요리 방법이 바뀌면?   → 이 사람이 바뀌어야 함
- 계산 시스템이 바뀌면? → 이 사람이 바뀌어야 함
- 청소 방식이 바뀌면?   → 이 사람이 바뀌어야 함
→ 4가지 이유로 변경 필요! 너무 많음!
```

**✅ 좋은 예 : 각자 하나의 역할만**
```
- 요리사 : 요리만
- 웨이터 : 서빙만
- 계산원 : 계산만
- 청소부 : 청소만

장점 :
- 요리 방법이 바뀌면?   → 요리사만 바뀜
- 계산 시스템이 바뀌면? → 계산원만 바뀜
- 청소 방식이 바뀌면?   → 청소부만 바뀜
→ 각자 1가지 이유로만 변경! 명확함!
```

### 판단 기준

**"이 클래스가 변경되는 이유가 몇 가지인가?"**

- ✅ **1가지** → SRP 준수! 잘 설계됨!
- ❌ **2가지 이상** → SRP 위반! 분리 필요!

---

## 🎯 원칙 개요

**Single Responsibility Principle (SRP)** 은 **SOLID 원칙**의 첫 번째 원칙으로, 클래스는 단 하나의 책임만 가져야 한다는 원칙입니다.

### 📌 핵심 개념

```
하나의 클래스는 하나의 이유로만 변경되어야 한다!
```

**잘못된 설계 :**
```csharp
// ❌ PlayerController : 입력, 이동, 사운드, 이펙트 모두 처리
public class PlayerController
{
    void Update()     { /* 입력 처리 */ }
    void Move()       { /* 이동 처리 */ }
    void PlaySound()  { /* 사운드 재생 */ }
    void PlayEffect() { /* 이펙트 재생 */ }
}
// 문제 : 입력 시스템 변경, 이동 로직 수정, 사운드 교체, 이펙트 변경
//        → 4가지 이유로 클래스가 변경될 수 있음!
```

**SRP 적용 :**
```csharp
// ✅ 각 클래스가 하나의 책임만 담당
public class PlayerInput { }    // 입력 처리만
public class PlayerMovement { } // 이동 처리만
public class PlayerAudio { }    // 사운드만
public class PlayerFX { }       // 이펙트만
public class Player { }         // 컴포넌트 조합만
```

---

## 🤔 왜 SRP가 필요한가?

### 문제 상황

게임에서 플레이어 기능을 하나의 클래스에 모두 구현한 경우 :

```csharp
public class UnrefactoredPlayer : MonoBehaviour
{
    // 이동 관련
    private float moveSpeed;
    private float acceleration;

    // 입력 관련
    private KeyCode forwardKey;
    private KeyCode backwardKey;

    // 오디오 관련
    private AudioClip[] bounceClips;
    private AudioSource audioSource;

    // 이펙트 관련
    private ParticleSystem particleSystem;

    void Update()
    {
        HandleInput();      // 입력 처리
        Move();            // 이동 처리
    }

    void OnCollision()
    {
        PlaySound();       // 사운드 재생
        PlayEffect();      // 이펙트 재생
    }
}
```

**이 코드의 문제점 :**

❌ **유지보수 어려움**
   - 한 기능을 수정하려면 거대한 클래스를 열어야 함
   - 어떤 변수가 어떤 기능에 속하는지 파악 어려움

❌ **테스트 어려움**
   - 이동만 테스트하고 싶어도 전체 클래스를 로드해야 함
   - 의존성이 복잡하게 얽혀있음

❌ **재사용 불가능**
   - PlayerMovement만 따로 쓰고 싶어도 불가능
   - 다른 프로젝트에 이식 어려움

❌ **협업 충돌**
   - 팀원 A가 입력 수정, 팀원 B가 이펙트 수정
   - 같은 파일을 동시 수정 → Git 충돌!

❌ **변경의 파급 효과**
   - 이동 로직 수정이 사운드에 영향을 줄 수 있음
   - 한 부분의 버그가 다른 부분에 영향

### SRP의 해결책

✅ **명확한 책임 분리**
   - 각 클래스가 하나의 역할만 담당

✅ **독립적인 테스트**
   - 각 컴포넌트를 독립적으로 테스트 가능

✅ **높은 재사용성**
   - 필요한 컴포넌트만 다른 프로젝트에 이식

✅ **협업 효율 향상**
   - 팀원들이 서로 다른 파일 작업 가능

✅ **변경 영향 최소화**
   - 한 클래스 수정이 다른 클래스에 영향 없음

---

## 🏗️ 핵심 개념

SRP를 이해하기 위한 핵심 질문 :

### ❓ "이 클래스가 변경되어야 하는 이유가 몇 가지인가?"

**나쁜 설계 (여러 이유) :**
```csharp
public class UnrefactoredPlayer
{
    // 변경 이유 1 : 입력 시스템 변경 (키보드 → 게임패드)
    void HandleInput() { }

    // 변경 이유 2 : 이동 알고리즘 변경
    void Move() { }

    // 변경 이유 3 : 사운드 시스템 변경
    void PlaySound() { }

    // 변경 이유 4 : 이펙트 시스템 변경
    void PlayEffect() { }
}
// ❌ 4가지 이유로 변경될 수 있음!
```

**좋은 설계 (하나의 이유) :**
```csharp
// 변경 이유 1개 : 입력 처리 방식 변경
public class PlayerInput { }

// 변경 이유 1개 : 이동 로직 변경
public class PlayerMovement { }

// 변경 이유 1개 : 오디오 재생 방식 변경
public class PlayerAudio { }

// 변경 이유 1개 : 이펙트 재생 방식 변경
public class PlayerFX { }
```

---

## 📊 코드 구조

### 폴더 구조

```
1_SingleResponsibility/
├── Scripts/
│   ├── Player.cs                    ← ✅ SRP 적용 (조합 담당)
│   ├── PlayerInput.cs               ← 입력 처리만
│   ├── PlayerMovement.cs            ← 이동 처리만
│   ├── PlayerAudio.cs               ← 오디오 재생만
│   ├── PlayerFX.cs                  ← 이펙트 재생만
│   ├── ObjectToggle.cs              ← 데모용 토글
│   │
│   └── Unrefactored/
│       └── UnrefactoredPlayer.cs    ← ❌ SRP 미적용 (모든 기능 포함)
│
└── README.md                         ← 📍 현재 문서
```

### 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────┐
│                    Player (조합)                        │
│  - 여러 컴포넌트를 조합하여 플레이어 기능 구현          │
├─────────────────────────────────────────────────────────┤
│  - PlayerInput    m_PlayerInput                         │
│  - PlayerMovement m_PlayerMovement                      │
│  - PlayerAudio    m_PlayerAudio                         │
│  - PlayerFX       m_PlayerFX                            │
├─────────────────────────────────────────────────────────┤
│  + Awake()                                              │
│  + OnControllerColliderHit()                            │
│  + LateUpdate()                                         │
└─────────────────────────────────────────────────────────┘
         │                │              │           │
         │ uses           │ uses         │ uses     │ uses
         ▼                ▼              ▼           ▼
┌──────────────┐  ┌──────────────┐  ┌──────────┐  ┌──────────┐
│ PlayerInput  │  │PlayerMovement│  │PlayerAudio│ │PlayerFX  │
├──────────────┤  ├──────────────┤  ├──────────┤  ├──────────┤
│ - m_XInput   │  │ - m_MoveSpeed│  │- m_Audio │  │- m_Part  │
│ - m_ZInput   │  │ - m_Accel    │  │  Source  │  │  icle    │
│              │  │ - m_Decel    │  │- m_Clips │  │  System  │
├──────────────┤  ├──────────────┤  ├──────────┤  ├──────────┤
│+ HandleInput │  │+ Move()      │  │+ Play    │  │+ Play    │
│+ InputVector │  │              │  │  Random  │  │  Effect  │
│              │  │              │  │  Clip()  │  │          │
└──────────────┘  └──────────────┘  └──────────┘  └──────────┘
책임 : 입력 처리   책임 : 이동 처리   책임 : 사운드   책임 : 이펙트


VS


┌─────────────────────────────────────────────────────────┐
│          UnrefactoredPlayer (모든 책임 포함)            │
│  ❌ 입력, 이동, 사운드, 이펙트 모두 처리                │
├─────────────────────────────────────────────────────────┤
│  - moveSpeed, acceleration, deceleration (이동)         │
│  - forwardKey, backwardKey, leftKey, rightKey (입력)    │
│  - bounceClips[], audioSource (오디오)                  │
│  - particleSystem (이펙트)                              │
├─────────────────────────────────────────────────────────┤
│  + HandleInput()        ← 입력 처리                     │
│  + Move()               ← 이동 처리                     │
│  + PlayRandomAudioClip() ← 사운드 재생                  │
│  + PlayEffect()         ← 이펙트 재생                   │
└─────────────────────────────────────────────────────────┘
       ⚠️ 4가지 책임을 하나의 클래스가 담당!
```

---

## 🔄 Before & After 비교

### ❌ Before : SRP 미적용 (UnrefactoredPlayer.cs)

```csharp
public class UnrefactoredPlayer : MonoBehaviour
{
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 이동 관련 변수들
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    [SerializeField] private float moveSpeed    = 5f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 5f;
    private float               currentSpeed = 0f;
    private CharacterController charController;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 입력 관련 변수들
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    [SerializeField] private KeyCode forwardKey  = KeyCode.W;
    [SerializeField] private KeyCode backwardKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey     = KeyCode.A;
    [SerializeField] private KeyCode rightKey    = KeyCode.D;
    private Vector3 inputVector;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 오디오 관련 변수들
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    [SerializeField] private AudioClip[] bounceClips;
    [SerializeField] private float       audioCooldownTime = 2f;
    private AudioSource audioSource;
    private float       lastAudioPlayedTime;

    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    // 이펙트 관련 변수들
    // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
    [SerializeField] private ParticleSystem m_ParticleSystem;
    private const float effectCooldown   = 1f;
    private float       timeToNextEffect = -1f;

    void Update()
    {
        HandleInput();  // 입력 처리
        Move(inputVector); // 이동 처리
    }

    private void HandleInput() { /* 입력 처리 로직 */ }
    private void Move(Vector3 input) { /* 이동 로직 */ }
    public void PlayRandomAudioClip() { /* 사운드 재생 */ }
    public void PlayEffect() { /* 이펙트 재생 */ }
}
```

**문제점 :**
- 🔴 4가지 책임이 하나의 클래스에 혼재
- 🔴 코드가 길고 복잡함 (약 145줄)
- 🔴 어떤 변수가 어떤 기능에 속하는지 불명확
- 🔴 한 기능을 수정하면 다른 기능에 영향 가능성

---

### ✅ After : SRP 적용

#### 1️⃣ PlayerInput.cs (입력 처리만)

```csharp
public class PlayerInput : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private KeyCode m_ForwardKey  = KeyCode.W;
    [SerializeField] private KeyCode m_BackwardKey = KeyCode.S;
    [SerializeField] private KeyCode m_LeftKey     = KeyCode.A;
    [SerializeField] private KeyCode m_RightKey    = KeyCode.D;

    private Vector3 m_InputVector;
    private float   m_XInput;
    private float   m_ZInput;

    public Vector3 InputVector => m_InputVector;

    private void Update()
    {
        HandleInput();
    }

    public void HandleInput()
    {
        m_XInput = 0;
        m_ZInput = 0;

        if (Input.GetKey(m_ForwardKey))  m_ZInput++;
        if (Input.GetKey(m_BackwardKey)) m_ZInput--;
        if (Input.GetKey(m_LeftKey))     m_XInput--;
        if (Input.GetKey(m_RightKey))    m_XInput++;

        m_InputVector = new Vector3(m_XInput, 0, m_ZInput);
    }
}
```

**책임 : 입력 처리만 담당**
- 키보드 입력 감지
- 입력 벡터 계산
- 다른 컴포넌트에 입력값 제공

---

#### 2️⃣ PlayerMovement.cs (이동 처리만)

```csharp
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float m_MoveSpeed    = 5f;
    [SerializeField] private float m_Acceleration = 10f;
    [SerializeField] private float m_Deceleration = 5f;

    private float               m_CurrentSpeed;
    private CharacterController m_CharController;
    private float               m_InitialYPosition;

    private void Awake()
    {
        m_CharController = GetComponent<CharacterController>();
    }

    void Start()
    {
        m_InitialYPosition = transform.position.y;
    }

    public void Move(Vector3 inputVector)
    {
        if (inputVector == Vector3.zero)
        {
            // 입력이 없을 때 감속
            m_CurrentSpeed -= m_Deceleration * Time.deltaTime;
            m_CurrentSpeed  = Mathf.Max(m_CurrentSpeed, 0);
        }
        else
        {
            // 입력이 있을 때 가속
            m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, m_MoveSpeed,
                                        Time.deltaTime * m_Acceleration);
        }

        Vector3 movement = m_CurrentSpeed * Time.deltaTime *
                          inputVector.normalized;
        m_CharController.Move(movement);

        // Y 위치 고정
        transform.position = new Vector3(transform.position.x,
                                        m_InitialYPosition,
                                        transform.position.z);
    }
}
```

**책임 : 이동 처리만 담당**
- 이동 속도 계산
- 가속/감속 처리
- CharacterController를 통한 이동

---

#### 3️⃣ PlayerAudio.cs (사운드 재생만)

```csharp
public class PlayerAudio : MonoBehaviour
{
    [SerializeField] private float       m_CooldownTime = 2f;
    [SerializeField] private AudioClip[] m_BounceClips;

    private float       m_LastTimePlayed;
    private AudioSource m_AudioSource;

    void Awake()
    {
        m_AudioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        m_LastTimePlayed = -m_CooldownTime;
    }

    public void PlayRandomClip()
    {
        float timeToNextPlay = m_CooldownTime + m_LastTimePlayed;

        if (Time.time > timeToNextPlay)
        {
            m_LastTimePlayed   = Time.time;
            m_AudioSource.clip = GetRandomClip();
            m_AudioSource.Play();
        }
    }

    private AudioClip GetRandomClip()
    {
        int randomIndex = UnityEngine.Random.Range(0, m_BounceClips.Length);
        return m_BounceClips[randomIndex];
    }
}
```

**책임 : 오디오 재생만 담당**
- 사운드 재생
- 쿨다운 관리
- 랜덤 클립 선택

---

#### 4️⃣ PlayerFX.cs (이펙트 재생만)

```csharp
public class PlayerFX : MonoBehaviour
{
    [SerializeField] private ParticleSystem m_ParticleSystem;

    const float k_Cooldown = 1f;
    private float m_TimeToNextPlay = -1f;

    public void PlayEffect()
    {
        if (Time.time < m_TimeToNextPlay)
            return;

        if (m_ParticleSystem != null)
        {
            m_ParticleSystem.Stop();
            m_ParticleSystem.Play();

            m_TimeToNextPlay = Time.time + k_Cooldown;
        }
    }
}
```

**책임 : 이펙트 재생만 담당**
- 파티클 재생
- 쿨다운 관리

---

#### 5️⃣ Player.cs (컴포넌트 조합만)

```csharp
[RequireComponent(typeof(PlayerInput), typeof(PlayerAudio), typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [SerializeField] private LayerMask m_ObstacleLayer;

    private PlayerInput    m_PlayerInput;
    private PlayerMovement m_PlayerMovement;
    private PlayerAudio    m_PlayerAudio;
    private PlayerFX       m_PlayerFX;

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        m_PlayerInput    = GetComponent<PlayerInput>();
        m_PlayerMovement = GetComponent<PlayerMovement>();
        m_PlayerAudio    = GetComponent<PlayerAudio>();
        m_PlayerFX       = GetComponent<PlayerFX>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (m_ObstacleLayer.ContainsLayer(hit.gameObject))
        {
            m_PlayerAudio.PlayRandomClip();

            if (m_PlayerFX != null)
                m_PlayerFX.PlayEffect();
        }
    }

    private void LateUpdate()
    {
        Vector3 inputVector = m_PlayerInput.InputVector;
        m_PlayerMovement.Move(inputVector);
    }
}
```

**책임 : 컴포넌트 조합만 담당**
- 각 컴포넌트 초기화
- 컴포넌트 간 통신 중개
- 충돌 이벤트 처리

---

### 📊 개선 효과

| 항목 | Before (UnrefactoredPlayer) | After (SRP 적용) |
|------|----------------------------|------------------|
| **파일 수** | 1개 (145줄) | 5개 (각 30~60줄) |
| **책임 분리** | ❌ 4가지 책임 혼재 | ✅ 각 1가지 책임 |
| **가독성** | 🔴 낮음 | 🟢 높음 |
| **테스트** | 🔴 어려움 | 🟢 쉬움 |
| **재사용** | 🔴 불가능 | 🟢 가능 |
| **유지보수** | 🔴 어려움 | 🟢 쉬움 |
| **협업** | 🔴 충돌 위험 | 🟢 독립 작업 |

---

## 💻 주요 코드 분석

### 📌 핵심 1 : 입력과 이동의 분리

**Before (결합) :**
```csharp
void Update()
{
    HandleInput();      // 입력 처리
    Move(inputVector);  // 바로 이동
}
// ❌ 입력과 이동이 강하게 결합
```

**After (분리) :**
```csharp
// PlayerInput.cs
void Update()
{
    HandleInput();  // 입력만 처리
}

// Player.cs
void LateUpdate()
{
    Vector3 input = m_PlayerInput.InputVector;  // 입력 가져오기
    m_PlayerMovement.Move(input);               // 이동 처리
}
// ✅ 입력과 이동이 독립적
// ✅ 입력 방식 변경해도 이동 코드는 수정 불필요
```

---

### 📌 핵심 2 : 충돌 이벤트 처리의 분리

**Before (직접 처리) :**
```csharp
void OnControllerColliderHit(ControllerColliderHit hit)
{
    // 직접 사운드 재생
    audioSource.clip = bounceClips[Random.Range(0, bounceClips.Length)];
    audioSource.Play();

    // 직접 이펙트 재생
    particleSystem.Play();
}
// ❌ Player가 사운드와 이펙트 구현을 직접 알고 있음
```

**After (위임) :**
```csharp
void OnControllerColliderHit(ControllerColliderHit hit)
{
    if (m_ObstacleLayer.ContainsLayer(hit.gameObject))
    {
        m_PlayerAudio.PlayRandomClip();  // 오디오에게 위임

        if (m_PlayerFX != null)
            m_PlayerFX.PlayEffect();     // FX에게 위임
    }
}
// ✅ Player는 "재생해라"만 요청
// ✅ 구체적인 재생 방식은 각 컴포넌트가 담당
```

---

### 📌 핵심 3 : RequireComponent 활용

```csharp
[RequireComponent(typeof(PlayerInput),
                  typeof(PlayerAudio),
                  typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    // ...
}
```

**효과 :**
- ✅ Player 컴포넌트 추가 시 필수 컴포넌트 자동 추가
- ✅ 누락 방지
- ✅ 의존성 명시

---

## ⚖️ 장단점

### ✅ 장점

**1. 가독성 향상**
- 각 클래스가 짧고 명확함
- 코드 이해가 쉬움
- 파일 이름만 봐도 역할 파악 가능

**2. 유지보수 용이**
- 수정할 코드의 위치가 명확
- 한 기능 수정이 다른 기능에 영향 없음
- 버그 수정이 쉬움

**3. 테스트 용이**
- 각 컴포넌트를 독립적으로 테스트
- 단위 테스트 작성 쉬움
- Mock 객체 활용 가능

**4. 재사용성 증가**
- PlayerMovement만 다른 프로젝트에 이식 가능
- 컴포넌트 조합으로 다양한 객체 생성

**5. 협업 효율 향상**
- 팀원들이 다른 파일 작업 가능
- Git 충돌 감소
- 코드 리뷰 용이

**6. 확장 용이**
- 새로운 기능 추가 시 새 컴포넌트만 작성
- 기존 코드 수정 최소화

### ❌ 단점

**1. 파일 수 증가**
- 클래스가 많아짐
- 프로젝트 구조 복잡해 보일 수 있음

**2. 초기 설계 시간**
- 책임 분리에 대한 고민 필요
- 오버엔지니어링 위험

**3. 간단한 기능에는 과도함**
- 매우 단순한 스크립트는 분리 불필요
- 트레이드오프 고려 필요

**4. 컴포넌트 간 통신 오버헤드**
- GetComponent 호출 증가
- 약간의 성능 오버헤드 (미미함)

---

## 🎮 실제 적용 사례

### 1️⃣ 게임 개발

**적 AI 시스템**
```csharp
// ✅ SRP 적용
public class EnemyAI { }          // AI 로직만
public class EnemyMovement { }    // 이동만
public class EnemyAnimation { }   // 애니메이션만
public class EnemyHealth { }      // 체력 관리만
public class EnemyDetection { }   // 탐지만
public class Enemy { }            // 조합
```

**UI 시스템**
```csharp
// ✅ SRP 적용
public class HealthBarUI { }      // 체력바 표시만
public class ScoreUI { }          // 점수 표시만
public class MenuUI { }           // 메뉴만
public class UIManager { }        // 조합 및 관리
```

### 2️⃣ 유니티 에디터 확장

```csharp
// ✅ SRP 적용
public class DataValidator { }    // 데이터 검증만
public class DataSerializer { }   // 직렬화만
public class DataImporter { }     // 임포트만
```

### 3️⃣ 네트워크 시스템

```csharp
// ✅ SRP 적용
public class NetworkSender { }    // 전송만
public class NetworkReceiver { }  // 수신만
public class NetworkSerializer { } // 직렬화만
public class NetworkManager { }   // 조합
```

---

## 📝 학습 정리

### 핵심 요약

1. **하나의 클래스는 하나의 책임만 가져야 한다**
   - "변경의 이유"가 하나여야 함

2. **책임 분리의 기준**
   - "이 클래스가 변경되는 이유가 몇 가지인가?"
   - 여러 이유가 있다면 분리 필요

3. **실전 적용 방법**
   - 큰 클래스를 작은 컴포넌트로 분리
   - 각 컴포넌트는 명확한 하나의 역할
   - 조합 클래스로 통합

4. **장점**
   - 가독성, 유지보수성, 테스트 용이성, 재사용성 향상
   - 협업 효율 증가

5. **주의사항**
   - 과도한 분리는 오버엔지니어링
   - 적절한 수준 판단 필요

### Before vs After

| | Before | After |
|---|--------|-------|
| **구조** | 1개 거대 클래스 | 5개 작은 클래스 |
| **변경 이유** | 4가지 | 각 1가지 |
| **코드 길이** | 145줄 | 30~60줄 |
| **가독성** | 낮음 | 높음 |
| **유지보수** | 어려움 | 쉬움 |
| **테스트** | 어려움 | 쉬움 |
| **재사용** | 불가능 | 가능 |

### 실무 적용 팁

✅ **이런 경우 SRP 적용 고려**
- 클래스가 100줄 이상일 때
- 여러 이유로 수정이 필요할 때
- 팀원과 동시 작업이 필요할 때
- 재사용 가능성이 있을 때

❌ **이런 경우 무리하게 적용 X**
- 매우 단순한 스크립트 (10~20줄)
- 한 번만 사용하는 일회성 코드
- 프로토타입 단계

### 다른 SOLID 원칙과의 관계

**OCP (Open-Closed Principle)와의 관계 :**
- SRP를 지키면 OCP도 지키기 쉬움
- 각 클래스가 하나의 책임만 가지므로 확장이 용이

**LSP (Liskov Substitution Principle)와의 관계 :**
- SRP를 지키면 LSP 위반 가능성이 낮아짐
- 명확한 책임은 일관된 동작을 보장

**ISP (Interface Segregation Principle)와의 관계 :**
- SRP는 클래스 레벨, ISP는 인터페이스 레벨의 책임 분리
- 둘 다 "하나의 책임"이라는 공통 철학