## 📋 목차
- [개요](#-개요)
- [프로젝트 구조](#-프로젝트-구조)
- [프로젝트 목록](#-프로젝트-목록)


## 🎮 개요

이 레포지토리는 Unity 개발 과정에서 궁금했던 기술들을 직접 구현하고 실험하는 공간입니다.

## 📁 프로젝트 구조

```
Assets/
   └── Project/
       ├── 2D_3D_Collision(Complete)/  # ✅ 완료
       ├── LevelUpYourCode/            # 🚧 진행 중
       ├── URP Shader Basic/           # 🚧 진행 중
       │
       └── [다음 프로젝트]/             # 추가될 프로젝트
           ├── README.MD
           └── ...
```


## 🎯 프로젝트 목록

### 1. [2D/3D Collision](./Assets/Project/2D_3D_Collision(Complete)/README.MD)

**기간 :** 2025.10.10 ~ 2025.10.12

**주제 :** 2D 오브젝트와 3D 오브젝트 간의 충돌 감지 시스템

**🔍 진행 배경 :**
데이브 더 다이버(Dave the Diver)를 플레이하면서 "2D 플레이어가 어떻게 3D 상어와 충돌할 수 있을까?"라는 궁금증에서 시작되었습니다.

**✨ 핵심 기술 :**
- Perspective 카메라 환경에서 2D Physics 콜라이더 투영
- 원근 효과에 따른 스케일 자동 보정

**📂 주요 스크립트 :**
- `Shark3DCollider2DProjector.cs` : 3D 콜라이더를 2D 평면에 투영

---

### 2. [LevelUpYourCode - Design Patterns & SOLID Principles](./Assets/Project/LevelUpYourCode/README.md)

**기간 :** 2025.11.24 ~ ∞

**주제 :** 유니티 코리아에서 제공하는 '디자인 패턴 및 SOLID 원칙으로 코딩 스킬 업그레이드 PDF'와 데모 프로젝트를 기반으로 솔리드 원칙과 디자인 패턴 학습 및 이해

**🔍 진행 배경 :**
실무에서 활용할 수 있는 디자인 패턴과 SOLID 원칙을 체계적으로 학습하고, 각 패턴의 구조와 동작 원리를 깊이 있게 이해하기 위한 학습 프로젝트입니다.

**✨ 핵심 기술 :**
- **SOLID 원칙** : SRP, OCP, LSP, ISP, DIP
- **디자인 패턴** : Factory, Object Pool, Singleton, Command, State, Observer, MVP, Strategy, Flyweight, Dirty Flag 등

**📂 학습 내용 :**

**SOLID 원칙 :**
- ✅ [Single Responsibility Principle (SRP)](./Assets/Project/LevelUpYourCode/_SOLID/1_SingleResponsibility/README.md) - 단일 책임 원칙
- ✅ [Open-Closed Principle (OCP)](./Assets/Project/LevelUpYourCode/_SOLID/2_OpenClosed/README.MD) - 개방-폐쇄 원칙
- ✅ [Liskov Substitution Principle (LSP)](./Assets/Project/LevelUpYourCode/_SOLID/3_LiskovSubstitution/README.md) - 리스코프 치환 원칙
- ✅ [Interface Segregation Principle (ISP)](./Assets/Project/LevelUpYourCode/_SOLID/4_InterfaceSegregation/README.md) - 인터페이스 분리 원칙
- ✅ [Dependency Inversion Principle (DIP)](./Assets/Project/LevelUpYourCode/_SOLID/5_DependencyInversion/README.md) - 의존 역전 원칙

**디자인 패턴 :**
- ✅ [Factory Pattern](./Assets/Project/LevelUpYourCode/_DesignPatterns/1_Factory/README.md) - 객체 생성 캡슐화 및 느슨한 결합
- ✅ [Object Pool Pattern](./Assets/Project/LevelUpYourCode/_DesignPatterns/2_ObjectPool/README.md) - 객체 재사용 및 GC 최적화
- ✅ [Singleton Pattern](./Assets/Project/LevelUpYourCode/_DesignPatterns/3_Singleton/README.md) - 단일 인스턴스 보장 및 전역 접근
- ✅ [Command Pattern](./Assets/Project/LevelUpYourCode/_DesignPatterns/4_Command/README.md) - 명령 캡슐화 및 Undo/Redo 구현
- ✅ [Observer Pattern](./Assets/Project/LevelUpYourCode/_DesignPatterns/6_Observer/README.md) - 이벤트 구독/발행 및 느슨한 결합
- 🚧 다른 패턴들 학습 진행 중...

---

### 3. [URP Shader Basic - 셰이더 코딩 입문](./Assets/Project/URP%20Shader%20Basic/README.md)

**기간 :** 2026.01.15 ~ 진행 중

**주제 :** URP 셰이더를 직접 코딩하며 셰이더의 기초 지식 습득

**🔍 진행 배경 :**
셰이더 그래프를 더 깊이 있게 활용하기 위해, 먼저 셰이더 코드를 직접 작성해보며 렌더링 파이프라인과 셰이더 구조를 이해하고자 시작한 학습 프로젝트입니다.

**✨ 핵심 기술 :**
- **ShaderLab** : Unity 셰이더 구조 (Shader, Properties, SubShader, Pass)
- **HLSL** : 버텍스/프래그먼트 셰이더 작성
- **URP** : Universal Render Pipeline 이해

**📂 학습 내용 :**
- ✅ [URP Shader Training #1](./Assets/Project/URP%20Shader%20Basic/URP%20Shader%20Training%20%231/README.md) - 기본 구조 및 Properties
- ✅ [URP Shader Training #2](./Assets/Project/URP%20Shader%20Basic/URP%20Shader%20Training%20%232/README.md) - Alpha Cutout 및 렌더 스테이트
- ✅ [URP Shader Training #3](./Assets/Project/URP%20Shader%20Basic/URP%20Shader%20Training%20%233/README.md) - UV 활용 및 버텍스 애니메이션
- 🚧 URP Shader Training #4 - 예정

---

### 4. 🚧 다음 프로젝트 준비 중...

추후, 새로운 미니 프로젝트 진행 후, 추가 예정...
