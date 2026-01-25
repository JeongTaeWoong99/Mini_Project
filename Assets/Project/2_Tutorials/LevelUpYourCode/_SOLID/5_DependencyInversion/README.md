# 🔄 Dependency Inversion Principle (의존 역전 원칙)

## 📋 목차
- [풀어서 설명](#-풀어서-설명)
- [원칙 개요](#-원칙-개요)
- [왜 DIP가 필요한가?](#-왜-dip가-필요한가)
- [핵심 개념](#-핵심-개념)
- [코드 구조](#-코드-구조)
- [Before & After 비교](#-before--after-비교)
- [확장 시나리오](#-확장-시나리오)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 적용 사례](#-실제-적용-사례)
- [학습 정리](#-학습-정리)

---

## 💡 풀어서 설명

### 한 문장으로 이해하기

```
높은 수준의 모듈은 낮은 수준의 모듈에 직접 의존하지 말고,
둘 다 추상화(인터페이스)에 의존해야 한다.
```

### 실생활 비유

**❌ 나쁜 예 : 콘센트마다 전용 플러그가 필요**
```
TV용 특수 콘센트  → TV만 연결 가능
냉장고용 특수 콘센트 → 냉장고만 연결 가능
에어컨용 특수 콘센트 → 에어컨만 연결 가능

문제점 :
→ 새 제품마다 새 콘센트 설치 필요!
→ 유연성 제로!
→ 집 전체를 뜯어고쳐야 함!
```

**✅ 좋은 예 : 표준 콘센트와 플러그**
```
표준 콘센트 (인터페이스)
   ↑
   └─ TV, 냉장고, 에어컨, 선풍기... 모두 연결 가능!

장점 :
→ 어떤 제품이든 연결 가능!
→ 새 제품이 나와도 콘센트는 그대로!
→ 표준 규격만 맞으면 됨!
```

### 쉽게 이해하기

**"중간에 인터페이스를 둔다"**

- 상위 모듈(Switch)이 하위 모듈(Door, Trap)을 직접 알지 않음!!!
- ISwitchable 인터페이스를 통해서만 소통
- 새로운 하위 모듈(NPC)을 추가해도 상위 모듈은 수정 불필요

### 판단 기준

**"구체적인 클래스에 직접 의존하는가?"**

- ✅ **인터페이스에 의존** → DIP 준수! 잘 설계됨!
- ❌ **구체 클래스에 의존** → DIP 위반! 설계 수정 필요!

```csharp
// ❌ DIP 위반 징후
public class Switch
{
    public Door door;         // 구체 클래스에 직접 의존!
    public Trap trap;         // 구체 클래스에 직접 의존!

    public void Toggle()
    {
        door.Open();          // 구체적인 메서드 호출
        trap.Enable();        // 구체적인 메서드 호출
    }
}

// ✅ DIP 준수
public class Switch
{
    public ISwitchable client;  // 인터페이스에 의존!

    public void Toggle()
    {
        client.Activate();      // 추상화된 메서드 호출
    }
}
```

---

## 🎯 원칙 개요

**Dependency Inversion Principle (DIP)** 은 **SOLID 원칙**의 다섯 번째 원칙으로, 상위 모듈은 하위 모듈에 의존하지 말고, 둘 다 추상화에 의존해야 한다는 원칙입니다.

### 📌 핵심 개념

```
1. 상위(High-level) 모듈은 하위(Low-level) 모듈의 것을 직접 가져오면 안됨
   → 둘 다 추상화(abstraction)에 의존해야 함

2. 추상화는 세부 사항에 의존해서는 안됨
   → 세부사항이 추상화에 의존해야 함

3. 클래스가 다른 클래스와 관계가 있으면 안됨
   → 클래스가 다른 클래스의 작동 방식을 많이 알고 있으면 안됨
   → 종속성(dependency) 또는 결합(coupling) 발생
   → 종속성은 어느 잠재적인 위험
```

**잘못된 설계 (High Coupling, Low Cohesion) :**
```csharp
// ❌ 상위 모듈이 하위 모듈에 직접 의존
public class UnrefactoredSwitch : MonoBehaviour
{
    public UnrefactoredDoor Door;  // 구체 클래스에 직접 의존!
    public UnrefactoredTrap Trap;  // 구체 클래스에 직접 의존!

    public void Activate()
    {
        if (IsActivated)
        {
            Door.Close();      // 구체적인 메서드 호출
            Trap.Disable();    // 구체적인 메서드 호출
        }
        else
        {
            Door.Open();       // 구체적인 메서드 호출
            Trap.Enable();     // 구체적인 메서드 호출
        }
    }
}
// 문제 : Switch가 Door와 Trap에 강하게 결합됨!
//        새로운 타입(NPC, Light 등) 추가 시 Switch를 수정해야 함!
```

**DIP 적용 (Loose Coupling, High Cohesion) :**
```csharp
// ✅ 인터페이스 정의 (추상화)
public interface ISwitchable
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
}

// ✅ 상위 모듈이 추상화에 의존
public class Switch : MonoBehaviour
{
    private MonoBehaviour m_ClientBehaviour;
    private ISwitchable m_Client => m_ClientBehaviour as ISwitchable;  // 추상화에 의존!

    public void Toggle()
    {
        if (m_Client.IsActive)
            m_Client.Deactivate();  // 추상화된 메서드 호출
        else
            m_Client.Activate();    // 추상화된 메서드 호출
    }
}

// ✅ 하위 모듈들이 추상화를 구현
public class Door : MonoBehaviour, ISwitchable
{
    public bool IsActive => m_IsActive;
    public void Activate()   { /* 도어 열기 */ }
    public void Deactivate() { /* 도어 닫기 */ }
}

public class Trap : MonoBehaviour, ISwitchable
{
    public bool IsActive => m_IsActive;
    public void Activate()   { /* 트랩 활성화 */ }
    public void Deactivate() { /* 트랩 비활성화 */ }
}

// 모두 추상화(ISwitchable)를 중심으로 연결됨! ✅
```

---

## 🤔 왜 DIP가 필요한가?

### 문제 상황

게임에서 스위치 시스템을 만들 때, 구체 클래스에 직접 의존하는 경우 :

```csharp
// 상위 모듈 : 스위치가 도어와 트랩을 직접 제어
public class UnrefactoredSwitch : MonoBehaviour
{
    public UnrefactoredDoor Door;  // 구체 클래스에 직접 의존!
    public UnrefactoredTrap Trap;  // 구체 클래스에 직접 의존!

    public void Activate()
    {
        if (IsActivated)
        {
            Door.Close();
            Trap.Disable();
        }
        else
        {
            Door.Open();
            Trap.Enable();
        }
    }
}

// 하위 모듈들
public class UnrefactoredDoor : MonoBehaviour
{
    public void Open()  { Debug.Log("도어가 열렸습니다."); }
    public void Close() { Debug.Log("도어가 닫혔습니다."); }
}

public class UnrefactoredTrap : MonoBehaviour
{
    public void Enable()  { Debug.Log("트랩이 활성화되었습니다."); }
    public void Disable() { Debug.Log("트랩이 비활성화되었습니다."); }
}
```

**이 코드의 문제점 :**

❌ **강한 결합 (High Coupling)**
   - Switch가 Door와 Trap의 구체적인 구현을 알아야 함
   - Door가 `Open()`, `Close()` 메서드를 가진다는 것을 알아야 함
   - Trap이 `Enable()`, `Disable()` 메서드를 가진다는 것을 알아야 함

❌ **확장성 부족**
   - 새로운 타입(NPC, Light, Elevator 등) 추가 시 Switch를 수정해야 함
   - Switch가 모든 타입의 메서드를 알아야 함

❌ **유지보수 어려움**
   - Door의 메서드명이 `Open()` → `Unlock()`로 변경되면?
   - Switch도 함께 수정해야 함!

❌ **재사용성 저하**
   - Switch를 다른 프로젝트에 재사용하려면?
   - Door, Trap도 함께 가져와야 함

❌ **테스트 어려움**
   - Switch를 테스트하려면 실제 Door, Trap 객체가 필요
   - Mock 객체 사용이 어려움

### DIP의 해결책

✅ **느슨한 결합 (Loose Coupling)**
   - Switch는 ISwitchable 인터페이스만 알면 됨
   - 구체적인 구현은 몰라도 됨

✅ **무한 확장 가능**
   - 새로운 타입 추가 시 ISwitchable만 구현하면 됨
   - Switch는 수정 불필요!

✅ **독립적 변경**
   - Door의 내부 구현 변경해도 Switch는 영향받지 않음
   - 인터페이스만 유지되면 됨

✅ **재사용성 향상**
   - Switch는 ISwitchable에만 의존
   - 어디서든 재사용 가능

✅ **테스트 용이**
   - Mock 객체로 ISwitchable 구현
   - Switch를 독립적으로 테스트 가능

---

## 🏗️ 핵심 개념

DIP를 이해하기 위한 핵심 구조 :

### 📐 의존성 역전 (Dependency Inversion)

**핵심 아이디어 :**
```
Before (의존성 방향) :
┌────────┐
│ Switch │ ─────→ Door (구체 클래스)
└────────┘    │
              └──→ Trap (구체 클래스)

⚠️ 상위 모듈(Switch)이 하위 모듈(Door, Trap)에 의존


After (의존성 역전!) :
┌────────┐          ┌──────────────┐
│ Switch │ ───→    │ ISwitchable  │ ←─── Door
└────────┘          │ (interface)  │ ←─── Trap
                    └──────────────┘ ←─── NPC
                                     ←─── Light

✅ 모두 추상화(ISwitchable)에 의존!
✅ Switch는 구체 클래스를 몰라도 됨!
✅ 새 타입 추가해도 Switch는 수정 불필요!
```

**구조 :**
```csharp
// 1단계 : 인터페이스 정의 (추상화)
public interface ISwitchable
{
    bool IsActive { get; }
    void Activate();
    void Deactivate();
}

// 2단계 : 상위 모듈이 추상화에 의존
public class Switch : MonoBehaviour
{
    [SerializeField] private MonoBehaviour m_ClientBehaviour;
    private ISwitchable m_Client => m_ClientBehaviour as ISwitchable;

    public void Toggle()
    {
        if (m_Client.IsActive)
            m_Client.Deactivate();
        else
            m_Client.Activate();
    }
}

// 3단계 : 하위 모듈들이 추상화를 구현
public class Door : MonoBehaviour, ISwitchable
{
    private bool m_IsActive;
    public bool IsActive => m_IsActive;

    public void Activate()
    {
        m_IsActive = true;
        Debug.Log("도어가 열렸습니다.");
        // 도어 열기 로직...
    }

    public void Deactivate()
    {
        m_IsActive = false;
        Debug.Log("도어가 닫혔습니다.");
        // 도어 닫기 로직...
    }
}

public class Trap : MonoBehaviour, ISwitchable
{
    private bool m_IsActive;
    public bool IsActive => m_IsActive;

    public void Activate()
    {
        m_IsActive = true;
        Debug.Log("트랩이 활성화되었습니다.");
        // 트랩 활성화 로직...
    }

    public void Deactivate()
    {
        m_IsActive = false;
        Debug.Log("트랩이 리셋되었습니다.");
        // 트랩 비활성화 로직...
    }
}

// 4단계 : 새 타입 추가 (Switch 수정 불필요!)
public class NPC : MonoBehaviour, ISwitchable
{
    private bool m_IsActive;
    public bool IsActive => m_IsActive;

    public void Activate()
    {
        m_IsActive = true;
        Debug.Log("NPC가 활성화되었습니다.");
        // NPC 활성화 로직...
    }

    public void Deactivate()
    {
        m_IsActive = false;
        Debug.Log("NPC가 비활성화되었습니다.");
        // NPC 비활성화 로직...
    }
}
```

---

## 📊 코드 구조

### 폴더 구조

```
5_DependencyInversion/
├── Scripts/
│   ├── ISwitchable.cs                ← ✅ 인터페이스 (추상화)
│   ├── Switch.cs                     ← ✅ 상위 모듈 (ISwitchable에 의존)
│   ├── Door.cs                       ← ✅ 하위 모듈 (ISwitchable 구현)
│   ├── Trap.cs                       ← ✅ 하위 모듈 (ISwitchable 구현)
│   │
│   └── Unrefactored/                 ← ❌ DIP 위반 예시
│       ├── UnrefactoredSwitch.cs     (구체 클래스에 직접 의존)
│       ├── UnrefactoredDoor.cs       (인터페이스 없음)
│       └── UnrefactoredTrap.cs       (인터페이스 없음)
│
└── README.md                          ← 📍 현재 문서
```

### 클래스 다이어그램

```
═══════════════════════════════════════════════════════════════
❌ DIP 위반 (Unrefactored)
═══════════════════════════════════════════════════════════════

        UnrefactoredSwitch (상위 모듈)
        ┌────────────────────────────────┐
        │ + Door door                    │
        │ + Trap trap                    │ ─────→ UnrefactoredDoor
        │ + bool IsActivated             │           (하위 모듈)
        │ + Activate()                   │           + Open()
        │   - door.Open()                │           + Close()
        │   - trap.Enable()              │
        │                                │ ─────→ UnrefactoredTrap
        └────────────────────────────────┘           (하위 모듈)
                                                      + Enable()
                                                      + Disable()

⚠️ Switch가 Door와 Trap에 강하게 결합!
⚠️ 새 타입 추가 시 Switch를 수정해야 함!
⚠️ Door의 메서드명 변경 시 Switch도 수정!

═══════════════════════════════════════════════════════════════
✅ DIP 준수 (Refactored)
═══════════════════════════════════════════════════════════════

                    <<interface>>
                    ISwitchable
                    ┌─────────────────┐
                    │ + IsActive      │
                    │ + Activate()    │
                    │ + Deactivate()  │
                    └─────────────────┘
                            △
                            │ implements
              +─────────────┼─────────────+
              │             │             │
        ┌─────┴──┐    ┌────┴────┐    ┌──┴───┐
        │  Door  │    │  Trap   │    │ NPC  │
        ├────────┤    ├─────────┤    ├──────┤
        │IsActive│    │IsActive │    │...   │
        │Activate│    │Activate │    │      │
        │Deactiv.│    │Deactiv. │    │      │
        └────────┘    └─────────┘    └──────┘
             △             △             △
             │             │             │
             └─────────────┴─────────────┘
                          │
                    ┌─────┴─────┐
                    │   Switch  │ (상위 모듈)
                    ├───────────┤
                    │+ client :  │
                    │ ISwitchable│ ← 추상화에만 의존!
                    ├───────────┤
                    │+ Toggle() │
                    │  if Active│
                    │   Deactiv.│
                    │  else     │
                    │   Activate│
                    └───────────┘

✅ Switch는 ISwitchable에만 의존!
✅ Door, Trap, NPC 모두 ISwitchable 구현!
✅ 새 타입 추가해도 Switch는 수정 불필요!
✅ 느슨한 결합 (Loose Coupling)!

═══════════════════════════════════════════════════════════════
```

---

## 🔄 Before & After 비교

### ❌ Before : DIP 미적용 (Unrefactored)

#### 1️⃣ UnrefactoredSwitch.cs (상위 모듈 - DIP 위반)

```csharp
/// <summary>
/// 리팩토링되지 않은 형태의 스위치 메커니즘을 나타내며, 도어나 트랩을 직접 제어합니다.
/// 구체적인 클래스(UnrefactoredDoor, UnrefactoredTrap)에 직접 의존하므로
/// 유연성이 떨어지고 제어하는 메커니즘의 특정 구현에 강하게 결합되어 있습니다.
/// </summary>
public class UnrefactoredSwitch : MonoBehaviour
{
    public UnrefactoredTrap Trap;  // ❌ 구체 클래스에 직접 의존!
    public UnrefactoredDoor Door;  // ❌ 구체 클래스에 직접 의존!
    public bool IsActivated;

    public void Activate()
    {
        if (IsActivated)
        {
            IsActivated = false;
            Door.Close();        // ❌ 구체적인 메서드 호출
            Trap.Disable();      // ❌ 구체적인 메서드 호출
        }
        else
        {
            IsActivated = true;
            Door.Open();         // ❌ 구체적인 메서드 호출
            Trap.Enable();       // ❌ 구체적인 메서드 호출
        }
    }
}
```

**문제점 :**
- 🔴 Switch가 Door와 Trap의 구체적인 메서드명을 알아야 함
- 🔴 Door가 `Open()`, `Close()`를 가진다는 것을 알아야 함
- 🔴 Trap이 `Enable()`, `Disable()`을 가진다는 것을 알아야 함
- 🔴 새로운 타입(NPC, Light 등) 추가 시 Switch를 수정해야 함

---

#### 2️⃣ UnrefactoredDoor.cs (하위 모듈)

```csharp
public class UnrefactoredDoor : MonoBehaviour
{
    public void Open()
    {
        Debug.Log("도어가 열렸습니다.");
    }

    public void Close()
    {
        Debug.Log("도어가 닫혔습니다.");
    }
}
```

**문제점 :**
- 🔴 인터페이스가 없어 표준화되지 않음
- 🔴 다른 타입들과 일관성이 없음

---

#### 3️⃣ UnrefactoredTrap.cs (하위 모듈)

```csharp
public class UnrefactoredTrap : MonoBehaviour
{
    private bool m_IsActive;
    public bool IsActive => m_IsActive;

    public void Enable()
    {
        m_IsActive = true;
        Debug.Log("트랩이 활성화되었습니다.");
    }

    public void Disable()
    {
        m_IsActive = false;
        Debug.Log("트랩이 비활성화되었습니다.");
    }
}
```

**문제점 :**
- 🔴 Door와 메서드명이 다름 (`Open/Close` vs `Enable/Disable`)
- 🔴 표준화된 인터페이스가 없음

---

### ✅ After : DIP 적용

#### 1️⃣ ISwitchable.cs (인터페이스 - 추상화)

```csharp
/// <summary>
/// 전환 가능한 객체에 대한 계약을 정의합니다. 이 인터페이스는 객체의 활성화/비활성화 세부사항을 추상화하여
/// 의존 역전 원칙(Dependency Inversion Principle, DIP)을 구현하는 데 도움을 줍니다.
/// </summary>
public interface ISwitchable
{
    public bool IsActive { get; }

    public void Activate();
    public void Deactivate();
}
```

**역할 :**
- ✅ 모든 전환 가능한 객체의 공통 계약 정의
- ✅ 상위 모듈과 하위 모듈 사이의 추상화 계층
- ✅ 표준화된 메서드명 (`Activate`, `Deactivate`)

---

#### 2️⃣ Switch.cs (상위 모듈 - DIP 준수)

```csharp
/// <summary>
/// ISwitchable 클라이언트의 상태를 전환할 수 있는 스위치 컴포넌트입니다. 이 클래스는
/// 구체적인 구현이 아닌 추상화(ISwitchable)에 의존함으로써 의존 역전 원칙을 보여줍니다.
/// </summary>
public class Switch : MonoBehaviour
{
    // Unity의 직렬화 시스템은 인터페이스를 직접 지원하지 않습니다. 이 제한을 우회하기 위해
    // ISwitchable을 구현하는 MonoBehaviour에 대한 직렬화된 참조를 사용합니다.
    [SerializeField] private MonoBehaviour m_ClientBehaviour;
    private ISwitchable m_Client => m_ClientBehaviour as ISwitchable;  // ✅ 추상화에 의존!

    // 연결된 ISwitchable 클라이언트의 활성 상태를 전환합니다.
    public void Toggle()
    {
        if (m_Client == null)
            return;

        if (m_Client.IsActive)
        {
            m_Client.Deactivate();  // ✅ 추상화된 메서드 호출
        }
        else
        {
            m_Client.Activate();    // ✅ 추상화된 메서드 호출
        }
    }
}
```

**개선 사항 :**
- ✅ ISwitchable 인터페이스에만 의존
- ✅ 구체 클래스를 몰라도 됨
- ✅ 새 타입 추가해도 수정 불필요
- ✅ Unity의 직렬화를 위한 우회 방법 제공

---

#### 3️⃣ Door.cs (하위 모듈 - ISwitchable 구현)

```csharp
/// <summary>
/// 두 개의 슬라이딩 도어를 열고 닫는 Door 컴포넌트입니다. 이 클래스는 추상 인터페이스 ISwitchable을 통해
/// 제어될 수 있도록 함으로써 의존 역전 원칙(DIP)을 보여줍니다. 이를 통해 도어를 트리거하는
/// 스위치로부터 도어를 분리합니다.
/// </summary>
public class Door : MonoBehaviour, ISwitchable
{
    [Tooltip("왼쪽 슬라이딩 도어")]
    [SerializeField] private Transform m_LeftDoor;
    [Tooltip("오른쪽 슬라이딩 도어")]
    [SerializeField] private Transform m_RightDoor;
    [Tooltip("왼쪽 도어를 열 때의 오프셋 위치")]
    [SerializeField] private Vector3 m_LeftDoorOffset;
    [Tooltip("오른쪽 도어를 열 때의 오프셋 위치")]
    [SerializeField] private Vector3 m_RightDoorOffset;
    [Tooltip("도어 열림/닫힘 속도")]
    [SerializeField] private float m_Speed = 5f;

    // 도어 위치 캐싱
    private Vector3 m_LeftDoorStartPosition;
    private Vector3 m_RightDoorStartPosition;
    private Vector3 m_LeftDoorEndPosition;
    private Vector3 m_RightDoorEndPosition;

    // 도어가 현재 열린 상태인지 추적합니다.
    private bool m_IsActive;
    public bool IsActive => m_IsActive;  // ✅ ISwitchable 구현

    private void Start()
    {
        // 도어 트랜스폼이 닫힌 위치에서 시작한다고 가정합니다.
        m_LeftDoorStartPosition  = m_LeftDoor.position;
        m_RightDoorStartPosition = m_RightDoor.position;
        m_LeftDoorEndPosition    = m_LeftDoorStartPosition + m_LeftDoorOffset;
        m_RightDoorEndPosition   = m_RightDoorStartPosition + m_RightDoorOffset;
    }

    /// 도어를 열고, 지정된 열림 위치로 이동시킵니다.
    public void Activate()  // ✅ ISwitchable 구현
    {
        m_IsActive = true;
        Debug.Log("도어가 열렸습니다.");
        StartCoroutine(SlideDoor(m_LeftDoor, m_LeftDoorEndPosition, m_Speed));
        StartCoroutine(SlideDoor(m_RightDoor, m_RightDoorEndPosition, m_Speed));
    }

    /// 도어를 닫고, 시작 위치로 되돌립니다.
    public void Deactivate()  // ✅ ISwitchable 구현
    {
        m_IsActive = false;
        Debug.Log("도어가 닫혔습니다.");
        StartCoroutine(SlideDoor(m_LeftDoor, m_LeftDoorStartPosition, m_Speed));
        StartCoroutine(SlideDoor(m_RightDoor, m_RightDoorStartPosition, m_Speed));
    }

    // 단일 도어를 특정 위치로 보간합니다.
    private IEnumerator SlideDoor(Transform door, Vector3 targetPosition, float speed)
    {
        while (door.position != targetPosition)
        {
            door.position = Vector3.MoveTowards(door.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }
    }
}
```

**개선 사항 :**
- ✅ ISwitchable 인터페이스 구현
- ✅ 표준화된 메서드명 사용 (`Activate`, `Deactivate`)
- ✅ 실제 슬라이딩 도어 구현 (코루틴 사용)

---

#### 4️⃣ Trap.cs (하위 모듈 - ISwitchable 구현)

```csharp
/// <summary>
/// Trap 클래스는 ISwitchable을 구현하는 물리 기반 트랩도어를 나타냅니다.
/// </summary>
public class Trap : MonoBehaviour, ISwitchable
{
    // 물리 상호작용을 위한 Rigidbody 컴포넌트
    private Rigidbody m_Rigidbody;

    // 트랩의 원래 위치, 위치 리셋에 사용됩니다.
    private Vector3 m_OriginalPosition;

    // 트랩의 원래 회전, 회전 리셋에 사용됩니다.
    private Quaternion m_OriginalRotation;

    // ISwitchable 활성 상태
    private bool m_IsActive;
    public bool IsActive => m_IsActive;  // ✅ ISwitchable 구현

    private void Start()
    {
        // 물리 컴포넌트 캐싱
        m_Rigidbody = GetComponent<Rigidbody>();

        // 물리 기반 이동을 비활성화하지만 충돌 감지와 수동 이동은 허용합니다.
        m_Rigidbody.isKinematic = true;

        // 원래 트랜스폼 값 캐싱
        m_OriginalPosition = transform.position;
        m_OriginalRotation = transform.rotation;
    }

    // 물리를 활성화하고 활성 상태로 표시합니다.
    public void Activate()  // ✅ ISwitchable 구현
    {
        m_IsActive = true;
        Debug.Log("트랩이 활성화되었습니다.");

        m_Rigidbody.isKinematic = false;
    }

    // 트랩을 비활성화하고 비활성 상태로 표시합니다.
    public void Deactivate()  // ✅ ISwitchable 구현
    {
        // Rigidbody를 kinematic으로 리셋하여 물리 기반 이동을 비활성화합니다.
        m_Rigidbody.isKinematic = true;
        m_IsActive = false;

        // 트랩의 위치와 회전을 원래 값으로 리셋합니다.
        transform.position = m_OriginalPosition;
        transform.rotation = m_OriginalRotation;

        Debug.Log("트랩이 리셋되었습니다.");
    }
}
```

**개선 사항 :**
- ✅ ISwitchable 인터페이스 구현
- ✅ 표준화된 메서드명 사용 (`Activate`, `Deactivate`)
- ✅ 실제 물리 기반 트랩 구현 (Rigidbody 사용)

---

### 📊 개선 효과

| 항목 | Before (Unrefactored) | After (DIP 적용) |
|------|----------------------|------------------|
| **의존성 방향** | Switch → Door, Trap (구체 클래스) | Switch → ISwitchable ← Door, Trap |
| **결합도** | 🔴 강한 결합 (High Coupling) | 🟢 느슨한 결합 (Loose Coupling) |
| **확장성** | 🔴 Switch 수정 필요 | 🟢 Switch 수정 불필요 |
| **메서드명** | 🔴 불일치 (Open/Close, Enable/Disable) | 🟢 통일 (Activate/Deactivate) |
| **재사용성** | 🔴 낮음 | 🟢 높음 |
| **테스트** | 🔴 어려움 | 🟢 쉬움 (Mock 사용 가능) |
| **유지보수** | 🔴 어려움 | 🟢 쉬움 |

---

## 🚀 확장 시나리오 : 프로젝트가 커질 때 어떻게 다를까?

데모 프로젝트는 의존 역전 원칙의 진가를 보여주는 완벽한 예시입니다.
실제 게임 개발에서 프로젝트가 확장될 때 어떤 차이가 있는지 살펴봅시다.

### 📌 시나리오 1 : NPC 추가하기

**상황 :** 스위치로 NPC를 활성화/비활성화하고 싶습니다.

#### ❌ DIP 미적용 (Unrefactored) - 수정 지옥

```csharp
// Step 1 : UnrefactoredSwitch 수정 필요! ⚠️
public class UnrefactoredSwitch : MonoBehaviour
{
    public UnrefactoredDoor Door;
    public UnrefactoredTrap Trap;
    public UnrefactoredNPC NPC;   // ⚠️ 새 필드 추가!
    public bool IsActivated;

    public void Activate()
    {
        if (IsActivated)
        {
            IsActivated = false;
            Door.Close();
            Trap.Disable();
            NPC.Disable();        // ⚠️ 새 메서드 호출 추가!
        }
        else
        {
            IsActivated = true;
            Door.Open();
            Trap.Enable();
            NPC.Enable();         // ⚠️ 새 메서드 호출 추가!
        }
    }
}

// Step 2 : UnrefactoredNPC 클래스 작성
public class UnrefactoredNPC : MonoBehaviour
{
    public void Enable()
    {
        Debug.Log("NPC가 활성화되었습니다.");
        // NPC 로직...
    }

    public void Disable()
    {
        Debug.Log("NPC가 비활성화되었습니다.");
        // NPC 로직...
    }
}

// ⚠️ 문제점 :
// - Switch 클래스를 수정해야 함 (OCP 위반!)
// - Switch가 모든 타입을 알아야 함
// - 타입이 10개, 20개로 늘어나면?
//   → Switch는 거대한 클래스가 됨!
```

#### ✅ DIP 적용 - 확장만 하면 끝!

```csharp
// Step 1 : NPC 클래스 작성 (Switch 수정 불필요!)
public class NPC : MonoBehaviour, ISwitchable
{
    private bool m_IsActive;
    public bool IsActive => m_IsActive;

    public void Activate()
    {
        m_IsActive = true;
        Debug.Log("NPC가 활성화되었습니다.");
        // NPC 활성화 로직...
    }

    public void Deactivate()
    {
        m_IsActive = false;
        Debug.Log("NPC가 비활성화되었습니다.");
        // NPC 비활성화 로직...
    }
}

// Step 2 : 끝! Switch는 전혀 수정하지 않음! ✅

// Unity Inspector에서 :
// Switch의 m_ClientBehaviour에 NPC를 할당만 하면 작동!

// ✅ 장점 :
// - Switch 클래스는 전혀 수정하지 않음!
// - NPC가 ISwitchable만 구현하면 됨
// - 100개의 타입이 추가되어도 Switch는 그대로!
```

---

### 📌 시나리오 2 : Light, Elevator, Turret 추가하기

**상황 :** 게임이 커져서 스위치로 제어할 오브젝트가 많아졌습니다.

#### ❌ DIP 미적용 - 클래스가 괴물이 됨

```csharp
public class UnrefactoredSwitch : MonoBehaviour
{
    // ⚠️ 필드가 계속 늘어남!
    public UnrefactoredDoor Door;
    public UnrefactoredTrap Trap;
    public UnrefactoredNPC NPC;
    public UnrefactoredLight Light;
    public UnrefactoredElevator Elevator;
    public UnrefactoredTurret Turret;
    // ... 계속 추가...

    public bool IsActivated;

    public void Activate()
    {
        if (IsActivated)
        {
            IsActivated = false;
            Door.Close();
            Trap.Disable();
            NPC.Disable();
            Light.TurnOff();         // ⚠️ 또 다른 메서드명!
            Elevator.MoveDown();     // ⚠️ 또 다른 메서드명!
            Turret.StopFiring();     // ⚠️ 또 다른 메서드명!
            // ⚠️ 메서드 호출이 계속 늘어남!
        }
        else
        {
            IsActivated = true;
            Door.Open();
            Trap.Enable();
            NPC.Enable();
            Light.TurnOn();          // ⚠️ 또 다른 메서드명!
            Elevator.MoveUp();       // ⚠️ 또 다른 메서드명!
            Turret.StartFiring();    // ⚠️ 또 다른 메서드명!
            // ⚠️ 메서드 호출이 계속 늘어남!
        }
    }
}

// ⚠️ 문제점 :
// - Switch가 200줄, 300줄로 비대해짐
// - 메서드명이 제각각 (Open/Close, Enable/Disable, TurnOn/TurnOff, MoveUp/MoveDown...)
// - 새 개발자가 이해하기 어려움
// - Git 충돌 발생 위험 (여러 팀원이 Switch를 수정)
```

#### ✅ DIP 적용 - Switch는 여전히 10줄!

```csharp
// ✅ Switch는 그대로! (10줄 유지)
public class Switch : MonoBehaviour
{
    [SerializeField] private MonoBehaviour m_ClientBehaviour;
    private ISwitchable m_Client => m_ClientBehaviour as ISwitchable;

    public void Toggle()
    {
        if (m_Client == null) return;

        if (m_Client.IsActive)
            m_Client.Deactivate();
        else
            m_Client.Activate();
    }
}

// ✅ 각 타입이 ISwitchable 구현 (독립적인 파일)

// 📁 Light.cs
public class Light : MonoBehaviour, ISwitchable
{
    public bool IsActive => m_IsActive;
    public void Activate() { /* 불 켜기 */ }
    public void Deactivate() { /* 불 끄기 */ }
}

// 📁 Elevator.cs
public class Elevator : MonoBehaviour, ISwitchable
{
    public bool IsActive => m_IsActive;
    public void Activate() { /* 엘리베이터 올리기 */ }
    public void Deactivate() { /* 엘리베이터 내리기 */ }
}

// 📁 Turret.cs
public class Turret : MonoBehaviour, ISwitchable
{
    public bool IsActive => m_IsActive;
    public void Activate() { /* 터렛 발사 시작 */ }
    public void Deactivate() { /* 터렛 발사 중지 */ }
}

// ✅ 장점 :
// - Switch는 여전히 10줄! (100개 타입 추가해도!)
// - 각 타입이 독립적인 파일 (Git 충돌 없음)
// - 메서드명이 통일됨 (Activate/Deactivate)
// - 팀원들이 독립적으로 작업 가능
```

---

### 📌 시나리오 3 : 멀티 스위치 (하나의 스위치로 여러 객체 제어)

**상황 :** 하나의 스위치로 도어 + 트랩 + NPC를 동시에 제어하고 싶습니다.

#### ❌ DIP 미적용 - 하드코딩된 조합

```csharp
public class UnrefactoredMultiSwitch : MonoBehaviour
{
    public UnrefactoredDoor Door;
    public UnrefactoredTrap Trap;
    public UnrefactoredNPC NPC;

    public void Activate()
    {
        // ⚠️ 조합이 하드코딩됨!
        Door.Open();
        Trap.Enable();
        NPC.Enable();
    }
}

// ⚠️ 문제점 :
// - 조합을 바꾸려면 클래스를 수정해야 함
// - 다른 조합을 만들려면 새 클래스 필요
//   → UnrefactoredMultiSwitch1, UnrefactoredMultiSwitch2, ...
// - 유연성 제로!
```

#### ✅ DIP 적용 - 무한 조합 가능!

```csharp
// ✅ 리스트로 관리!
public class MultiSwitch : MonoBehaviour
{
    [SerializeField] private MonoBehaviour[] m_ClientBehaviours;  // 배열!

    public void Toggle()
    {
        foreach (var behaviour in m_ClientBehaviours)
        {
            ISwitchable client = behaviour as ISwitchable;
            if (client == null) continue;

            if (client.IsActive)
                client.Deactivate();
            else
                client.Activate();
        }
    }
}

// Unity Inspector에서 :
// m_ClientBehaviours 배열에
// - Door, Trap, NPC 추가 → 3개 동시 제어
// - Door만 추가 → 도어만 제어
// - Light, Elevator, Turret 추가 → 이것들만 제어
// → 코드 수정 없이 인스펙터에서 자유롭게 조합!

// ✅ 장점 :
// - 무한 조합 가능!
// - 런타임에 동적으로 추가/제거 가능!
// - 코드 수정 전혀 불필요!
```

---

### 📊 확장 시나리오 비교표

| 시나리오 | Before (Unrefactored) | After (DIP 적용) |
|---------|----------------------|------------------|
| **NPC 추가** | Switch 수정 필요 (20줄 추가) | NPC.cs만 작성 (Switch 수정 0줄) |
| **6개 타입 추가** | Switch가 200줄로 비대해짐 | Switch는 여전히 10줄 |
| **멀티 스위치** | 새 클래스 필요 (조합마다) | 배열로 무한 조합 가능 |
| **메서드명 변경** | 모든 코드 수정 필요 | 인터페이스만 변경 |
| **테스트** | 모든 타입 필요 | Mock으로 독립 테스트 |
| **Git 충돌** | 🔴 자주 발생 | 🟢 거의 없음 |

---

### 🎯 데모 프로젝트가 보여주는 핵심

이 데모 프로젝트는 **DIP의 진정한 가치**를 보여줍니다 :

1. **처음에는** (Door, Trap만) UnrefactoredSwitch가 더 간단해 보입니다
2. **확장되면서** (NPC, Light, Elevator 추가) DIP의 위력이 드러납니다
3. **결국에는** DIP 적용한 코드가 **압도적으로 유지보수가 쉬워집니다**

**이것이 바로 의존 역전 원칙의 힘입니다! 🚀**

---

## 💻 주요 코드 분석

### 📌 핵심 1 : Unity의 인터페이스 직렬화 우회

**문제 :** Unity의 `[SerializeField]`는 인터페이스를 직접 지원하지 않습니다.

**해결책 :**
```csharp
public class Switch : MonoBehaviour
{
    // ⚠️ 이렇게는 안 됨!
    // [SerializeField] private ISwitchable m_Client;  // 컴파일 에러!

    // ✅ 이렇게 우회!
    [SerializeField] private MonoBehaviour m_ClientBehaviour;
    private ISwitchable m_Client => m_ClientBehaviour as ISwitchable;

    public void Toggle()
    {
        if (m_Client == null) return;  // null 체크 필수!

        if (m_Client.IsActive)
            m_Client.Deactivate();
        else
            m_Client.Activate();
    }
}
```

**이해 포인트 :**
- `MonoBehaviour`를 직렬화 → Inspector에 표시됨
- 런타임에 `as ISwitchable`로 캐스팅
- `null` 체크로 안전성 확보

---

### 📌 핵심 2 : 프로퍼티를 통한 인터페이스 구현

```csharp
public interface ISwitchable
{
    bool IsActive { get; }  // ← 프로퍼티!
    void Activate();
    void Deactivate();
}

public class Door : MonoBehaviour, ISwitchable
{
    private bool m_IsActive;                // private 필드
    public bool IsActive => m_IsActive;     // public 프로퍼티 (읽기 전용)

    public void Activate()
    {
        m_IsActive = true;  // ✅ 내부에서만 쓰기 가능
        // ...
    }
}
```

**이해 포인트 :**
- `IsActive`는 읽기 전용 프로퍼티
- 외부에서는 읽기만 가능, 쓰기는 불가능
- 캡슐화 유지하면서 인터페이스 준수

---

### 📌 핵심 3 : 다형성을 통한 유연한 제어

```csharp
ISwitchable client;

// Door 할당
client = new Door();
client.Activate();  // → Door.Activate() 호출

// Trap 할당
client = new Trap();
client.Activate();  // → Trap.Activate() 호출

// NPC 할당
client = new NPC();
client.Activate();  // → NPC.Activate() 호출

// ✅ 같은 코드(client.Activate())로 다른 동작!
// ✅ 이것이 다형성(Polymorphism)의 힘!
```

---

## ⚖️ 장단점

### ✅ 장점

**1. 느슨한 결합 (Loose Coupling)**
- 상위 모듈이 하위 모듈에 의존하지 않음
- 변경 사항이 전파되지 않음

**2. 무한 확장 가능**
- 새 타입 추가 시 기존 코드 수정 불필요
- ISwitchable만 구현하면 됨

**3. 재사용성 향상**
- Switch를 다른 프로젝트에 재사용 가능
- 구체 클래스와 독립적

**4. 테스트 용이**
- Mock 객체로 Switch를 독립적으로 테스트
- 하위 모듈 없이도 테스트 가능

**5. 유지보수 쉬움**
- 각 클래스가 독립적으로 관리됨
- 변경 영향 범위가 제한적

**6. 협업 효율**
- 팀원들이 독립적으로 작업 가능
- Git 충돌 최소화

**7. 표준화**
- 모든 타입이 동일한 인터페이스 사용
- 일관된 메서드명

### ❌ 단점

**1. 초기 설계 복잡도**
- 인터페이스 설계 필요
- 추상화 이해 필요

**2. 간접 참조 오버헤드**
- 인터페이스를 통한 호출
- 약간의 성능 오버헤드 (미미함)

**3. Unity의 제약**
- 인터페이스 직렬화 불가
- 우회 방법 필요

**4. 간단한 프로젝트에는 과도함**
- 타입이 1~2개만 있을 때는 오버엔지니어링
- 트레이드오프 고려 필요

---

## 🎮 실제 적용 사례

### 1️⃣ 인터랙션 시스템

```csharp
// ✅ DIP 적용
public interface IInteractable
{
    void Interact(GameObject interactor);
}

public class Player : MonoBehaviour
{
    private IInteractable currentInteractable;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable?.Interact(gameObject);
        }
    }
}

// 다양한 인터랙션 구현
public class Chest : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor) { /* 상자 열기 */ }
}

public class NPC : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor) { /* 대화 시작 */ }
}

public class LeverSwitch : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor) { /* 레버 당기기 */ }
}
```

### 2️⃣ 데미지 시스템

```csharp
// ✅ DIP 적용
public interface IDamageable
{
    void TakeDamage(float amount);
}

public class Weapon : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        damageable?.TakeDamage(10f);
    }
}

// 다양한 데미지 대상
public class Enemy : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount) { /* 적 데미지 */ }
}

public class Barrel : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount) { /* 폭발하는 통 */ }
}

public class Shield : MonoBehaviour, IDamageable
{
    public void TakeDamage(float amount) { /* 데미지 흡수 */ }
}
```

### 3️⃣ 저장 시스템

```csharp
// ✅ DIP 적용
public interface ISaveSystem
{
    void Save(GameData data);
    GameData Load();
}

public class GameManager : MonoBehaviour
{
    private ISaveSystem saveSystem;

    void Awake()
    {
        // 런타임에 저장 방식 결정
        #if UNITY_EDITOR
            saveSystem = new LocalSaveSystem();
        #else
            saveSystem = new CloudSaveSystem();
        #endif
    }

    void SaveGame()
    {
        saveSystem.Save(gameData);
    }
}

// 다양한 저장 방식
public class LocalSaveSystem : ISaveSystem
{
    public void Save(GameData data) { /* 로컬 저장 */ }
    public GameData Load() { /* 로컬 로드 */ }
}

public class CloudSaveSystem : ISaveSystem
{
    public void Save(GameData data) { /* 클라우드 저장 */ }
    public GameData Load() { /* 클라우드 로드 */ }
}
```

---

## 📝 학습 정리

### 핵심 요약

1. **의존 역전 원칙 (DIP)**
   - 상위 모듈은 하위 모듈에 의존하지 말아야 함
   - 둘 다 추상화(인터페이스)에 의존해야 함

2. **DIP 위반 사례**
   - 상위 모듈이 구체 클래스에 직접 의존
   - 강한 결합 (High Coupling)
   - 확장이 어려움

3. **DIP 준수 방법**
   - 인터페이스 정의 (추상화)
   - 상위 모듈이 인터페이스에 의존
   - 하위 모듈들이 인터페이스 구현

4. **실전 적용**
   - `ISwitchable` 인터페이스 정의
   - `Switch`가 `ISwitchable`에 의존
   - `Door`, `Trap`, `NPC` 등이 `ISwitchable` 구현

5. **장점**
   - 느슨한 결합, 무한 확장, 재사용성, 테스트 용이, 유지보수 쉬움

6. **주의사항**
   - 간단한 프로젝트에는 과도함
   - Unity의 인터페이스 직렬화 제약 고려

### Before vs After

| | Before (Unrefactored) | After (DIP 적용) |
|---|----------------------|------------------|
| **의존성** | Switch → Door, Trap | Switch → ISwitchable |
| **결합도** | ❌ 강한 결합 | ✅ 느슨한 결합 |
| **확장** | ❌ Switch 수정 필요 | ✅ Switch 수정 불필요 |
| **재사용** | ❌ 낮음 | ✅ 높음 |
| **테스트** | ❌ 어려움 | ✅ 쉬움 |

### 실무 적용 팁

✅ **이런 경우 DIP 적용 고려**
- 상위 모듈이 여러 하위 모듈에 의존할 때
- 하위 모듈이 자주 추가/변경될 때
- 테스트 용이성이 중요할 때
- 재사용성을 높이고 싶을 때

❌ **이런 경우 무리하게 적용 X**
- 하위 모듈이 1개만 있을 때
- 변경 가능성이 거의 없을 때
- 프로토타입 단계
- 간단한 일회성 코드

### 다른 SOLID 원칙과의 관계

**OCP (Open-Closed Principle)와의 관계 :**
- DIP는 OCP를 달성하는 수단
- 추상화에 의존하면 확장에 열림

**LSP (Liskov Substitution Principle)와의 관계 :**
- DIP를 위해서는 LSP가 필수
- 하위 타입들이 인터페이스를 올바르게 구현해야 함

**ISP (Interface Segregation Principle)와의 관계 :**
- DIP와 ISP는 함께 사용됨
- 작고 명확한 인터페이스가 DIP를 쉽게 만듦

### 핵심 문구

```
"상위 모듈은 하위 모듈에 의존하지 말고,
 둘 다 추상화에 의존하라"

상위 모듈 (Higher-level) : Switch (정책, 비즈니스 로직)
하위 모듈 (Lower-level) : Door, Trap, NPC (구현 세부사항)
추상화 (Abstraction) : ISwitchable (인터페이스)

→ Switch → ISwitchable ← Door, Trap, NPC
→ 모두 ISwitchable을 중심으로 연결됨!
```

### 마무리

**의존 역전 원칙의 진정한 가치 :**

처음 2개 타입(Door, Trap)만 만들 때는 UnrefactoredSwitch가 더 간단해 보입니다.

하지만 프로젝트가 성장하면서 NPC, Light, Elevator, Turret...로 늘어날 때,
ISwitchable 인터페이스를 사용한 설계의 진가가 드러납니다.

**단기적으로는** 코드가 조금 더 복잡해 보일 수 있지만,
**장기적으로는** 유지보수와 확장을 **극적으로** 쉽게 만듭니다!

이것이 바로 **의존 역전 원칙**의 힘입니다! 🚀

---

**마지막 업데이트 :** 2025.12.01
