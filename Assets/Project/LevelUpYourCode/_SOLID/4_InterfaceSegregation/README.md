# 🔌 Interface Segregation Principle (인터페이스 분리 원칙)

## 📋 목차
- [풀어서 설명](#-풀어서-설명)
- [원칙 개요](#-원칙-개요)

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
public class Train : IVehicle
{
    public void TurnRight() { throw new Exception(); }  // 사용 못함!
    public void TurnLeft()  { throw new Exception(); }  // 사용 못함!
}

// ✅ ISP 준수
public class Train : IMovable  // 이동 기능만 구현
{
    public void GoForward() { ... }
    public void Reverse()   { ... }
    // TurnRight, TurnLeft 구현 안 해도 됨! ✅
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
public interface IVehicle
{
    void GoForward();
    void Reverse();
    void TurnRight();  // 기차는 사용 못함!
    void TurnLeft();   // 기차는 사용 못함!
}

public class Train : IVehicle
{
    public void GoForward() { ... }
    public void Reverse() { ... }
    public void TurnRight() { throw new Exception(); }  // ❌ 강제 구현!
    public void TurnLeft() { throw new Exception(); }   // ❌ 강제 구현!
}
// 문제 : Train이 사용하지 않는 메서드를 구현해야 함!
```

**ISP 적용 :**
```csharp
// ✅ 작은 인터페이스 여러 개
public interface IMovable
{
    void GoForward();
    void Reverse();
}

public interface ITurnable
{
    void TurnRight();
    void TurnLeft();
}

public class Car : IMovable, ITurnable  // 둘 다 필요
{
    public void GoForward() { ... }
    public void Reverse() { ... }
    public void TurnRight() { ... }
    public void TurnLeft() { ... }
}

public class Train : IMovable  // 이동만 필요
{
    public void GoForward() { ... }
    public void Reverse() { ... }
    // ✅ TurnRight, TurnLeft 구현 안 해도 됨!
}
```

---

**🚧 이 원칙은 아직 학습 전입니다.**

**학습 예정 :** 데모 프로젝트 분석 및 상세 내용 추가

---

**마지막 업데이트 :** 2025.11.28
