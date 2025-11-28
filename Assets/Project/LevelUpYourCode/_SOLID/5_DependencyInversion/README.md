# 🔄 Dependency Inversion Principle (의존성 역전 원칙)

## 📋 목차
- [풀어서 설명](#-풀어서-설명)
- [원칙 개요](#-원칙-개요)

---

## 💡 풀어서 설명

### 한 문장으로 이해하기

```
구체적인 것(구현 클래스)에 의존하지 말고,
추상적인 것(인터페이스, 추상 클래스)에 의존하라.
```

### 실생활 비유

**❌ 나쁜 예 : 특정 충전기에 의존**
```
스마트폰이 특정 회사의 충전기에만 연결 가능

삼성 충전기 → 삼성폰만 충전 가능 ✅
삼성 충전기 → 애플폰 충전 불가   ❌
애플 충전기 → 애플폰만 충전 가능 ✅
애플 충전기 → 삼성폰 충전 불가   ❌

문제점 :
→ 충전기가 바뀌면 폰도 바꿔야 함!
→ 확장이 어렵고 유연하지 못함!
```

**✅ 좋은 예 : USB-C 규격에 의존**
```
스마트폰이 USB-C 규격(인터페이스)에 연결 가능

USB-C 충전기 → 삼성폰 충전 가능 ✅
USB-C 충전기 → 애플폰 충전 가능 ✅
USB-C 충전기 → LG폰 충전 가능  ✅

장점 :
→ 어떤 USB-C 충전기든 사용 가능!
→ 확장이 쉽고 유연함!
```

### 판단 기준

**"new 키워드로 직접 생성하고 있는가?"**

- ✅ **외부에서 주입받음** → DIP 준수! 추상화에 의존!
- ❌ **내부에서 new 생성** → DIP 위반! 구체 클래스에 의존!

```csharp
// ❌ DIP 위반
public class PlayerController
{
    private KeyboardInput input = new KeyboardInput();  // 구체 클래스에 직접 의존!

    void Update()
    {
        input.GetInput();  // 키보드 입력만 가능, 조이스틱 불가!
    }
}

// ✅ DIP 준수
public class PlayerController
{
    private IInput input;  // 인터페이스에 의존

    public PlayerController(IInput input)  // 외부에서 주입!
    {
        this.input = input;
    }

    void Update()
    {
        input.GetInput();  // 키보드든, 조이스틱이든, 터치든 상관없음! ✅
    }
}
```

---

## 🎯 원칙 개요

**Dependency Inversion Principle (DIP)** 은 **SOLID 원칙**의 다섯 번째 원칙으로, 고수준 모듈이 저수준 모듈에 의존하지 않고, 둘 다 추상화에 의존해야 한다는 원칙입니다.

### 📌 핵심 개념

```
구체적인 구현이 아닌, 추상화에 의존하라.
new 키워드 사용을 줄이고, 의존성을 주입받아라.
```

**잘못된 설계 :**
```csharp
// ❌ 구체 클래스에 직접 의존
public class GameManager
{
    private FileLogger logger = new FileLogger();  // FileLogger에 직접 의존!

    public void LogMessage(string message)
    {
        logger.Log(message);
    }
}
// 문제 : FileLogger를 ConsoleLogger로 바꾸려면 GameManager 수정 필요!
```

**DIP 적용 :**
```csharp
// ✅ 추상화(인터페이스)에 의존
public interface ILogger
{
    void Log(string message);
}

public class FileLogger : ILogger
{
    public void Log(string message) { /* 파일에 기록 */ }
}

public class ConsoleLogger : ILogger
{
    public void Log(string message) { /* 콘솔에 출력 */ }
}

public class GameManager
{
    private ILogger logger;  // 인터페이스에 의존!

    public GameManager(ILogger logger)  // 의존성 주입!
    {
        this.logger = logger;
    }

    public void LogMessage(string message)
    {
        logger.Log(message);
    }
}

// 사용 예시
var gameManager1 = new GameManager(new FileLogger());     // 파일 로거 사용
var gameManager2 = new GameManager(new ConsoleLogger());  // 콘솔 로거 사용
// ✅ GameManager 코드 수정 없이 로거 교체 가능!
```

---

**🚧 이 원칙은 아직 학습 전입니다.**

**학습 예정 :** 데모 프로젝트 분석 및 상세 내용 추가

---

**마지막 업데이트 :** 2025.11.28
