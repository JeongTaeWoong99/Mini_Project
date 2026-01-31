# 🎓 LevelUpYourCode - Design Patterns & SOLID Principles

## 📋 목차
- [프로젝트 개요](#-프로젝트-개요)
- [학습 자료](#-학습-자료)
- [SOLID 원칙](#-solid-원칙)
- [디자인 패턴](#-디자인-패턴)
- [학습 방법](#-학습-방법)

---

## 🎯 프로젝트 개요

**기간 :** 2025.11.24 ~ ∞

**목표 :** 유니티 코리아에서 제공하는 '디자인 패턴 및 SOLID 원칙으로 코딩 스킬 업그레이드' PDF와 데모 프로젝트를 통해 실무에서 활용 가능한 디자인 패턴과 SOLID 원칙을 체계적으로 학습하고 이해합니다.

**진행 방식 :**
1. 각 패턴/원칙의 이론 학습
2. 데모 프로젝트 코드 분석
3. 이해한 내용을 README로 정리

---

## 📚 학습 자료

**공식 자료 :**
- 📄 [디자인 패턴 및 SOLID 원칙으로 코딩 스킬 업그레이드.pdf(Unity Korea 공식 싸이트 제공 자료)](./디자인%20패턴%20및%20SOLID%20원칙으로%20코딩%20스킬%20업그레이드.pdf)
- 🎮 [Unity 공식 데모 프로젝트(Asset Store 다운로드 페이지)](https://assetstore.unity.com/packages/essentials/tutorial-projects/level-up-your-code-with-design-patterns-and-solid-289616)

---

## 🏗️ SOLID 원칙

객체 지향 설계의 5가지 핵심 원칙

| 원칙 | 설명 | 상세 보기 |
|------|------|-----------|
| **SRP**<br>Single Responsibility | 단일 책임 원칙 : 클래스는 하나의 책임만 가져야 한다 | [📖 상세 보기](./_SOLID/1_SingleResponsibility/README.md) |
| **OCP**<br>Open-Closed | 개방-폐쇄 원칙 : 확장에는 열려있고, 수정에는 닫혀있어야 한다 | [📖 상세 보기](./_SOLID/2_OpenClosed/README.MD) |
| **LSP**<br>Liskov Substitution | 리스코프 치환 원칙 : 하위 클래스는 상위 클래스를 대체할 수 있어야 한다 | [📖 상세 보기](./_SOLID/3_LiskovSubstitution/README.md) |
| **ISP**<br>Interface Segregation | 인터페이스 분리 원칙 : 클라이언트는 사용하지 않는 인터페이스에 의존하지 않아야 한다 | [📖 상세 보기](./_SOLID/4_InterfaceSegregation/README.md) |
| **DIP**<br>Dependency Inversion | 의존 역전 원칙 : 구체화가 아닌 추상화에 의존해야 한다 | [📖 상세 보기](./_SOLID/5_DependencyInversion/README.md) |

---

## 🎨 디자인 패턴

실무에서 자주 사용되는 디자인 패턴들

### ✅ 학습 완료

| 패턴 | 카테고리 | 핵심 개념 | 상세 보기 |
|------|----------|-----------|-----------|
| **Factory** | 생성 패턴 | 객체 생성 캡슐화, 느슨한 결합 | [📖 상세 보기](./_DesignPatterns/1_Factory/README.md) |
| **Object Pool** | 생성 패턴 | 객체 재사용, GC 최적화 | [📖 상세 보기](./_DesignPatterns/2_ObjectPool/README.md) |
| **Singleton** | 생성 패턴 | 단일 인스턴스 보장, 전역 접근 | [📖 상세 보기](./_DesignPatterns/3_Singleton/README.md) |
| **Command** | 행동 패턴 | 명령 캡슐화, Undo/Redo | [📖 상세 보기](./_DesignPatterns/4_Command/README.md) |
| **State** | 행동 패턴 | 상태 기반 행동 변경, 상태별 클래스 분리 | [📖 상세 보기](./_DesignPatterns/5_State/README.md) |
| **Observer** | 행동 패턴 | 이벤트 구독/발행, 느슨한 결합 | [📖 상세 보기](./_DesignPatterns/6_Observer/README.md) |

### 🚧 학습 예정

| 패턴 | 카테고리 | 핵심 개념 | 학습 상태 |
|------|----------|-----------|-----------|
| **Strategy** | 행동 패턴 | 알고리즘 캡슐화 | 🚧 예정 |
| **MVP** | 아키텍처 패턴 | Model-View-Presenter | 🚧 예정 |
| **MVVM** | 아키텍처 패턴 | Model-View-ViewModel | 🚧 예정 |
| **Flyweight** | 구조 패턴 | 메모리 최적화 | 🚧 예정 |
| **Dirty Flag** | 최적화 패턴 | 변경 감지 최적화 | 🚧 예정 |

---

## 📂 프로젝트 구조

```
LevelUpYourCode/
├── _SOLID/                          # SOLID 원칙 예제
│   ├── 1_SingleResponsibility/      # ✅ 단일 책임 원칙 (완료)
│   ├── 2_OpenClosed/                # ✅ 개방-폐쇄 원칙 (완료)
│   ├── 3_LiskovSubstitution/        # ✅ 리스코프 치환 원칙 (완료)
│   ├── 4_InterfaceSegregation/      # ✅ 인터페이스 분리 원칙 (완료)
│   └── 5_DependencyInversion/       # ✅ 의존성 역전 원칙 (완료)
│
├── _DesignPatterns/                 # 디자인 패턴 예제
│   ├── 1_Factory/                   # ✅ 팩토리 패턴 (완료)
│   ├── 2_ObjectPool/                # ✅ 오브젝트 풀 패턴 (완료)
│   ├── 3_Singleton/                 # ✅ 싱글톤 패턴 (완료)
│   ├── 4_Command/                   # ✅ 커맨드 패턴 (완료)
│   ├── 5_State/                     # ✅ 스테이트 패턴 (완료)
│   ├── 6_Observer/                  # ✅ 옵저버 패턴 (완료)
│   ├── 7_MVP/                       # MVP 패턴
│   ├── 7_MVP_UIToolkit/             # MVP (UI Toolkit)
│   ├── 7_MVVM/                      # MVVM 패턴
│   ├── 8_Strategy/                  # 전략 패턴
│   ├── 9_Flyweight/                 # 플라이웨이트 패턴
│   └── 10_DirtyFlag/                # 더티 플래그 패턴
│
├── Scenes/                          # 데모 씬들
├── Scripts/                         # 공통 스크립트
└── README.md                        # 📍 현재 문서
```

---

## 🎯 학습 진행 상황

### SOLID 원칙
- ✅ Single Responsibility Principle (SRP) - 완료 (2025.11.26)
- ✅ Open-Closed Principle (OCP) - 완료 (2025.11.27)
- ✅ Liskov Substitution Principle (LSP) - 완료 (2025.11.28)
- ✅ Interface Segregation Principle (ISP) - 완료 (2025.12.01)
- ✅ Dependency Inversion Principle (DIP) - 완료 (2025.12.01)

### 디자인 패턴
- ✅ Command Pattern - 완료 (2025.11.24)
- ✅ Singleton Pattern - 완료 (2025.12.08)
- ✅ Observer Pattern - 완료 (2025.12.18)
- ✅ Factory Pattern - 완료 (2026.01.17)
- ✅ Object Pool Pattern - 완료 (2026.01.22)
- ✅ State Pattern - 완료 (2026.01.31)
- 🚧 다른 패턴들 학습 진행 중...

---

**마지막 업데이트 :** 2026.01.31 (✅ State Pattern 완료!)