# 🎮 Command Pattern (커맨드 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [왜 Command Pattern이 필요한가?](#-왜-command-pattern이-필요한가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 사용 사례](#-실제-사용-사례)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**Command Pattern**은 **행동 패턴(Behavioral Pattern)** 중 하나로, 요청(명령)을 객체로 캡슐화하여 다양한 작업을 수행할 수 있게 하는 패턴입니다.

### 📌 핵심 개념

```
명령(요청)을 "객체"로 만들어서 다룬다!
```

**일반적인 방법 :**
```csharp
// 명령을 직접 실행
player.Move(Vector3.forward);
// ❌ 문제 : Undo 불가능, 저장 불가능, 전송 불가능
```

**Command Pattern :**
```csharp
// 명령을 객체로 만듦
ICommand command = new MoveCommand(player, Vector3.forward);
CommandInvoker.ExecuteCommand(command);
// ✅ 장점 : Undo 가능, 저장 가능, 전송 가능!
```

---

## 🤔 왜 Command Pattern이 필요한가?

### 문제 상황

게임에서 플레이어가 이동하는 기능을 만들 때, 일반적으로 이렇게 작성합니다 :

```csharp
void Update()
{
    if (Input.GetKeyDown(KeyCode.W))
    {
        player.transform.position += Vector3.forward;
    }
}
```

**이 코드의 문제점 :**

❌ **Undo/Redo 구현 어려움**
   - 어떤 동작을 했는지 기록이 없음
   - 되돌리기 위해서는 복잡한 히스토리 시스템 필요

❌ **재사용 불가능**
   - 명령을 저장하거나 다시 실행할 수 없음

❌ **네트워크 전송 어려움**
   - 플레이어 입력을 다른 클라이언트에 전송하기 복잡함

❌ **타이밍 제어 어려움**
   - 명령을 나중에 실행하거나 큐에 넣기 어려움

### Command Pattern의 해결책

✅ **명령을 객체로 캡슐화**
   - 명령에 필요한 모든 정보를 객체에 저장

✅ **Undo/Redo 쉽게 구현**
   - 스택 자료구조로 명령 히스토리 관리

✅ **명령 저장/전송/재생 가능**
   - 객체이므로 어디든 전달 가능

✅ **유연한 타이밍 제어**
   - 즉시 실행, 지연 실행, 큐에 추가 등 자유롭게 제어

---

## 🏗️ 핵심 구성요소

Command Pattern은 다음 5가지 요소로 구성됩니다 :

### 1️⃣ Command (명령 인터페이스)

**📁 파일 :** [ICommand.cs](./Scripts/Pattern/ICommand.cs)

```csharp
public interface ICommand
{
    public void Execute();  // 명령 실행
    public void Undo();     // 명령 취소
}
```

**역할 :**
- 모든 명령 객체가 따라야 할 표준 인터페이스
- `Execute()` : 명령을 실행
- `Undo()` : 명령을 되돌림

---

### 2️⃣ Concrete Command (구체적인 명령)

**📁 파일 :** [MoveCommand.cs](./Scripts/ExampleUsage/MoveCommand.cs)

```csharp
public class MoveCommand : ICommand
{
    private PlayerMover m_PlayerMover;  // 누구를?
    private Vector3     m_MoveVector;   // 어떻게?

    // 생성자 : 명령에 필요한 정보 저장
    public MoveCommand(PlayerMover player, Vector3 moveVector)
    {
        this.m_PlayerMover = player;
        this.m_MoveVector  = moveVector;
    }

    // 실행 : 플레이어 이동
    public void Execute()
    {
        m_PlayerMover.PlayerPath.AddToPath(...);
        m_PlayerMover.Move(m_MoveVector);
    }

    // 취소 : 반대 방향 이동
    public void Undo()
    {
        m_PlayerMover.Move(-m_MoveVector);
        m_PlayerMover.PlayerPath.RemoveFromPath();
    }
}
```

**역할 :**
- `ICommand`를 구현한 실제 명령 클래스
- 명령 실행에 필요한 정보를 멤버 변수로 저장
- Execute()와 Undo() 로직 구현

---

### 3️⃣ Invoker (호출자/관리자)

**📁 파일 :** [CommandInvoker.cs](./Scripts/Pattern/CommandInvoker.cs)

```csharp
public class CommandInvoker
{
    private static Stack<ICommand> s_UndoStack;  // Undo 스택
    private static Stack<ICommand> s_RedoStack;  // Redo 스택

    // 명령 실행 및 스택에 저장
    public static void ExecuteCommand(ICommand command)
    {
        command.Execute();
        s_UndoStack.Push(command);
        s_RedoStack.Clear();
    }

    // Undo : 마지막 명령 되돌리기
    public static void UndoCommand() { ... }

    // Redo : 취소한 명령 다시 실행
    public static void RedoCommand() { ... }
}
```

**역할 :**
- 명령을 실행하고 히스토리 관리
- Undo/Redo 스택 관리
- 명령의 생명주기 제어

---

### 4️⃣ Receiver (수신자/실행자)

**📁 파일 :** [PlayerMover.cs](./Scripts/ExampleUsage/PlayerMover.cs)

```csharp
public class PlayerMover : MonoBehaviour
{
    // 실제 이동 수행
    public void Move(Vector3 movement)
    {
        transform.position += movement;
    }

    // 이동 가능 여부 검사
    public bool IsValidMove(Vector3 movement)
    {
        return !Physics.Raycast(...);
    }
}
```

**역할 :**
- 명령이 실제로 수행해야 할 작업을 정의
- 명령의 대상(Target) 역할

---

### 5️⃣ Client (클라이언트)

**📁 파일 :** [ButtonInputs.cs](./Scripts/ExampleUsage/ButtonInputs.cs)

```csharp
public class ButtonInputs : MonoBehaviour
{
    private void RunPlayerCommand(PlayerMover player, Vector3 movement)
    {
        // 1. 명령 객체 생성
        ICommand command = new MoveCommand(player, movement);

        // 2. Invoker를 통해 실행
        CommandInvoker.ExecuteCommand(command);
    }

    private void OnUndoInput()
    {
        CommandInvoker.UndoCommand();
    }
}
```

**역할 :**
- 사용자 입력을 감지
- 명령 객체를 생성
- Invoker에게 명령 실행 요청

---

## 📊 코드 구조

### 폴더 구조

```
4_Command/
├── Scripts/
│   ├── Pattern/                    (핵심 패턴 구현)
│   │   ├── ICommand.cs            ← 명령 인터페이스
│   │   └── CommandInvoker.cs      ← 명령 관리자
│   │
│   └── ExampleUsage/              (사용 예시)
│       ├── MoveCommand.cs         ← 구체적인 명령
│       ├── PlayerMover.cs         ← 수신자 (Receiver)
│       ├── PlayerPath.cs          ← 경로 시각화
│       └── ButtonInputs.cs        ← 클라이언트
│
└── README.md                       ← 📍 현재 문서
```

### 클래스 다이어그램

```
┌─────────────────┐
│   ICommand      │  ← 인터페이스
├─────────────────┤
│ + Execute()     │
│ + Undo()        │
└─────────────────┘
         △
         │ implements
         │
┌─────────────────┐
│  MoveCommand    │  ← 구체 명령
├─────────────────┤
│ - m_PlayerMover │
│ - m_MoveVector  │
├─────────────────┤
│ + Execute()     │
│ + Undo()        │
└─────────────────┘
         │ uses
         ▼
┌─────────────────┐
│  PlayerMover    │  ← Receiver
├─────────────────┤
│ + Move()        │
│ + IsValidMove() │
└─────────────────┘

┌─────────────────┐
│ CommandInvoker  │  ← Invoker
├─────────────────┤
│ - s_UndoStack   │
│ - s_RedoStack   │
├─────────────────┤
│ + ExecuteCommand()│
│ + UndoCommand() │
│ + RedoCommand() │
└─────────────────┘
         △
         │ calls
         │
┌─────────────────┐
│  ButtonInputs   │  ← Client
├─────────────────┤
│ + OnForwardInput()│
│ + OnUndoInput() │
└─────────────────┘
```

---

## 🔄 실행 흐름

### 1️⃣ 명령 실행 흐름 (Execute)

```
[사용자 입력]
    ⬇️
W 키 누름 (ButtonInputs.Update)
    ⬇️
OnForwardInput() 호출
    ⬇️
RunPlayerCommand(player, Vector3.forward)
    ⬇️
┌─────────────────────────────────┐
│ 1. 명령 객체 생성 (캡슐화)      │
│    ICommand cmd = new           │
│    MoveCommand(player, forward) │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 2. Invoker에게 전달             │
│    CommandInvoker.              │
│    ExecuteCommand(cmd)          │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 3. Invoker가 명령 실행          │
│    cmd.Execute()                │
│    + Undo 스택에 저장           │
│    + Redo 스택 초기화           │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 4. MoveCommand.Execute()        │
│    - 경로 점 추가               │
│    - PlayerMover.Move() 호출    │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 5. PlayerMover.Move()           │
│    transform.position += vector │
│    → 플레이어 실제 이동!        │
└─────────────────────────────────┘
```

### 2️⃣ Undo 흐름

```
[사용자 입력]
    ⬇️
U 키 누름
    ⬇️
CommandInvoker.UndoCommand()
    ⬇️
┌─────────────────────────────────┐
│ 1. Undo 스택에서 명령 꺼내기   │
│    ICommand cmd =               │
│    s_UndoStack.Pop()            │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 2. Redo 스택에 저장             │
│    s_RedoStack.Push(cmd)        │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 3. 명령 취소 실행               │
│    cmd.Undo()                   │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 4. MoveCommand.Undo()           │
│    - 반대 방향 이동             │
│    - 경로 점 제거               │
└─────────────────────────────────┘
```

### 3️⃣ Redo 흐름

```
[사용자 입력]
    ⬇️
R 키 누름
    ⬇️
CommandInvoker.RedoCommand()
    ⬇️
┌─────────────────────────────────┐
│ 1. Redo 스택에서 명령 꺼내기   │
│    ICommand cmd =               │
│    s_RedoStack.Pop()            │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 2. Undo 스택에 다시 저장        │
│    s_UndoStack.Push(cmd)        │
└─────────────────────────────────┘
    ⬇️
┌─────────────────────────────────┐
│ 3. 명령 다시 실행               │
│    cmd.Execute()                │
└─────────────────────────────────┘
```

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : 명령 생성 및 실행

**위치 :** ButtonInputs.cs:43-62

```csharp
private void RunPlayerCommand(PlayerMover playerMover, Vector3 movement)
{
    if (playerMover == null)
    {
        return;
    }

    // 이동이 가능한지 검사 (장애물 체크)
    if (playerMover.IsValidMove(movement))
    {
        // ✅ 핵심 1 : 명령 객체 생성 (캡슐화)
        ICommand command = new MoveCommand(playerMover, movement);

        // ✅ 핵심 2 : Invoker를 통해 명령 실행 및 스택 저장
        CommandInvoker.ExecuteCommand(command);
    }
}
```

**이해 포인트 :**
- 명령을 즉시 실행하지 않고, 먼저 **객체로 만듦** (캡슐화)
- Invoker를 거쳐서 실행하므로 **히스토리 관리 자동화**
- 나중에 Undo/Redo 가능!

---

### 📌 핵심 코드 2 : Undo/Redo 스택 관리

**위치 :** CommandInvoker.cs:18-52

```csharp
public static void ExecuteCommand(ICommand command)
{
    command.Execute();              // 명령 즉시 실행
    s_UndoStack.Push(command);      // Undo 스택에 저장

    // 새로운 명령 실행 시 Redo 히스토리 삭제
    // (새로운 분기가 생성되므로 이전 Redo는 무효화)
    s_RedoStack.Clear();
}

public static void UndoCommand()
{
    if (s_UndoStack.Count > 0)
    {
        ICommand activeCommand = s_UndoStack.Pop();   // Undo 스택에서 꺼냄
        s_RedoStack.Push(activeCommand);              // Redo 스택에 저장
        activeCommand.Undo();                         // 명령 되돌리기
    }
}

public static void RedoCommand()
{
    if (s_RedoStack.Count > 0)
    {
        ICommand activeCommand = s_RedoStack.Pop();   // Redo 스택에서 꺼냄
        s_UndoStack.Push(activeCommand);              // Undo 스택에 다시 저장
        activeCommand.Execute();                      // 명령 다시 실행
    }
}
```

**이해 포인트 :**
- **두 개의 스택**으로 Undo/Redo 구현
- 새 명령 실행 시 Redo 스택 초기화 (새로운 타임라인 생성)
- 스택 자료구조의 LIFO(Last In First Out) 특성 활용

---

### 📌 핵심 코드 3 : 명령 캡슐화

**위치 :** MoveCommand.cs:7-38

```csharp
public class MoveCommand : ICommand
{
    // 명령에 필요한 정보를 멤버 변수로 저장
    private PlayerMover m_PlayerMover;  // 누구를?
    private Vector3     m_MoveVector;   // 어떻게?

    // 생성자 : 명령 생성 시 정보 저장 (아직 실행 안 함!)
    public MoveCommand(PlayerMover player, Vector3 moveVector)
    {
        this.m_PlayerMover = player;
        this.m_MoveVector  = moveVector;
    }

    // 명령 실행
    public void Execute()
    {
        m_PlayerMover.PlayerPath.AddToPath(...);  // 경로 추가
        m_PlayerMover.Move(m_MoveVector);         // 이동
    }

    // 명령 취소
    public void Undo()
    {
        m_PlayerMover.Move(-m_MoveVector);        // 반대로 이동
        m_PlayerMover.PlayerPath.RemoveFromPath(); // 경로 제거
    }
}
```

**이해 포인트 :**
- 생성자에서는 **정보만 저장**, 실행하지 않음!
- Execute()에서 **실제 실행**
- Undo()에서 **반대 동작** 수행
- 모든 정보가 객체 안에 캡슐화되어 있음

---

## ⚖️ 장단점

### ✅ 장점

**1. Undo/Redo 구현 용이**
- 스택으로 명령 히스토리 자동 관리
- 무제한 Undo/Redo 가능
- 각 명령이 자신의 Undo 로직을 가짐

**2. 명령의 캡슐화**
- 요청과 실행을 분리
- 명령을 객체로 다룰 수 있음
- 저장, 전달, 스케줄링 가능

**3. 확장성**
- 새로운 명령 추가 쉬움 (ICommand만 구현)
- 기존 코드 수정 없이 확장 (OCP 원칙)

**4. 디커플링 (Decoupling)**
- 입력과 실행이 분리
- Invoker가 중간에서 명령 관리
- 각 컴포넌트가 독립적

**5. 매크로 명령 구현 가능**
- 여러 명령을 하나로 묶을 수 있음
- 복잡한 작업을 하나의 명령으로 실행

**6. 로깅/기록 가능**
- 모든 명령이 객체이므로 로그 남기기 쉬움
- 리플레이 기능 구현 가능

### ❌ 단점

**1. 클래스 수 증가**
- 각 명령마다 새로운 클래스 필요
- 코드량 증가

**2. 메모리 사용량 증가**
- 명령 객체들이 스택에 쌓임
- Undo 깊이 제한 필요할 수 있음

**3. 간단한 작업에는 과도함**
- 단순한 기능에는 오버엔지니어링
- 복잡도 증가

**4. Undo 구현 복잡도**
- 되돌리기 로직이 복잡할 수 있음
- 상태 저장이 어려운 경우 존재

---

## 🎮 실제 사용 사례

### 1️⃣ 게임 개발

**턴제 전략 게임**
```csharp
// 각 턴의 행동을 Command로 저장
ICommand moveUnit   = new MoveUnitCommand(...);
ICommand attackUnit = new AttackCommand(...);
ICommand castSpell  = new CastSpellCommand(...);

// 플레이어가 실수하면 Undo로 되돌리기
CommandInvoker.UndoCommand();
```

**리플레이 시스템**
```csharp
// 플레이어의 모든 입력을 명령으로 저장
List<ICommand> replayCommands = new List<ICommand>();

// 나중에 리플레이
foreach (var cmd in replayCommands)
{
    cmd.Execute();
}
```

**AI 행동 큐**
```csharp
// AI의 행동을 큐에 저장
Queue<ICommand> aiCommands = new Queue<ICommand>();
aiCommands.Enqueue(new MoveToCommand(...));
aiCommands.Enqueue(new AttackCommand(...));
aiCommands.Enqueue(new ReturnCommand(...));

// 순차적으로 실행
ICommand nextCommand = aiCommands.Dequeue();
nextCommand.Execute();
```

### 2️⃣ 레벨 에디터

```csharp
// 오브젝트 배치/삭제를 Command로 처리
ICommand place  = new PlaceObjectCommand(...);
ICommand delete = new DeleteObjectCommand(...);
ICommand move   = new MoveObjectCommand(...);

// Ctrl+Z / Ctrl+Y 자동 지원!
```

### 3️⃣ 네트워크 게임

```csharp
// 플레이어 입력을 Command 객체로 전송
ICommand playerInput = new MoveCommand(...);
networkManager.SendCommand(playerInput);

// 서버에서 명령 수신 후 실행
ICommand receivedCommand = networkManager.ReceiveCommand();
receivedCommand.Execute();
```