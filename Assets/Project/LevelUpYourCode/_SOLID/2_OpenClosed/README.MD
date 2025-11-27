# 🔓 Open-Closed Principle (개방-폐쇄 원칙)

## 📋 목차
- [원칙 개요](#-원칙-개요)
- [왜 OCP가 필요한가?](#-왜-ocp가-필요한가)
- [핵심 개념](#-핵심-개념)
- [코드 구조](#-코드-구조)
- [Before & After 비교](#-before--after-비교)
- [실전 시뮬레이션](#-실전-시뮬레이션)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 적용 사례](#-실제-적용-사례)
- [학습 정리](#-학습-정리)

---

## 🎯 원칙 개요

**Open-Closed Principle (OCP)** 은 **SOLID 원칙**의 두 번째 원칙으로, 소프트웨어 개체는 **확장에는 열려 있고, 수정에는 닫혀 있어야** 한다는 원칙입니다.

### 📌 핵심 개념

```
확장에는 열려 있고 (Open for Extension)
수정에는 닫혀 있어야 한다 (Closed for Modification)
```

**확장에 열려 있다 (Open for Extension) :**
- ✅ 새로운 기능(도형)을 **자유롭게 추가**할 수 있다
- ✅ **새 파일**을 만들어서 확장한다

**수정에 닫혀 있다 (Closed for Modification) :**
- ✅ 기존 코드를 **수정하지 않는다**
- ✅ **버그 위험**을 최소화한다

---

## 🤔 왜 OCP가 필요한가?

### 문제 상황

게임에서 다양한 도형 효과의 면적을 계산해야 하는 경우 :

```csharp
public class UnrefactoredAreaCalculator
{
    public float GetRectangleArea(Rectangle rectangle)
    {
        return rectangle.Width * rectangle.Height;
    }

    public float GetCircleArea(Circle circle)
    {
        return circle.Radius * circle.Radius * Mathf.PI;
    }

    // ⚠️ 새 도형 추가하려면?
    // → 이 클래스를 수정해야 함!
    // → 기존에 잘 작동하던 코드를 건드려야 함!
}
```

**이 코드의 문제점 :**

❌ **기존 코드 수정 필요**
   - 새 도형 추가할 때마다 `UnrefactoredAreaCalculator` 수정
   - 잘 작동하던 코드를 건드려야 함 → 버그 위험!

❌ **확장이 어려움**
   - 메서드를 계속 추가해야 함
   - 타입 체크 분기문이 계속 늘어남

❌ **테스트 부담 증가**
   - 클래스 수정할 때마다 모든 도형을 다시 테스트해야 함

❌ **코드 복잡도 증가**
   - 도형이 늘어날수록 클래스가 비대해짐
   - 유지보수가 점점 어려워짐

### OCP의 해결책

✅ **확장 가능**
   - 새로운 도형을 자유롭게 추가 가능

✅ **기존 코드 보호**
   - 기존 코드를 수정하지 않음
   - 버그 발생 위험 최소화

✅ **독립적 테스트**
   - 새 도형만 테스트하면 됨
   - 기존 도형들은 영향받지 않음

✅ **선형적 복잡도**
   - 도형이 늘어나도 복잡도가 선형적으로만 증가

---

## 🏗️ 핵심 개념

OCP를 이해하기 위한 핵심 구조 :

### 📐 추상화와 다형성

**핵심 아이디어 :**
```
1. 추상 클래스/인터페이스로 공통 동작 정의
2. 구체 클래스들이 이를 상속/구현
3. 새 기능은 새 구체 클래스로 추가
4. 기존 코드는 추상화에만 의존 → 수정 불필요!
```

**구조 :**
```csharp
// 1단계 : 추상화 정의
public abstract class AreaOfEffect
{
    public abstract float CalculateArea();  // 공통 동작
    public void PlayEffect() { /* ... */ }  // 공통 기능
}

// 2단계 : 구체 클래스 구현
public class CircleEffect : AreaOfEffect
{
    public override float CalculateArea()
    {
        return Radius * Radius * Mathf.PI;
    }
}

public class RectangleEffect : AreaOfEffect
{
    public override float CalculateArea()
    {
        return Width * Height;
    }
}

// 3단계 : 새 기능 추가 (기존 코드 수정 없음!)
public class HexagonEffect : AreaOfEffect
{
    public override float CalculateArea()
    {
        return (3 * Mathf.Sqrt(3) / 2) * SideLength * SideLength;
    }
}

// 4단계 : 사용하는 쪽 (수정 불필요!)
public class EffectTrigger
{
    [SerializeField] AreaOfEffect m_Effect;  // 추상화에만 의존

    void OnTriggerEnter(Collider other)
    {
        m_Effect.PlayEffect();  // ✅ 어떤 도형이든 작동!
        // ↳ PlayEffect() 내부에서 CalculateArea() 호출됨
        //   → CircleEffect면 → CircleEffect.CalculateArea()
        //   → HexagonEffect면 → HexagonEffect.CalculateArea()
    }
}
```

---

## 📊 코드 구조

### 폴더 구조

```
2_OpenClosed/
├── Scripts/
│   ├── AreaOfEffect.cs              ← ✅ 추상 클래스 (공통 기능)
│   ├── CircleEffect.cs              ← 원형 효과
│   ├── RectangleEffect.cs           ← 사각형 효과
│   ├── HexagonalEffect.cs           ← 육각형 효과
│   ├── TriangularEffect.cs          ← 삼각형 효과
│   ├── EffectTrigger.cs             ← 효과 트리거 (사용하는 쪽)
│   │
│   └── Unrefactored/
│       └── UnrefactoredAreaCalculator.cs  ← ❌ OCP 미적용
│
└── README.MD                         ← 📍 현재 문서
```

### 클래스 다이어그램

```
                    AreaOfEffect (추상 클래스)
                           |
              +CalculateArea() (추상 메서드)
              +PlayEffect()
              +파티클/오디오 관련 기능
                           |
         +-----------------+-----------------+-----------------+
         |                 |                 |                 |
    CircleEffect    RectangleEffect   HexagonalEffect   TriangularEffect
  +CalculateArea()  +CalculateArea()  +CalculateArea()  +CalculateArea()
  (반지름 공식)     (가로×세로)       (육각형 공식)     (삼각형 공식)


         EffectTrigger
              |
    AreaOfEffect 참조  ← ✅ 추상화에만 의존!
    - PlayEffect() 호출


VS (OCP 미적용)


    UnrefactoredAreaCalculator
              |
    +GetRectangleArea(Rectangle)  ← ❌ 구체 타입에 의존!
    +GetCircleArea(Circle)        ← ❌ 도형 추가 시 수정 필요!
    +GetTriangleArea(Triangle)    ← ❌ 메서드 계속 추가...
    +GetPentagonArea(Pentagon)    ← ❌ 클래스가 비대해짐...
    // ... 계속 추가
```

---

## 🔄 Before & After 비교

### ❌ Before : OCP 미적용 (UnrefactoredAreaCalculator.cs)

```csharp
public class UnrefactoredAreaCalculator
{
    // Non-SOLID 구현 : 개방-폐쇄 원칙을 사용하지 않습니다.
    // 이 접근 방식은 적은 수의 효과에서는 잘 작동하지만,
    // 프로젝트가 커지면 확장성이 떨어지고 다루기 어려워집니다.

    public float GetRectangleArea(Rectangle rectangle)
    {
        return rectangle.Width * rectangle.Height;
    }

    public float GetCircleArea(Circle circle)
    {
        return circle.Radius * circle.Radius * Mathf.PI;
    }

    // ⚠️ 삼각형 추가하려면 이 클래스를 수정해야 함!
    public float GetTriangleArea(Triangle triangle)
    {
        return (Mathf.Sqrt(3) / 4) * triangle.SideLength * triangle.SideLength;
    }

    // ⚠️ 육각형 추가하려면 또 수정!
    public float GetHexagonArea(Hexagon hexagon)
    {
        return (3 * Mathf.Sqrt(3) / 2) * hexagon.SideLength * hexagon.SideLength;
    }

    // ⚠️ 계속 추가...
    // GetPentagonArea, GetOctagonArea, GetEllipseArea...
}

public class Rectangle
{
    public float Height;
    public float Width;
}

public class Circle
{
    public float Radius;
}
```

**문제점 :**
- 🔴 새 도형 추가할 때마다 `UnrefactoredAreaCalculator` 수정 필요
- 🔴 기존에 잘 작동하던 코드를 건드려야 함 → 버그 위험!
- 🔴 도형이 10개, 20개로 늘어나면 클래스가 비대해짐
- 🔴 타입 체크 분기문이 계속 늘어남

**사용하는 쪽의 문제 :**
```csharp
public class ShapeProcessor
{
    UnrefactoredAreaCalculator calculator = new UnrefactoredAreaCalculator();

    public void ProcessShape(object shape)
    {
        // ⚠️ 타입 체크 분기문이 계속 늘어남!
        if (shape is Rectangle rect)
        {
            float area = calculator.GetRectangleArea(rect);
        }
        else if (shape is Circle circle)
        {
            float area = calculator.GetCircleArea(circle);
        }
        else if (shape is Triangle triangle)
        {
            float area = calculator.GetTriangleArea(triangle);
        }
        else if (shape is Hexagon hexagon)
        {
            float area = calculator.GetHexagonArea(hexagon);
        }
        // ⚠️ 새 도형 추가하면 여기도 수정!
        // ⚠️ 실수로 분기를 빠뜨리면 버그 발생!
    }
}
```

---

### ✅ After : OCP 적용

#### 1️⃣ AreaOfEffect.cs (추상 클래스)

```csharp
/// <summary>
/// ParticleSystem과 AudioClip을 재생하는 클래스.
///
/// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
/// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
/// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
/// </summary>
public abstract class AreaOfEffect : MonoBehaviour
{
    [Header("Particle Effect")]
    [SerializeField] ParticleSystem m_EffectParticleSystem;

    [Header("Audio Effect")]
    [SerializeField] AudioSource m_EffectAudioSource;
    [SerializeField] AudioClip   m_EffectSoundFX;

    [Space]
    [SerializeField] float  m_CooldownTime = 1.0f;
    [SerializeField] string m_LabelString;
    [SerializeField] Text   m_LabelText;

    /// <summary> 이 AreaOfEffect의 파티클 시스템 </summary>
    public ParticleSystem EffectParticleSystem => m_EffectParticleSystem;
    /// <summary> 이 AreaOfEffect의 오디오 소스 </summary>
    public AudioSource EffectAudioSource => m_EffectAudioSource;
    /// <summary> 이 AreaOfEffect의 오디오 클립 </summary>
    public AudioClip EffectSoundFX => m_EffectSoundFX;

    private float cooldownTimer;

    /// <summary>
    /// 각 AreaOfEffect 서브클래스는 고유한 CalculateArea 정의를 구현합니다.
    /// </summary>
    public abstract float CalculateArea();

    /// <summary>
    /// 사운드와 효과를 재생합니다.
    /// </summary>
    public void PlayEffect()
    {
        // 쿨다운 시간이 경과했는지 확인
        if (Time.time >= cooldownTimer)
        {
            cooldownTimer = Time.time + m_CooldownTime;
            PlayParticleEffect();
            PlaySoundEffect();
            ShowAreaText();  // ← 여기서 CalculateArea() 호출!
        }
    }

    private void PlayParticleEffect()
    {
        if (m_EffectParticleSystem != null)
        {
            m_EffectParticleSystem.Play();
        }
    }

    private void PlaySoundEffect()
    {
        if (m_EffectAudioSource != null && m_EffectSoundFX != null)
        {
            m_EffectAudioSource.PlayOneShot(m_EffectSoundFX);
        }
    }

    public void ShowAreaText()
    {
        if (m_LabelText != null)
        {
            // ✅ 여기서 각 서브클래스의 CalculateArea()가 호출됨!
            // → CircleEffect.CalculateArea() 또는
            // → RectangleEffect.CalculateArea() 또는
            // → HexagonEffect.CalculateArea() 등
            m_LabelText.text = m_LabelString + " " + CalculateArea();
        }
    }
}
```

**책임 : 공통 기능 제공**
- 추상 메서드 `CalculateArea()` 정의
- 파티클 효과 재생
- 사운드 재생
- 면적 표시

**📞 호출 체인 (Call Chain) :**
```
외부에서 호출 : PlayEffect()
    ↓
내부 호출 : ShowAreaText()
    ↓
추상 메서드 호출 : CalculateArea()  ← 각 서브클래스에서 구현!
    ↓
결과 반환 : 계산된 면적 값
```

---

#### 2️⃣ CircleEffect.cs (원형 효과)

```csharp
/// <summary>
/// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
/// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
/// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
/// </summary>
public class CircleEffect : AreaOfEffect
{
    [Header("Shape")]
    [Tooltip("원의 반지름")]
    [SerializeField] float m_Radius;

    public float Radius { get => m_Radius; set => m_Radius = value; }

    public override float CalculateArea()
    {
        return Radius * Radius * Mathf.PI;
    }
}
```

**책임 : 원형 면적 계산**
- `CalculateArea()` 구현 (원 공식)

---

#### 3️⃣ RectangleEffect.cs (사각형 효과)

```csharp
/// <summary>
/// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
/// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
/// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
/// </summary>
public class RectangleEffect : AreaOfEffect
{
    [Header("Shape")]
    [Tooltip("사각형의 너비")]
    [SerializeField] private float m_Width;
    [Tooltip("사각형의 높이")]
    [SerializeField] private float m_Height;

    public override float CalculateArea()
    {
        return m_Width * m_Height;
    }
}
```

**책임 : 사각형 면적 계산**
- `CalculateArea()` 구현 (가로 × 세로)

---

#### 4️⃣ HexagonalEffect.cs (육각형 효과)

```csharp
/// <summary>
/// 육각형 효과를 표시하는 클래스.
///
/// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
/// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
/// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
/// </summary>
public class HexagonalEffect : AreaOfEffect
{
    [Header("Shape")]
    [Tooltip("육각형의 한 변 길이")]
    [SerializeField] private float m_SideLength;

    public override float CalculateArea()
    {
        return (3 * Mathf.Sqrt(3) / 2) * m_SideLength * m_SideLength;
    }
}
```

**책임 : 육각형 면적 계산**
- `CalculateArea()` 구현 (육각형 공식)

---

#### 5️⃣ TriangularEffect.cs (삼각형 효과)

```csharp
/// <summary>
/// 정삼각형 효과를 표시하는 클래스.
///
/// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
/// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
/// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
/// </summary>
public class TriangularEffect : AreaOfEffect
{
    [Header("Shape")]
    [Tooltip("삼각형의 한 변 길이")]
    [SerializeField] private float m_SideLength;

    public override float CalculateArea()
    {
        return (Mathf.Sqrt(3) / 4) * m_SideLength * m_SideLength;
    }
}
```

**책임 : 삼각형 면적 계산**
- `CalculateArea()` 구현 (정삼각형 공식)

---

#### 6️⃣ EffectTrigger.cs (사용하는 쪽)

```csharp
public class EffectTrigger : MonoBehaviour
{
    [Tooltip("이 컴포넌트와 충돌 시 트리거되는 AreaOfEffect")]
    [SerializeField] AreaOfEffect m_Effect;
    [Tooltip("트리거 간 최소 시간(초)")]
    [SerializeField] float m_Cooldown = 2f;

    float m_LastEffectTime = -1;
    const string k_PlayerTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        PlayEffect(other);

        if (other.CompareTag(k_PlayerTag) && m_Effect != null)
            m_Effect.ShowAreaText();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(k_PlayerTag))
            PlayEffect(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(k_PlayerTag) && m_Effect != null)
            m_Effect.ShowLabelText(string.Empty);
    }

    private void PlayEffect(Collider other)
    {
        float nextEffectTime = m_LastEffectTime + m_Cooldown;

        // 태그로 확인
        if (other.CompareTag(k_PlayerTag) && Time.time > nextEffectTime)
        {
            m_LastEffectTime = Time.time;

            // ✅ 어떤 도형이든 동일하게 처리!
            // ✅ 분기문 없음!
            m_Effect.PlayEffect();
            // ↳ PlayEffect() 내부에서 ShowAreaText() 호출
            //   ↳ ShowAreaText() 내부에서 CalculateArea() 호출
            //     → 다형성으로 각 도형의 CalculateArea()가 자동 실행됨!
        }
    }
}
```

**핵심 :**
- ✅ `AreaOfEffect` 추상화에만 의존
- ✅ 타입 체크 분기문 없음!
- ✅ 새 도형 추가해도 이 코드는 수정 불필요!

---

### 📊 개선 효과

| 항목 | Before (UnrefactoredAreaCalculator) | After (OCP 적용) |
|------|-------------------------------------|------------------|
| **파일 수** | 1개 (계속 늘어남) | 각 도형마다 1개 |
| **새 도형 추가** | 🔴 기존 클래스 수정 | 🟢 새 파일만 추가 |
| **기존 코드 영향** | 🔴 수정 필요 | 🟢 수정 불필요 |
| **타입 체크** | 🔴 분기문 계속 증가 | 🟢 분기문 없음 |
| **테스트** | 🔴 전체 테스트 필요 | 🟢 새 도형만 테스트 |
| **버그 위험** | 🔴 높음 | 🟢 낮음 |
| **확장성** | 🔴 매우 낮음 | 🟢 매우 높음 |

---

## 🎬 실전 시뮬레이션

### 시나리오 : "오각형(Pentagon)" 추가하기

프로젝트에 새로운 오각형 도형을 추가해야 한다고 가정해봅시다.

---

### ❌ 방식 1 : UnrefactoredAreaCalculator (OCP 위반)

#### 🔴 Step 1 : 오각형 클래스 추가

```csharp
public class Pentagon
{
    public float SideLength;
}
```

#### 🔴 Step 2 : UnrefactoredAreaCalculator 수정 ⚠️ 기존 코드 변경!

```csharp
public class UnrefactoredAreaCalculator
{
    // 기존 메서드들
    public float GetRectangleArea(Rectangle rectangle)
    {
        return rectangle.Width * rectangle.Height;
    }

    public float GetCircleArea(Circle circle)
    {
        return circle.Radius * circle.Radius * Mathf.PI;
    }

    public float GetTriangleArea(Triangle triangle)
    {
        return (Mathf.Sqrt(3) / 4) * triangle.SideLength * triangle.SideLength;
    }

    // ⚠️ 새로운 메서드 추가 (기존 클래스 수정!)
    public float GetPentagonArea(Pentagon pentagon)
    {
        return 1.72f * pentagon.SideLength * pentagon.SideLength;
    }

    // Hexagon, Heptagon, Octagon... 계속 추가하면?
    // 이 클래스는 계속 커지고 복잡해짐!
}
```

#### 🔴 Step 3 : 사용하는 곳도 수정 ⚠️ 또 수정!

```csharp
public class GameManager
{
    UnrefactoredAreaCalculator calculator = new UnrefactoredAreaCalculator();

    public void ProcessShape(object shape)
    {
        // ⚠️ 타입 체크 분기문이 계속 늘어남!
        if (shape is Rectangle rect)
        {
            float area = calculator.GetRectangleArea(rect);
            Debug.Log("Rectangle Area : " + area);
        }
        else if (shape is Circle circle)
        {
            float area = calculator.GetCircleArea(circle);
            Debug.Log("Circle Area : " + area);
        }
        else if (shape is Triangle triangle)
        {
            float area = calculator.GetTriangleArea(triangle);
            Debug.Log("Triangle Area : " + area);
        }
        else if (shape is Pentagon pentagon)  // ⚠️ 새로운 분기 추가!
        {
            float area = calculator.GetPentagonArea(pentagon);
            Debug.Log("Pentagon Area : " + area);
        }
        // Hexagon, Heptagon, Octagon 추가하면 또 분기 추가...
        // 10개, 20개로 늘어나면?
        // → 이 메서드는 거대한 if-else 덩어리가 됨!
    }
}
```

#### ⚠️ 문제점 정리

❌ **폐쇄 원칙 위반 (Closed for Modification)**
   - 새 도형 추가할 때마다 `UnrefactoredAreaCalculator` 클래스를 수정해야 함
   - 기존에 잘 작동하던 코드를 건드려야 함
   - 버그 발생 위험 증가!

❌ **개방 원칙 위반 (Open for Extension)**
   - 확장이 어려움 - 메서드를 계속 추가해야 함
   - 타입 체크 분기문이 계속 늘어남
   - 실수로 분기를 빠뜨리면 런타임 버그!

❌ **유지보수성 저하**
   - 도형이 10개, 20개로 늘어나면 코드가 매우 복잡해짐
   - 클래스가 300줄, 500줄로 비대해짐
   - 새 개발자가 이해하기 어려워짐

❌ **테스트 어려움**
   - `UnrefactoredAreaCalculator`를 수정할 때마다 모든 도형을 다시 테스트해야 함
   - 회귀 테스트(Regression Test) 부담 증가
   - 테스트 시간 증가

❌ **협업 충돌**
   - 팀원 A가 Hexagon 추가, 팀원 B가 Octagon 추가
   - 같은 파일(`UnrefactoredAreaCalculator.cs`) 수정 → Git 충돌 발생!

---

### ✅ 방식 2 : AreaOfEffect 패턴 (OCP 준수)

#### 🟢 Step 1 : 오각형 효과 클래스 추가 (새 파일) ✅ 확장!

```csharp
// 📁 새 파일 : PentagonEffect.cs
/// <summary>
/// 정오각형 효과를 표시하는 클래스.
///
/// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
/// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
/// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
/// </summary>
public class PentagonEffect : AreaOfEffect
{
    [Header("Shape")]
    [Tooltip("오각형의 한 변 길이")]
    [SerializeField] private float m_SideLength;

    public override float CalculateArea()
    {
        // 정오각형 면적 공식
        return 1.72f * m_SideLength * m_SideLength;
    }
}
```

#### 🟢 Step 2 : 기존 코드는 전혀 수정하지 않음! ✅ 폐쇄!

```csharp
// AreaOfEffect.cs - 수정 없음! ✅
public abstract class AreaOfEffect : MonoBehaviour
{
    public abstract float CalculateArea();
    public void PlayEffect() { /* ... */ }
    // ... 기타 메서드들
}

// CircleEffect.cs - 수정 없음! ✅
// RectangleEffect.cs - 수정 없음! ✅
// HexagonalEffect.cs - 수정 없음! ✅
// TriangularEffect.cs - 수정 없음! ✅
```

#### 🟢 Step 3 : 사용하는 곳도 수정 불필요! ✅ 폐쇄!

```csharp
// EffectTrigger.cs - 수정 없음! ✅
public class EffectTrigger : MonoBehaviour
{
    // ✅ 어떤 도형이든 AreaOfEffect로 처리!
    [SerializeField] AreaOfEffect m_Effect;

    private void PlayEffect(Collider other)
    {
        // ✅ 분기문 없음! 다형성으로 자동 처리!
        // ✅ PentagonEffect든 HexagonEffect든 동일하게 작동!
        m_Effect.PlayEffect();
        // ↳ PlayEffect() 내부에서 ShowAreaText() 호출
        //   ↳ ShowAreaText() 내부에서 CalculateArea() 호출
        //     → 자동으로 올바른 도형의 CalculateArea() 실행됨!
    }
}
```

**Unity Editor에서 :**
```
Inspector에서 m_Effect에 PentagonEffect를 드래그 앤 드롭만 하면 끝!
코드 수정 없음!
```

#### ✅ 장점 정리

✅ **개방 원칙 준수 (Open for Extension)**
   - 새로운 도형 추가는 자유롭게 가능 (확장에 개방)
   - 새 파일(`PentagonEffect.cs`)만 추가하면 됨
   - 10개, 20개, 100개 도형 추가해도 동일한 방식

✅ **폐쇄 원칙 준수 (Closed for Modification)**
   - 기존 코드는 전혀 수정하지 않음 (수정에 폐쇄)
   - `AreaOfEffect`, `CircleEffect`, `EffectTrigger` 등 모두 안전
   - 버그 발생 위험 최소화

✅ **유지보수성 향상**
   - 각 도형이 독립적으로 관리됨
   - 코드가 깔끔하고 이해하기 쉬움
   - 새 팀원도 쉽게 파악 가능

✅ **테스트 용이**
   - 새로운 `PentagonEffect`만 테스트하면 됨
   - 기존 도형들은 영향받지 않음
   - 회귀 테스트 불필요

✅ **협업 효율**
   - 팀원들이 서로 다른 파일 작업
   - Git 충돌 없음
   - 병렬 개발 가능

---

### 🔥 규모별 비교 시뮬레이션

#### 📊 시나리오 : 10개의 도형을 지원하는 게임 개발

**❌ UnrefactoredAreaCalculator 방식 :**

```csharp
public class UnrefactoredAreaCalculator
{
    // 10개의 메서드가 필요 (클래스가 비대해짐)
    public float GetRectangleArea(Rectangle rectangle) { /*...*/ }
    public float GetCircleArea(Circle circle) { /*...*/ }
    public float GetTriangleArea(Triangle triangle) { /*...*/ }
    public float GetPentagonArea(Pentagon pentagon) { /*...*/ }
    public float GetHexagonArea(Hexagon hexagon) { /*...*/ }
    public float GetHeptagonArea(Heptagon heptagon) { /*...*/ }
    public float GetOctagonArea(Octagon octagon) { /*...*/ }
    public float GetEllipseArea(Ellipse ellipse) { /*...*/ }
    public float GetTrapezoidArea(Trapezoid trapezoid) { /*...*/ }
    public float GetRhombusArea(Rhombus rhombus) { /*...*/ }
}
// ⚠️ 약 300줄 이상의 거대한 클래스!

// 사용하는 쪽
public class ShapeProcessor
{
    UnrefactoredAreaCalculator calculator = new UnrefactoredAreaCalculator();

    public void ProcessShape(object shape)
    {
        // ⚠️ 10개의 if-else 분기!
        if (shape is Rectangle rect)
            float area = calculator.GetRectangleArea(rect);
        else if (shape is Circle circle)
            float area = calculator.GetCircleArea(circle);
        else if (shape is Triangle triangle)
            float area = calculator.GetTriangleArea(triangle);
        else if (shape is Pentagon pentagon)
            float area = calculator.GetPentagonArea(pentagon);
        else if (shape is Hexagon hexagon)
            float area = calculator.GetHexagonArea(hexagon);
        else if (shape is Heptagon heptagon)
            float area = calculator.GetHeptagonArea(heptagon);
        else if (shape is Octagon octagon)
            float area = calculator.GetOctagonArea(octagon);
        else if (shape is Ellipse ellipse)
            float area = calculator.GetEllipseArea(ellipse);
        else if (shape is Trapezoid trapezoid)
            float area = calculator.GetTrapezoidArea(trapezoid);
        else if (shape is Rhombus rhombus)
            float area = calculator.GetRhombusArea(rhombus);

        // ⚠️ 11번째 도형 추가하면?
        // → UnrefactoredAreaCalculator 수정
        // → 여기도 분기 추가
        // → 모든 도형 다시 테스트
    }
}
// ⚠️ 약 100줄의 복잡한 분기문!
```

**문제점 :**
- 🔴 `UnrefactoredAreaCalculator` 클래스가 비대해짐 (300+ 줄)
- 🔴 if-else 분기문이 복잡해짐 (가독성 저하)
- 🔴 새 도형 추가 시 여러 곳을 수정해야 함
- 🔴 실수로 분기를 빠뜨릴 위험
- 🔴 코드 리뷰가 어려워짐
- 🔴 팀원들이 같은 파일을 동시에 수정 → Git 충돌

**✅ AreaOfEffect 패턴 방식 :**

```csharp
// 10개의 독립적인 파일 (각 30-40줄)
// 📁 CircleEffect.cs
public class CircleEffect : AreaOfEffect
{
    [SerializeField] private float m_Radius;
    public override float CalculateArea() { return m_Radius * m_Radius * Mathf.PI; }
}

// 📁 RectangleEffect.cs
public class RectangleEffect : AreaOfEffect
{
    [SerializeField] private float m_Width;
    [SerializeField] private float m_Height;
    public override float CalculateArea() { return m_Width * m_Height; }
}

// 📁 TriangleEffect.cs
public class TriangleEffect : AreaOfEffect
{
    [SerializeField] private float m_SideLength;
    public override float CalculateArea() { return (Mathf.Sqrt(3) / 4) * m_SideLength * m_SideLength; }
}

// ... 나머지 7개 파일도 동일한 패턴 (각각 독립적) ...

// 사용하는 쪽
public class EffectTrigger : MonoBehaviour
{
    // ✅ 단 한 줄! 모든 도형 지원!
    [SerializeField] AreaOfEffect m_Effect;

    private void PlayEffect(Collider other)
    {
        // ✅ 분기문 없음! 다형성이 자동 처리!
        m_Effect.PlayEffect();

        // ✅ 11번째 도형 추가해도 이 코드는 그대로!
        // ✅ 20번째 도형 추가해도 이 코드는 그대로!
        // ✅ 100번째 도형 추가해도 이 코드는 그대로!
    }
}
```

**장점 :**
- 🟢 각 도형이 독립적인 파일 (30-40줄)
- 🟢 분기문 없음 (다형성 활용)
- 🟢 새 도형 추가 시 새 파일만 추가
- 🟢 기존 코드 수정 불필요
- 🟢 코드 리뷰가 쉬움
- 🟢 팀원들이 독립적으로 작업 가능

---

### 📈 코드 규모에 따른 복잡도 비교

```
도형 개수    │ UnrefactoredAreaCalculator │ AreaOfEffect 패턴
────────────┼──────────────────────────────┼──────────────────────
1개         │ 간단 (20줄)                  │ 간단 (30줄)
            │ - 클래스 1개                 │ - 클래스 2개
────────────┼──────────────────────────────┼──────────────────────
3개         │ 보통 (80줄)                  │ 보통 (90줄)
            │ - 클래스 1개                 │ - 클래스 4개
            │ - 메서드 3개                 │ - 파일 4개
────────────┼──────────────────────────────┼──────────────────────
5개         │ 복잡 (150줄)                 │ 간단 (150줄, 6파일)
            │ - 클래스 1개 (비대)          │ - 각 클래스 25줄
            │ - 메서드 5개                 │ - 독립적 관리
────────────┼──────────────────────────────┼──────────────────────
10개        │ 매우 복잡 (350줄)            │ 보통 (300줄, 11파일)
            │ - 클래스 1개 (거대)          │ - 각 클래스 27줄
            │ - 메서드 10개                │ - 여전히 명확
            │ - 분기문 10개                │ - 분기문 0개
────────────┼──────────────────────────────┼──────────────────────
20개        │ 거의 불가능 (800줄)          │ 관리 가능 (600줄, 21파일)
            │ - 클래스 1개 (괴물)          │ - 각 클래스 28줄
            │ - 메서드 20개                │ - 구조 일관성 유지
            │ - 분기문 20개                │ - 분기문 0개
            │ - 유지보수 불가              │ - 여전히 쉬움
────────────┼──────────────────────────────┼──────────────────────
50개        │ 완전히 불가능                │ 여전히 관리 가능
            │ - 2000줄 이상                │ - 1500줄, 51파일
            │ - 읽을 수 없는 코드          │ - 각 클래스 29줄
            │ - 버그 온상                  │ - 구조 일관성 유지
```

**핵심 :**
- 🔴 도형이 늘어날수록 UnrefactoredAreaCalculator는 **지수적으로** 복잡해짐
- 🟢 AreaOfEffect 패턴은 **선형적으로만** 증가함
- 🟢 도형이 100개가 되어도 각 클래스는 여전히 30줄 내외!

---

### 🔥 핵심 비유 : 레스토랑 주방

**❌ UnrefactoredAreaCalculator = 만능 셰프 한 명**

```
                 만능 셰프 (UnrefactoredAreaCalculator)
                          |
    +---------+-----------+-----------+---------+---------+
    |         |           |           |         |         |
  피자     스파게티    스테이크    샐러드   디저트   음료

고객 : "피자 주세요"
셰프 : (if 피자면) { 피자 만드는 방법... }

고객 : "스파게티 주세요"
셰프 : (else if 스파게티면) { 스파게티 만드는 방법... }

고객 : "스테이크 주세요"
셰프 : (else if 스테이크면) { 스테이크 만드는 방법... }

⚠️ 새 메뉴(라자냐) 추가하려면?
- 셰프의 레시피북(UnrefactoredAreaCalculator 클래스)을 수정해야 함
- 셰프가 모든 요리법을 알아야 함 (클래스가 비대해짐)
- 셰프가 실수하면 모든 요리가 영향받음 (버그 위험)
- 새 요리 추가할 때마다 기존 레시피북 수정 (수정에 개방)
- 레시피북이 100페이지, 200페이지로 늘어남 (유지보수 불가)
```

**✅ AreaOfEffect 패턴 = 전문 셰프 여러 명**

```
              주방장 (AreaOfEffect 추상 클래스)
                  "요리 담당자를 배정합니다"
                          |
    +---------+-----------+-----------+---------+---------+
    |         |           |           |         |         |
  피자      스파게티    스테이크    샐러드   디저트
  셰프      셰프        셰프        셰프     셰프
  (Circle)  (Rectangle) (Triangle)  (Hexagon) (Pentagon)

고객 : "피자 주세요"
주방장 : "피자 셰프님, 요리해 주세요" ← 위임!
피자 셰프 : "네!" → 피자 전문 조리

고객 : "스파게티 주세요"
주방장 : "스파게티 셰프님, 요리해 주세요" ← 위임!
스파게티 셰프 : "네!" → 스파게티 전문 조리

✅ 새 메뉴(라자냐) 추가하려면?
- 새로운 라자냐 전문 셰프만 고용하면 됨 (새 클래스 추가)
- 기존 셰프들은 영향받지 않음 (기존 코드 수정 없음)
- 각 셰프는 자기 요리만 잘하면 됨 (단일 책임)
- 주방장은 "요리해 주세요"만 말하면 됨 (다형성)
- 100명의 셰프가 있어도 주방장 업무는 동일 (확장성)
```

---

## 💻 주요 코드 분석

### 📌 핵심 1 : 추상 메서드 정의

**AreaOfEffect.cs :**
```csharp
public abstract class AreaOfEffect : MonoBehaviour
{
    /// <summary>
    /// 각 AreaOfEffect 서브클래스는 고유한 CalculateArea 정의를 구현합니다.
    /// </summary>
    public abstract float CalculateArea();  // ← 추상 메서드!

    public void PlayEffect()
    {
        // ...
        ShowAreaText();  // ← 내부에서 호출
        // ...
    }

    public void ShowAreaText()
    {
        // ...
        float area = CalculateArea();  // ← 다형성으로 자동 호출!
        // ...
    }
}
```

**효과 :**
- ✅ 모든 서브클래스가 `CalculateArea()`를 반드시 구현해야 함
- ✅ 컴파일 타임에 강제 (실수 방지)
- ✅ 인터페이스 일관성 보장

**📞 호출 흐름 :**
```
EffectTrigger.PlayEffect(other)
    ↓
m_Effect.PlayEffect()  ← AreaOfEffect의 메서드
    ↓
ShowAreaText()  ← AreaOfEffect의 메서드
    ↓
CalculateArea()  ← 추상 메서드! 각 서브클래스에서 구현됨
    ↓
실제 구현 메서드 실행 :
- CircleEffect.CalculateArea() 또는
- RectangleEffect.CalculateArea() 또는
- HexagonEffect.CalculateArea() 등
```

---

### 📌 핵심 2 : 다형성 활용

**사용하는 쪽 (EffectTrigger.cs) :**
```csharp
public class EffectTrigger : MonoBehaviour
{
    // ✅ 추상 타입으로 선언!
    [SerializeField] AreaOfEffect m_Effect;

    private void PlayEffect(Collider other)
    {
        // ✅ 런타임에 실제 타입 결정!
        m_Effect.PlayEffect();
        // ↳ PlayEffect() 내부에서 ShowAreaText() 호출
        //   ↳ ShowAreaText() 내부에서 CalculateArea() 호출
        //     → m_Effect가 CircleEffect면 → CircleEffect.CalculateArea()
        //     → m_Effect가 RectangleEffect면 → RectangleEffect.CalculateArea()
        //     → m_Effect가 HexagonEffect면 → HexagonEffect.CalculateArea()
    }
}
```

**다형성의 마법 :**
```csharp
// Unity Inspector에서 할당
EffectTrigger.m_Effect = CircleEffect 인스턴스
→ m_Effect.PlayEffect() 호출 시
  ↳ PlayEffect() → ShowAreaText() → CalculateArea()
    → CircleEffect.CalculateArea() 자동 호출!

EffectTrigger.m_Effect = HexagonEffect 인스턴스
→ m_Effect.PlayEffect() 호출 시
  ↳ PlayEffect() → ShowAreaText() → CalculateArea()
    → HexagonEffect.CalculateArea() 자동 호출!

// ✅ 코드 수정 없이 동작 변경!
// ✅ 런타임에 동적으로 결정!
// ✅ PlayEffect() → ShowAreaText() → CalculateArea() 호출 체인!
```

---

### 📌 핵심 3 : 확장의 용이성

**새 도형 추가 프로세스 :**

```csharp
// 1단계 : 새 파일 생성 (DiamondEffect.cs)
// ✅ 기존 코드 수정 없음!
public class DiamondEffect : AreaOfEffect
{
    [SerializeField] private float m_Diagonal1;
    [SerializeField] private float m_Diagonal2;

    public override float CalculateArea()
    {
        return (m_Diagonal1 * m_Diagonal2) / 2f;
    }
}

// 2단계 : 끝!
// ✅ AreaOfEffect.cs 수정 안 함
// ✅ CircleEffect.cs 수정 안 함
// ✅ EffectTrigger.cs 수정 안 함
// ✅ Unity Inspector에서 DiamondEffect 할당만 하면 작동!
```

**추가 작업 :**
- 코드 수정 : 0곳
- 새 파일 생성 : 1개
- 테스트 : DiamondEffect만
- 배포 : 안전 (기존 코드 영향 없음)

---

### 📌 핵심 4 : 추상화에 의존

**의존성 방향 :**

```
Before (나쁜 설계) :

GameManager
    |
    ├─ depends on → Rectangle (구체 클래스)
    ├─ depends on → Circle (구체 클래스)
    ├─ depends on → Triangle (구체 클래스)
    └─ depends on → Hexagon (구체 클래스)

⚠️ 구체 클래스가 추가될 때마다 GameManager 수정 필요!


After (좋은 설계) :

EffectTrigger
    |
    └─ depends on → AreaOfEffect (추상 클래스)
                         |
         +---------------+---------------+---------------+
         |               |               |               |
    CircleEffect   RectangleEffect  HexagonEffect  TriangleEffect
    (구체 클래스)   (구체 클래스)    (구체 클래스)   (구체 클래스)

✅ EffectTrigger는 추상화(AreaOfEffect)에만 의존!
✅ 구체 클래스가 추가되어도 EffectTrigger는 수정 불필요!
```

**DIP(Dependency Inversion Principle)와의 연관성 :**
- 고수준 모듈(`EffectTrigger`)이 저수준 모듈(`CircleEffect`)에 의존하지 않음
- 둘 다 추상화(`AreaOfEffect`)에 의존
- 이것이 SOLID의 D(DIP)와 연결됨!

---

## ⚖️ 장단점

### ✅ 장점

**1. 확장 용이 (Open for Extension)**
- 새로운 도형을 자유롭게 추가 가능
- 새 파일만 생성하면 됨
- 무한 확장 가능

**2. 기존 코드 보호 (Closed for Modification)**
- 기존 코드를 수정하지 않음
- 버그 발생 위험 최소화
- 안정성 보장

**3. 유지보수 용이**
- 각 도형이 독립적으로 관리
- 코드 위치가 명확
- 수정 영향 범위가 한정적

**4. 테스트 용이**
- 새 도형만 테스트하면 됨
- 회귀 테스트 불필요
- 단위 테스트 작성 쉬움

**5. 협업 효율 향상**
- 팀원들이 독립적으로 작업
- Git 충돌 최소화
- 병렬 개발 가능

**6. 코드 가독성 향상**
- 각 클래스가 짧고 명확
- 분기문 없음
- 이해하기 쉬움

**7. 다형성 활용**
- 런타임에 동작 결정
- 유연한 설계
- 코드 재사용성 증가

### ❌ 단점

**1. 초기 설계 복잡도**
- 추상화 설계 필요
- 초기 파일 수 증가
- 학습 곡선 존재

**2. 간단한 기능에는 과도함**
- 도형이 1~2개만 있을 때는 오버엔지니어링
- 초기 개발 시간 증가

**3. 간접 참조 오버헤드**
- 추상 메서드 호출 (가상 함수)
- 약간의 성능 오버헤드 (미미함)

**4. 디버깅 복잡도**
- 런타임에 실제 타입 확인 필요
- 호출 스택이 길어질 수 있음

---

## 🎮 실제 적용 사례

### 1️⃣ 게임 개발

**무기 시스템**
```csharp
// ✅ OCP 적용
public abstract class Weapon
{
    public abstract void Attack();
    public abstract float GetDamage();
}

public class Sword : Weapon
{
    public override void Attack() { /* 근접 공격 */ }
    public override float GetDamage() { return 10f; }
}

public class Bow : Weapon
{
    public override void Attack() { /* 원거리 공격 */ }
    public override float GetDamage() { return 8f; }
}

public class MagicStaff : Weapon  // ✅ 새 무기 추가 (기존 코드 수정 없음!)
{
    public override void Attack() { /* 마법 공격 */ }
    public override float GetDamage() { return 15f; }
}

public class Player
{
    private Weapon currentWeapon;  // ✅ 추상화에 의존

    public void Attack()
    {
        currentWeapon.Attack();  // ✅ 다형성으로 자동 처리
    }
}
```

**적 AI 시스템**
```csharp
// ✅ OCP 적용
public abstract class EnemyAI
{
    public abstract void DecideAction();
}

public class AggressiveAI : EnemyAI
{
    public override void DecideAction() { /* 공격적 행동 */ }
}

public class DefensiveAI : EnemyAI
{
    public override void DecideAction() { /* 방어적 행동 */ }
}

public class SneakyAI : EnemyAI  // ✅ 새 AI 추가
{
    public override void DecideAction() { /* 은밀한 행동 */ }
}
```

### 2️⃣ UI 시스템

```csharp
// ✅ OCP 적용
public abstract class UIPanel
{
    public abstract void Show();
    public abstract void Hide();
}

public class InventoryPanel : UIPanel
{
    public override void Show() { /* 인벤토리 표시 */ }
    public override void Hide() { /* 인벤토리 숨김 */ }
}

public class SkillTreePanel : UIPanel
{
    public override void Show() { /* 스킬 트리 표시 */ }
    public override void Hide() { /* 스킬 트리 숨김 */ }
}

public class UIManager
{
    private UIPanel currentPanel;  // ✅ 추상화에 의존

    public void OpenPanel(UIPanel panel)
    {
        currentPanel?.Hide();
        currentPanel = panel;
        currentPanel.Show();  // ✅ 어떤 패널이든 작동
    }
}
```

### 3️⃣ 저장 시스템

```csharp
// ✅ OCP 적용
public abstract class SaveSystem
{
    public abstract void Save(GameData data);
    public abstract GameData Load();
}

public class LocalSaveSystem : SaveSystem
{
    public override void Save(GameData data) { /* 로컬 저장 */ }
    public override GameData Load() { /* 로컬 로드 */ }
}

public class CloudSaveSystem : SaveSystem
{
    public override void Save(GameData data) { /* 클라우드 저장 */ }
    public override GameData Load() { /* 클라우드 로드 */ }
}

public class PlayerPrefsSaveSystem : SaveSystem  // ✅ 새 저장 방식 추가
{
    public override void Save(GameData data) { /* PlayerPrefs 저장 */ }
    public override GameData Load() { /* PlayerPrefs 로드 */ }
}

public class GameManager
{
    private SaveSystem saveSystem;  // ✅ 추상화에 의존

    public void SaveGame(GameData data)
    {
        saveSystem.Save(data);  // ✅ 어떤 저장 방식이든 작동
    }
}
```

---

## 📝 학습 정리

### 핵심 요약

1. **개방-폐쇄 원칙 (OCP)**
   - 확장에는 열려 있고 (Open for Extension)
   - 수정에는 닫혀 있어야 한다 (Closed for Modification)

2. **핵심 메커니즘**
   - 추상화(추상 클래스/인터페이스)로 공통 동작 정의
   - 구체 클래스들이 이를 상속/구현
   - 새 기능은 새 구체 클래스로 추가
   - 기존 코드는 추상화에만 의존

3. **실전 적용 방법**
   - 변경 가능성이 있는 부분을 추상화
   - 다형성을 활용한 설계
   - 의존성을 추상화로 향하게 함

4. **장점**
   - 확장 용이, 기존 코드 보호, 유지보수성, 테스트 용이성
   - 협업 효율, 가독성 향상

5. **주의사항**
   - 과도한 추상화는 오버엔지니어링
   - 변경 가능성이 낮은 부분까지 추상화할 필요 없음

### Before vs After

| 항목 | Before (UnrefactoredAreaCalculator) | After (AreaOfEffect 패턴) |
|------|-------------------------------------|---------------------------|
| **구조** | 1개 거대 클래스 | 추상 클래스 + 구체 클래스들 |
| **확장** | 기존 클래스 수정 | 새 클래스 추가 |
| **수정** | 기존 코드 변경 | 기존 코드 보호 |
| **분기문** | 타입 체크 분기 증가 | 분기문 없음 (다형성) |
| **테스트** | 전체 테스트 | 새 클래스만 테스트 |
| **버그 위험** | 높음 | 낮음 |
| **확장성** | 매우 낮음 | 매우 높음 |
| **협업** | 충돌 위험 | 독립 작업 가능 |

### 실무 적용 팁

✅ **이런 경우 OCP 적용 고려**
- 새로운 타입이 계속 추가될 가능성이 있을 때
- if-else 또는 switch-case 분기가 많을 때
- 타입 체크 코드가 여러 곳에 흩어져 있을 때
- 팀원들이 동시에 새 기능을 추가할 때

❌ **이런 경우 무리하게 적용 X**
- 변경 가능성이 거의 없을 때
- 타입이 1~2개로 고정되어 있을 때
- 프로토타입 단계
- 간단한 일회성 코드

### OCP와 다른 원칙의 관계

**SRP (Single Responsibility Principle)와의 관계 :**
- OCP를 지키면 자연스럽게 SRP도 지켜짐
- 각 구체 클래스는 하나의 책임만 가짐

**LSP (Liskov Substitution Principle)와의 관계 :**
- OCP를 위해서는 LSP가 필수
- 서브클래스가 부모 클래스를 완벽히 대체할 수 있어야 함

**DIP (Dependency Inversion Principle)와의 관계 :**
- OCP 구현의 핵심은 DIP
- 추상화에 의존함으로써 OCP 달성

### 핵심 문구

```
"확장에는 열려 있고, 수정에는 닫혀 있다"

확장 (Extension) : 새로운 기능을 자유롭게 추가할 수 있다
수정 (Modification) : 기존 코드를 변경하지 않는다

→ 새 파일 추가 OK, 기존 파일 수정 NO!
```

### 마무리

**개방-폐쇄 원칙의 진정한 가치 :**

처음 3개 도형만 만들 때는 `UnrefactoredAreaCalculator`가 더 간단해 보입니다.

하지만 프로젝트가 성장하면서 10개, 20개, 50개로 늘어날 때,
`AreaOfEffect` 패턴의 진가가 드러납니다.

**단기적으로는** 코드가 조금 더 복잡해 보일 수 있지만,
**장기적으로는** 유지보수와 확장을 **극적으로** 쉽게 만듭니다!

이것이 바로 **개방-폐쇄 원칙**의 힘입니다! 🚀
