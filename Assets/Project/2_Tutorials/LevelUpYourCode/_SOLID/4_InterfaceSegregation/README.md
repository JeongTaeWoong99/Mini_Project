# 🔌 Interface Segregation Principle (인터페이스 분리 원칙)

## 📋 목차
- [풀어서 설명](#-풀어서-설명)
- [원칙 개요](#-원칙-개요)
- [왜 ISP가 필요한가?](#-왜-isp가-필요한가)
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
클라이언트(사용자)에게 사용하지 않는 메서드를 강요하지 말아야 한다.
큰 인터페이스 하나보다, 작은 인터페이스 여러 개가 낫다.
```

### 실생활 비유

**❌ 나쁜 예 : 복합기 전체 기능을 배워야 함**
```
프린터만 쓰고 싶은데...
- 프린터 기능 배우기 ✅
- 스캔 기능 배우기 ❌ (안 쓰는데 배워야 함)
- 팩스 기능 배우기 ❌ (안 쓰는데 배워야 함)
- 복사 기능 배우기 ❌ (안 쓰는데 배워야 함)

문제점 :
→ 필요 없는 것까지 배워야 함!
→ 비효율적이고 복잡함!
```

**✅ 좋은 예 : 필요한 기능만 배움**
```
프린터만 쓰고 싶으면...
- 프린터 기능만 배우기 ✅

스캔만 쓰고 싶으면...
- 스캔 기능만 배우기 ✅

장점 :
→ 필요한 것만 배우면 됨!
→ 효율적이고 간단함!
```

### 판단 기준

**"사용하지 않는 메서드를 구현하고 있는가?"**

- ✅ **모든 메서드 사용** → ISP 준수! 잘 설계됨!
- ❌ **빈 메서드/예외 던짐** → ISP 위반! 인터페이스 분리 필요!

```csharp
// ❌ ISP 위반 징후
public class SimpleTarget : ITarget
{
    public void TakeDamage(int amount) { /* 구현 */ }
    public void Explode()              { /* 사용 안 함! 빈 구현! */ }  // ❌
    public void TriggerEffect()        { /* 사용 안 함! 빈 구현! */ }  // ❌
}

// ✅ ISP 준수
public class SimpleTarget : IDamageable
{
    public void TakeDamage(float amount) { /* 구현 */ }
    // Explode(), TriggerEffect() 구현 안 해도 됨! ✅
}
```

---

## 🎯 원칙 개요

**Interface Segregation Principle (ISP)** 은 **SOLID 원칙**의 네 번째 원칙으로, 클라이언트는 자신이 사용하지 않는 메서드에 의존하지 않아야 한다는 원칙입니다.

### 📌 핵심 개념

```
인터페이스를 작게 유지하라.
클라이언트는 필요한 것만 구현하면 된다.
```

**잘못된 설계 :**
```csharp
// ❌ 큰 인터페이스 하나
public interface ITarget
{
    void TakeDamage(int amount);
    void Explode();           // 모든 타겟이 폭발하는 건 아님!
    void TriggerEffect();     // 모든 타겟이 이펙트를 가진 건 아님!
}

public class SimpleTarget : ITarget
{
    public void TakeDamage(int amount) { /* 데미지 처리 */ }
    public void Explode() { }          // ❌ 강제 구현! (사용 안 함)
    public void TriggerEffect() { }    // ❌ 강제 구현! (사용 안 함)
}
// 문제 : SimpleTarget이 사용하지 않는 메서드를 구현해야 함!
```

**ISP 적용 :**
```csharp
// ✅ 작은 인터페이스 여러 개
public interface IDamageable
{
    void TakeDamage(float amount);
}

public interface IExplodable
{
    void Explode();
}

public interface IEffectTrigger
{
    void TriggerEffect(Vector3 position);
}

// ✅ 필요한 인터페이스만 구현
public class SimpleTarget : IDamageable
{
    public void TakeDamage(float amount) { /* 데미지 처리 */ }
    // ✅ Explode(), TriggerEffect() 구현 안 해도 됨!
}

public class ExplodableTarget : IDamageable, IExplodable
{
    public void TakeDamage(float amount) { /* 데미지 처리 */ }
    public void Explode()                { /* 폭발 처리 */ }
    // ✅ 필요한 것만 구현!
}
```

---

## 🤔 왜 ISP가 필요한가?

### 문제 상황

게임에서 타겟 시스템을 만들 때, 큰 인터페이스를 잘못 설계한 경우 :

```csharp
// 큰 인터페이스 : 모든 기능 포함
public interface ITarget
{
    void TakeDamage(int amount);
    void Explode();
    void TriggerEffect();
}

// 단순한 타겟 : 데미지만 받음
public class SimpleTarget : ITarget
{
    public void TakeDamage(int amount)
    {
        // 데미지 로직 구현
    }

    public void Explode()
    {
        // ❌ 폭발하지 않는데 구현해야 함!
        // 빈 메서드 또는 예외 던짐
    }

    public void TriggerEffect()
    {
        // ❌ 이펙트 없는데 구현해야 함!
        // 빈 메서드 또는 예외 던짐
    }
}
```

**이 코드의 문제점 :**

❌ **강제 구현**
   - 사용하지 않는 메서드를 구현해야 함
   - 빈 메서드가 생김

❌ **인터페이스 오염**
   - 클라이언트가 필요 없는 의존성을 가짐
   - 인터페이스가 비대해짐

❌ **유지보수 어려움**
   - ITarget 변경 시 모든 구현체가 영향받음
   - 실제로 해당 기능을 사용하지 않는데도!

❌ **혼란스러운 설계**
   - 어떤 타겟이 어떤 기능을 가지는지 불명확
   - 빈 메서드가 많아지면 의도 파악 어려움

### ISP의 해결책

✅ **작은 인터페이스**
   - 클라이언트가 필요한 것만 구현

✅ **명확한 의도**
   - 어떤 기능을 가지는지 명확

✅ **독립적 변경**
   - 인터페이스 변경이 다른 클라이언트에 영향 없음

✅ **조합 가능**
   - 여러 작은 인터페이스를 조합하여 사용

---

## 🏗️ 핵심 개념

ISP를 이해하기 위한 핵심 구조 :

### 📐 인터페이스 분리

**핵심 아이디어 :**
```
1. 큰 인터페이스를 작은 단위로 분리
2. 각 인터페이스는 하나의 책임만 가짐
3. 클라이언트는 필요한 인터페이스만 구현
4. 여러 인터페이스 조합 가능
```

**구조 :**
```csharp
// ❌ Before : 큰 인터페이스
public interface ITarget
{
    void TakeDamage(int amount);     // 모든 타겟이 필요
    void Explode();                  // 일부 타겟만 필요
    void TriggerEffect();            // 일부 타겟만 필요
}

// ✅ After : 작은 인터페이스들
public interface IDamageable
{
    void TakeDamage(float amount);   // 데미지 받는 것만
}

public interface IExplodable
{
    void Explode();                  // 폭발하는 것만
}

public interface IEffectTrigger
{
    void TriggerEffect(Vector3 position);  // 이펙트만
}

// 사용 예시
public class SimpleTarget : Health, IDamageable
{
    // 데미지만 구현
}

public class ExplodableTarget : Target, IExplodable
{
    // 데미지 + 폭발 구현
}

public class HitEffect : MonoBehaviour, IEffectTrigger
{
    // 이펙트만 구현
}
```

---

## 📊 코드 구조

### 폴더 구조

```
4_InterfaceSegregation/
├── Scripts/
│   ├── Interfaces/                    ← ✅ 작은 인터페이스들
│   │   ├── IDamageable.cs             (데미지 받기)
│   │   ├── IExplodable.cs             (폭발)
│   │   └── IEffectTrigger.cs          (이펙트 트리거)
│   │
│   ├── Target.cs                      ← 기본 타겟 (Health 상속)
│   ├── ExplodableTarget.cs            ← 폭발 타겟
│   ├── HitEffect.cs                   ← 히트 이펙트
│   ├── Projectile.cs                  ← 발사체
│   ├── TurretGun.cs                   ← 터렛 건
│   ├── MouseToWorldPosition.cs        ← 마우스 위치 변환
│   ├── TargetShatter.cs               ← 파편 효과
│   │
│   └── Unrefactored/                  ← ❌ ISP 위반 예시
│       └── UnrefactoredTarget.cs      (큰 인터페이스 ITarget)
│
└── README.md                           ← 📍 현재 문서
```

### 클래스 다이어그램

```
═══════════════════════════════════════════════════════════════
❌ ISP 위반 (Unrefactored)
═══════════════════════════════════════════════════════════════

                    ITarget (큰 인터페이스)
        ┌────────────────────────────────────────┐
        │ + TakeDamage(int amount)               │
        │ + Explode()                            │
        │ + TriggerEffect()                      │
        └────────────────────────────────────────┘
                         △
                         │ implements
                         │
              UnrefactoredTarget
        ┌────────────────────────────────────────────────┐
        │ + TakeDamage(int amount) { /* 구현 */ }         │
        │ + Explode()              { /* 빈 구현 */ }  ❌  │
        │ + TriggerEffect()        { /* 빈 구현 */ }  ❌  │
        └────────────────────────────────────────────────┘
        ⚠️ 사용하지 않는 메서드를 강제로 구현!

═══════════════════════════════════════════════════════════════
✅ ISP 준수 (Refactored)
═══════════════════════════════════════════════════════════════

    IDamageable      IExplodable      IEffectTrigger
    ┌──────────┐    ┌──────────┐    ┌─────────────────┐
    │TakeDamage│    │Explode() │    │TriggerEffect()  │
    └──────────┘    └──────────┘    └─────────────────┘
         △               △                  △
         │               │                  │
         │               │                  │
    ┌────┴────┐     ┌───┴───┐         ┌───┴────┐
    │         │     │       │         │        │
  Target  ExplodableTarget  │    HitEffect     │
  (Health)    (Target +     │  (MonoBehaviour) │
              IExplodable)  │                  │
                            │                  │
                       (조합 가능)

✅ 각 클래스가 필요한 인터페이스만 구현!
✅ 빈 메서드 없음!
✅ 명확한 책임!

═══════════════════════════════════════════════════════════════
🎯 Projectile의 인터페이스 활용
═══════════════════════════════════════════════════════════════

                    Projectile
        ┌────────────────────────────────────┐
        │ OnCollisionEnter(Collision)        │
        │   ↓                                │
        │ CheckCollisionInterfaces()         │
        └────────────────────────────────────┘
                    ↓
        ┌───────────┴───────────┐
        ↓                       ↓
  HandleDamageableInterface  HandleEffectTriggerInterface
        ↓                       ↓
  if (IDamageable)          if (IEffectTrigger)
    TakeDamage()              TriggerEffect()

✅ 인터페이스를 통한 유연한 상호작용!
✅ 구체 클래스에 의존하지 않음!
```

---

## 🔄 Before & After 비교

### ❌ Before : ISP 미적용 (UnrefactoredTarget.cs)

```csharp
// 큰 인터페이스
public interface ITarget
{
    void TakeDamage(int amount);
    void Explode();
    void TriggerEffect();
}

/// <summary>
/// 이 클래스는 ITarget 인터페이스를 구현하며, 데미지 받기, 폭발, 이펙트 트리거 메서드를 포함합니다.
///
/// 단순한 타겟이 데미지만 받으면 되는 경우에도, ITarget 인터페이스에 정의된 모든 메서드를 구현해야 합니다.
/// 이는 빈 메서드 구현을 야기합니다.
/// </summary>
public class UnrefactoredTarget : MonoBehaviour, ITarget
{
    // 이 타겟이 데미지만 받으면 되는 경우에도, 모든 메서드를 구현해야 합니다.
    public void TakeDamage(int amount)
    {
        // 데미지 로직 구현
    }

    public void Explode()
    {
        // ❌ 이 타겟이 폭발할 필요가 없더라도, 이 메서드를 구현해야 합니다.
    }

    public void TriggerEffect()
    {
        // ❌ 마찬가지로, 불필요하더라도 구현이 필요합니다.
    }
}
```

**문제점 :**
- 🔴 사용하지 않는 `Explode()`, `TriggerEffect()` 메서드를 강제로 구현
- 🔴 빈 메서드가 생김 (코드 냄새)
- 🔴 인터페이스 변경 시 모든 구현체가 영향받음
- 🔴 클라이언트가 필요 없는 의존성을 가짐

---

### ✅ After : ISP 적용

#### 1️⃣ 작은 인터페이스들

**IDamageable.cs :**
```csharp
/// <summary>
/// 데미지를 받을 수 있는 객체에 대한 계약을 정의합니다.
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 지정된 양만큼 데미지를 받습니다.
    /// </summary>
    /// <param name="amount">받을 데미지 양</param>
    void TakeDamage(float amount);
}
```

**IExplodable.cs :**
```csharp
/// <summary>
/// 폭발할 수 있는 객체에 대한 계약을 정의합니다.
/// </summary>
public interface IExplodable
{
    /// <summary>
    /// 폭발을 트리거합니다 (예 : 파티클 또는 다른 GameObject 이펙트)
    /// </summary>
    void Explode();
}
```

**IEffectTrigger.cs :**
```csharp
/// <summary>
/// 특정 위치에서 파티클 시스템이나 사운드 이펙트 같은 효과를 트리거하기 위한 계약을 정의합니다.
/// </summary>
public interface IEffectTrigger
{
    /// <summary>
    /// 지정된 위치에서 이펙트를 트리거합니다.
    /// </summary>
    /// <param name="position">이펙트를 트리거할 위치</param>
    void TriggerEffect(Vector3 position);
}
```

---

#### 2️⃣ Target.cs (IDamageable만 구현)

```csharp
/// <summary>
/// 게임 내 타겟의 기본 클래스로, 체력과 데미지 시스템을 포함합니다.
/// </summary>
public class Target : Health, IDamageable
{
    [Tooltip("이 타겟의 데미지 배율 커스터마이징")]
    [SerializeField] float m_DamageMultiplier = 1f;

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount * m_DamageMultiplier);
        // 추가적인 클래스별 로직을 여기에 커스터마이징
    }
    // ✅ Explode(), TriggerEffect() 구현 안 해도 됨!
}
```

---

#### 3️⃣ ExplodableTarget.cs (IDamageable + IExplodable)

```csharp
/// <summary>
/// 폭발할 수 있고 죽을 때 이펙트를 생성하는 대체 타입의 타겟입니다. 여기서는
/// 기본 Target을 상속하고 IExplodable 인터페이스를 추가합니다.
/// </summary>
public class ExplodableTarget : Target, IExplodable
{
    [Tooltip("폭발 시 인스턴스화할 이펙트")]
    [SerializeField] GameObject m_ExplosionPrefab;

    protected override void Die()
    {
        base.Die();
        Explode();  // ✅ IExplodable 구현
    }

    public void Explode()
    {
        if (m_ExplosionPrefab)
        {
            GameObject instance = Instantiate(m_ExplosionPrefab, transform.position, quaternion.identity);
        }
        // 커스텀 폭발 로직을 여기에 추가
    }
}
```

---

#### 4️⃣ HitEffect.cs (IEffectTrigger만 구현)

```csharp
/// <summary>
/// 발사체가 표면에 충돌할 때의 이펙트 트리거를 구현합니다. 인터페이스 분리 원칙은
/// 더 작고 클라이언트별 인터페이스를 권장합니다.
/// </summary>
public class HitEffect : MonoBehaviour, IEffectTrigger
{
    [SerializeField] private ParticleSystem m_ParticleSystem;

    public void TriggerEffect(Vector3 position)
    {
        // 파티클 시스템이 null이 아니면 이펙트 재생
        if (m_ParticleSystem != null)
        {
            m_ParticleSystem.transform.position = position;
            // 겹치는 이펙트를 방지하기 위해 다시 재생하기 전에 파티클 시스템 정지
            m_ParticleSystem.Stop();
            m_ParticleSystem.Play();
        }
    }
    // ✅ TakeDamage(), Explode() 구현 안 해도 됨!
}
```

---

#### 5️⃣ Projectile.cs (인터페이스 활용)

```csharp
public class Projectile : MonoBehaviour
{
    [SerializeField] private int m_DamageValue = 5;
    [SerializeField] private float m_Lifetime = 3f;

    private void OnCollisionEnter(Collision collision)
    {
        CheckCollisionInterfaces(collision);
        DeactivateProjectile();
    }

    private void CheckCollisionInterfaces(Collision collision)
    {
        // 첫 번째 접촉점 가져오기
        ContactPoint contactPoint = collision.GetContact(0);

        // 표면 밖으로 이동하기 위한 약간의 오프셋
        float pushDistance = 0.1f;
        Vector3 offsetPosition = contactPoint.point + contactPoint.normal * pushDistance;

        var monoBehaviours = collision.gameObject.GetComponents<MonoBehaviour>();
        foreach (var monoBehaviour in monoBehaviours)
        {
            HandleDamageableInterface(monoBehaviour);         // ✅ IDamageable 처리
            HandleEffectTriggerInterface(monoBehaviour, offsetPosition);  // ✅ IEffectTrigger 처리
        }
    }

    private void HandleDamageableInterface(MonoBehaviour monoBehaviour)
    {
        // ✅ IDamageable 인터페이스를 가진 컴포넌트 처리
        if (monoBehaviour is IDamageable damageable)
        {
            damageable.TakeDamage(m_DamageValue);
        }
    }

    private void HandleEffectTriggerInterface(MonoBehaviour monoBehaviour, Vector3 position)
    {
        // ✅ IEffectTrigger 인터페이스를 가진 컴포넌트 처리
        if (monoBehaviour is IEffectTrigger effectTrigger)
        {
            effectTrigger.TriggerEffect(position);
        }
    }
}
```

**핵심 :**
- ✅ 작은 인터페이스들을 독립적으로 체크
- ✅ 각 인터페이스를 가진 컴포넌트만 처리
- ✅ 유연하고 확장 가능한 구조

---

### 📊 개선 효과

| 항목 | Before (UnrefactoredTarget) | After (ISP 적용) |
|------|----------------------------|------------------|
| **인터페이스 크기** | 1개 (큰 인터페이스) | 3개 (작은 인터페이스) |
| **빈 메서드** | 🔴 많음 | 🟢 없음 |
| **강제 구현** | 🔴 필요 | 🟢 불필요 |
| **의존성** | 🔴 불필요한 것 포함 | 🟢 필요한 것만 |
| **유연성** | 🔴 낮음 | 🟢 높음 |
| **확장성** | 🔴 어려움 | 🟢 쉬움 |
| **명확성** | 🔴 불명확 | 🟢 명확 |

---

## 💻 주요 코드 분석

### 📌 핵심 1 : 인터페이스 분리

**Before (큰 인터페이스) :**
```csharp
// ❌ 모든 기능을 하나의 인터페이스에
public interface ITarget
{
    void TakeDamage(int amount);     // 모든 타겟 필요
    void Explode();                  // 일부만 필요
    void TriggerEffect();            // 일부만 필요
}
// 문제 : 사용하지 않는 메서드도 구현해야 함
```

**After (작은 인터페이스들) :**
```csharp
// ✅ 각 기능을 독립적인 인터페이스로
public interface IDamageable
{
    void TakeDamage(float amount);   // 데미지만
}

public interface IExplodable
{
    void Explode();                  // 폭발만
}

public interface IEffectTrigger
{
    void TriggerEffect(Vector3 position);  // 이펙트만
}
// 장점 : 필요한 것만 구현!
```

---

### 📌 핵심 2 : 선택적 구현

```csharp
// ✅ Target : IDamageable만 구현
public class Target : Health, IDamageable
{
    public override void TakeDamage(float amount) { /* ... */ }
    // Explode(), TriggerEffect() 구현 안 해도 됨!
}

// ✅ ExplodableTarget : IDamageable + IExplodable 구현
public class ExplodableTarget : Target, IExplodable
{
    public override void TakeDamage(float amount) { /* ... */ }
    public void Explode() { /* ... */ }
    // TriggerEffect() 구현 안 해도 됨!
}

// ✅ HitEffect : IEffectTrigger만 구현
public class HitEffect : MonoBehaviour, IEffectTrigger
{
    public void TriggerEffect(Vector3 position) { /* ... */ }
    // TakeDamage(), Explode() 구현 안 해도 됨!
}
```

**효과 :**
- ✅ 각 클래스가 필요한 인터페이스만 구현
- ✅ 빈 메서드가 생기지 않음
- ✅ 명확한 책임

---

### 📌 핵심 3 : 유연한 상호작용

**Projectile의 인터페이스 체크 :**
```csharp
private void CheckCollisionInterfaces(Collision collision)
{
    var monoBehaviours = collision.gameObject.GetComponents<MonoBehaviour>();

    foreach (var monoBehaviour in monoBehaviours)
    {
        // ✅ IDamageable 체크
        if (monoBehaviour is IDamageable damageable)
        {
            damageable.TakeDamage(m_DamageValue);
        }

        // ✅ IEffectTrigger 체크
        if (monoBehaviour is IEffectTrigger effectTrigger)
        {
            effectTrigger.TriggerEffect(offsetPosition);
        }
    }
}
```

**효과 :**
- ✅ 각 인터페이스를 독립적으로 체크
- ✅ 인터페이스를 가진 컴포넌트만 처리
- ✅ 새로운 인터페이스 추가 쉬움
- ✅ 구체 클래스에 의존하지 않음

---

## ⚖️ 장단점

### ✅ 장점

**1. 명확한 책임**
- 각 인터페이스가 하나의 책임만 가짐
- 클라이언트가 필요한 것만 구현

**2. 유연한 조합**
- 여러 작은 인터페이스를 조합 가능
- 기능 확장이 쉬움

**3. 독립적 변경**
- 인터페이스 변경이 다른 클라이언트에 영향 없음
- 유지보수 용이

**4. 빈 메서드 제거**
- 사용하지 않는 메서드 구현 불필요
- 코드 품질 향상

**5. 테스트 용이**
- 각 인터페이스를 독립적으로 테스트
- Mock 객체 생성 쉬움

**6. 확장성**
- 새로운 인터페이스 추가 쉬움
- 기존 코드 수정 최소화

### ❌ 단점

**1. 인터페이스 수 증가**
- 관리할 인터페이스가 많아짐
- 초기 설계 복잡도 증가

**2. 초기 설계 시간**
- 인터페이스 분리 기준 파악 필요
- 과도한 분리 위험

**3. 간단한 시스템에는 과도함**
- 기능이 1~2개만 있을 때는 오버엔지니어링
- 트레이드오프 고려 필요

---

## 🎮 실제 적용 사례

### 1️⃣ 게임 개발

**유닛 시스템**
```csharp
// ✅ ISP 적용
public interface IMovable
{
    void Move(Vector3 direction);
}

public interface IAttackable
{
    void Attack(GameObject target);
}

public interface IHealable
{
    void Heal(float amount);
}

// 전사 : 이동 + 공격
public class Warrior : IMovable, IAttackable { }

// 마법사 : 이동 + 공격 + 힐
public class Mage : IMovable, IAttackable, IHealable { }

// 타워 : 공격만
public class Tower : IAttackable { }
```

**인벤토리 시스템**
```csharp
// ✅ ISP 적용
public interface IUsable
{
    void Use();
}

public interface IEquippable
{
    void Equip();
    void Unequip();
}

public interface IStackable
{
    int StackSize { get; }
}

// 포션 : 사용 가능 + 스택 가능
public class Potion : IUsable, IStackable { }

// 무기 : 장착 가능
public class Weapon : IEquippable { }

// 재료 : 스택 가능
public class Material : IStackable { }
```

### 2️⃣ UI 시스템

```csharp
// ✅ ISP 적용
public interface IClickable
{
    void OnClick();
}

public interface IDraggable
{
    void OnDragStart();
    void OnDragEnd();
}

public interface IHoverable
{
    void OnHoverEnter();
    void OnHoverExit();
}

// 버튼 : 클릭 + 호버
public class Button : IClickable, IHoverable { }

// 드래그 아이템 : 드래그 + 클릭
public class DraggableItem : IDraggable, IClickable { }

// 툴팁 : 호버만
public class Tooltip : IHoverable { }
```

### 3️⃣ 오디오 시스템

```csharp
// ✅ ISP 적용
public interface IPlayable
{
    void Play();
    void Stop();
}

public interface IVolumeable
{
    float Volume { get; set; }
}

public interface ILoopable
{
    bool IsLooping { get; set; }
}

// 효과음 : 재생 + 볼륨
public class SoundEffect : IPlayable, IVolumeable { }

// 배경음악 : 재생 + 볼륨 + 루프
public class BGM : IPlayable, IVolumeable, ILoopable { }

// 단순 사운드 : 재생만
public class SimpleSound : IPlayable { }
```

---

## 📝 학습 정리

### 핵심 요약

1. **인터페이스 분리 원칙 (ISP)**
   - 클라이언트는 사용하지 않는 메서드에 의존하지 않아야 함
   - 큰 인터페이스 하나보다 작은 인터페이스 여러 개가 낫다

2. **ISP 위반 사례**
   - 사용하지 않는 메서드를 강제로 구현
   - 빈 메서드가 생김
   - 인터페이스가 비대해짐

3. **ISP 준수 방법**
   - 큰 인터페이스를 작은 단위로 분리
   - 각 인터페이스는 하나의 책임만 가짐
   - 클라이언트는 필요한 인터페이스만 구현

4. **실전 적용**
   - `IDamageable`, `IExplodable`, `IEffectTrigger` 등으로 분리
   - 각 클래스가 필요한 인터페이스만 구현
   - 여러 인터페이스 조합 가능

5. **장점**
   - 명확한 책임
   - 유연한 조합
   - 독립적 변경
   - 빈 메서드 제거
   - 테스트 용이

6. **주의사항**
   - 과도한 분리 주의
   - 초기 설계 시간 필요
   - 간단한 시스템에는 오버엔지니어링 가능

### Before vs After

| | Before (UnrefactoredTarget) | After (ISP 적용) |
|---|----------------------------|------------------|
| **인터페이스** | 1개 (큰 인터페이스) | 3개 (작은 인터페이스) |
| **빈 메서드** | ❌ 많음 | ✅ 없음 |
| **강제 구현** | ❌ 필요 | ✅ 불필요 |
| **의존성** | ❌ 불필요한 것 포함 | ✅ 필요한 것만 |
| **유연성** | ❌ 낮음 | ✅ 높음 |

### 실무 적용 팁

✅ **이런 경우 ISP 점검 필요**
- 빈 메서드가 많을 때
- 사용하지 않는 메서드를 구현해야 할 때
- 인터페이스가 비대해질 때
- 클라이언트마다 필요한 기능이 다를 때

❌ **ISP 위반 징후**
```csharp
// ❌ 빈 메서드 = ISP 위반 가능성
public class SimpleTarget : ITarget
{
    public void TakeDamage(int amount) { /* 구현 */ }
    public void Explode() { }  // 빈 메서드!
    public void TriggerEffect() { }  // 빈 메서드!
}
```

✅ **ISP 준수 코드**
```csharp
// ✅ 필요한 인터페이스만 구현
public class SimpleTarget : IDamageable
{
    public void TakeDamage(float amount) { /* 구현 */ }
    // Explode(), TriggerEffect() 구현 안 해도 됨!
}
```

### 다른 SOLID 원칙과의 관계

**SRP (Single Responsibility Principle)와의 관계 :**
- ISP는 인터페이스 레벨의 SRP
- 각 인터페이스가 하나의 책임만 가짐

**OCP (Open-Closed Principle)와의 관계 :**
- 작은 인터페이스는 확장에 유리
- 새로운 인터페이스 추가가 쉬움

**LSP (Liskov Substitution Principle)와의 관계 :**
- ISP를 지키면 LSP도 지키기 쉬움
- 인터페이스가 명확하면 대체 가능성 증가

---

**마지막 업데이트 :** 2025.12.01
