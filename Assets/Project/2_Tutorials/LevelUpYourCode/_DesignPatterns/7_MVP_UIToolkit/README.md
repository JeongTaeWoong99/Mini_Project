# 🎮 MVP Pattern - UI Toolkit (Model-View-Presenter 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [Legacy와 무엇이 달라졌는가?](#-legacy와-무엇이-달라졌는가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

> 📖 MVP 패턴의 기본 개념(왜 필요한가, 핵심 원리, 통신 규칙 등)은
> [7_MVP(Legacy) README](../7_MVP(Legacy)/README.md)와 동일합니다.

이 프로젝트는 **동일한 MVP 패턴**을 **UI Toolkit + ScriptableObject**로 구현한 버전입니다.

패턴의 구조와 흐름은 Legacy와 완전히 동일하며, **"무엇으로 만들었느냐"** 만 다릅니다 :

```
┌─────────────────────────────────────────────────────────────────┐
│  MVP 패턴 구조 자체       → 동일 (Model ↔ Presenter ↔ View)     │
│  이벤트 기반 통신         → 동일 (HealthChanged 이벤트)          │
│  Presenter 수동 업데이트  → 동일 (UpdateUI 직접 호출)            │
├─────────────────────────────────────────────────────────────────┤
│  ✨ Model 구현 방식      → MonoBehaviour → ScriptableObject     │
│  ✨ View (UI 시스템)     → uGUI → UI Toolkit (UI Builder)       │
│  ✨ View 요소 접근 방식  → SerializeField → Q<T>() 쿼리         │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Legacy와 무엇이 달라졌는가?

### 한눈에 비교

| 항목 | **Legacy (uGUI)** | **UIToolkit** | 뭐가 좋아졌나? |
|------|-------------------|---------------|----------------|
| **Model** | `MonoBehaviour` | `ScriptableObject` | 씬 독립, 에셋 재사용 |
| **View 정의** | 씬에서 uGUI 직접 배치 | UI Builder에서 제작 → UXML로 저장 | 시각적 에디터로 직관적 제작 |
| **View 스타일** | 각 컴포넌트 Inspector에서 설정 | UI Builder에서 설정 → USS로 저장 | 스타일 일괄 관리, 재사용 |
| **View 요소 참조** | `[SerializeField] Slider` | `Q<ProgressBar>("name")` | Inspector 연결 없이 코드로 검색 |
| **씬 Hierarchy** | Model, View, Presenter 프리팹 각각 배치 | `DemoPresenter` 1개에 집약 | 씬 구조 단순화 |

---

### 1️⃣ Model : MonoBehaviour → ScriptableObject

**Legacy :**
```csharp
// 씬 오브젝트에 붙어야만 존재할 수 있음
public class Health : MonoBehaviour { ... }
```

**UIToolkit :**
```csharp
// 에셋 파일로 독립 존재, 어디서든 참조 가능
[CreateAssetMenu(fileName = "HealthData", menuName = "DesignPatterns/MVP_UIToolkit/HealthModel")]
public class HealthModel : ScriptableObject { ... }
```

**무엇이 좋아졌나?**

✅ **씬 독립** : 에셋 파일이므로 씬이 바뀌어도 데이터가 유지됨

✅ **Inspector 편집** : `Data/HealthData.asset` 파일을 클릭해서 직접 데이터 편집 가능

✅ **공유 용이** : 여러 Presenter가 같은 `.asset` 파일을 참조하면 데이터 공유 가능

✅ **런타임 인스턴스** : `CreateInstance()`로 복사본을 만들어 독립 데이터로 사용 가능

```
Legacy                                UIToolkit
───────────────                       ───────────────
씬 A의 Health(MonoBehaviour)          Data/HealthData.asset (에셋 파일)
  → 씬 A에서만 존재                      → 씬 A에서 참조 가능
  → 씬 B에서 사용 불가                   → 씬 B에서도 참조 가능
  → 씬 전환 시 파괴됨                    → 씬 전환해도 유지됨
```

---

### 2️⃣ View : uGUI → UI Toolkit (UI Builder)

**Legacy :**
```
씬의 Hierarchy에서 uGUI 오브젝트(Slider, Text, Button)를 직접 배치하고
각 컴포넌트의 Inspector에서 크기, 색상 등 스타일을 하나씩 설정
```

**UIToolkit :**
```
Unity의 UI Builder(시각적 에디터)에서 UI를 드래그 앤 드롭으로 제작
→ 구조는 UXML 파일로 저장 (HTML과 유사한 형식)
→ 스타일은 USS 파일로 저장 (CSS와 유사한 형식)
```

> 💡 UXML과 USS는 **직접 코딩하는 것이 아니라**, Unity의 **UI Builder** 에디터에서
> 시각적으로 UI를 만들면 **자동으로 저장되는 파일 형식**입니다.
> 웹의 "Figma로 디자인 → HTML/CSS로 내보내기"와 비슷한 개념입니다.

**UI Builder가 생성한 UXML (자동 저장된 구조) :**
```xml
<engine:ProgressBar name="health-bar" class="health-bar">
    <engine:Label name="health-bar__status-label" class="status-label" />
    <engine:Label name="health-bar__value-label" class="value-label" />
</engine:ProgressBar>
<engine:Button name="reset-button" class="reset-button">
    <engine:Label text="Reset" name="reset-button__label" />
</engine:Button>
```

**UI Builder가 생성한 USS (자동 저장된 스타일) :**
```css
.health-bar {
    position: absolute;
    bottom: 11%;
    width: 30%;
    height: 7%;
    transition-duration: 0.2s;
}

.reset-button:hover {
    scale: 1.1 1.1;
}
```

**무엇이 좋아졌나?**

✅ **시각적 에디터** : UI Builder에서 WYSIWYG 방식으로 UI를 제작, uGUI보다 직관적

✅ **구조/스타일 분리** : UXML(구조)과 USS(스타일)가 분리되어 관리가 용이

✅ **스타일 재사용** : 같은 USS 파일을 여러 UXML에서 공유 가능

✅ **USS 가상 클래스** : `:hover`, `:active`, `transition` 등 인터랙션 효과를 코드 없이 USS에서 설정 가능

---

### 3️⃣ View 요소 접근 : SerializeField → Q<T>() 쿼리

**Legacy :**
```csharp
// Inspector에서 드래그 앤 드롭으로 연결해야 함
[SerializeField] Slider m_HealthSlider;
[SerializeField] Text   m_HealthLabel;
```

**UIToolkit :**
```csharp
// 코드에서 이름으로 검색 (Inspector 연결 불필요)
m_HealthBar   = m_Root.Q<ProgressBar>("health-bar");
m_StatusLabel = m_Root.Q<Label>("health-bar__status-label");
m_ValueLabel  = m_Root.Q<Label>("health-bar__value-label");
```

**무엇이 좋아졌나?**

✅ **Inspector 연결 불필요** : UXML에 정의된 이름으로 코드에서 바로 검색

✅ **연결 끊김 방지** : uGUI는 프리팹 수정 시 Inspector 참조가 끊어질 수 있지만, Q<T>()는 이름 기반이라 안전

✅ **동적 UI 대응** : 런타임에 UXML이 바뀌어도 이름만 같으면 자동으로 찾음

---

### 4️⃣ 씬 구조 : 여러 프리팹 → 단일 오브젝트

**Legacy (씬 Hierarchy) :**
```
MVP (Scene)
├── Model (프리팹)          ← Health 컴포넌트
├── View (프리팹)           ← Slider, Text, Button
├── Presenter (프리팹)      ← HealthPresenter 컴포넌트
└── ShooterTarget (프리팹)  ← ClickDamage 컴포넌트
```

**UIToolkit (씬 Hierarchy) :**
```
MVP_UIToolkit (Scene)
├── SceneCamera
├── Directional Light
├── EventSystem
└── DemoPresenter           ← UIDocument + HealthPresenter + DamageTrigger 전부 여기에!
```

**무엇이 좋아졌나?**

✅ **씬 단순화** : UI Toolkit은 UXML 파일이 View를 통째로 정의하므로 별도 View 오브젝트 불필요

✅ **컴포넌트 집약** : Presenter에 UIDocument를 붙이면 View 접근이 바로 가능

---

## 🏗️ 핵심 구성요소

### 1️⃣ Model (ScriptableObject)

**📁 파일 :** [HealthModel.cs](./Scripts/Model/HealthModel.cs)

> 📖 기본 역할(데이터 보유, HealthChanged 이벤트 발행, Clamp 범위 보장)은
> [7_MVP(Legacy) README - Model](../7_MVP(Legacy)/README.md#1%EF%B8%8F⃣-model-모델---데이터-담당)과 동일합니다.

**Legacy와 다른 점 :**

```csharp
// ✨ ScriptableObject 기반 → 에셋 파일로 존재
[CreateAssetMenu(fileName = "HealthData", menuName = "DesignPatterns/MVP_UIToolkit/HealthModel")]
public class HealthModel : ScriptableObject
{
    // ✨ 추가 : 체력 오브젝트 식별용 라벨 (Inspector에서 설정)
    [SerializeField] private string m_LabelName;
    public string LabelName => m_LabelName;

    // ✨ 추가 : 런타임 복사본 생성 (여러 오브젝트가 독립 데이터로 동작)
    public static HealthModel CreateInstance(HealthModel original)
    {
        var newInstance         = CreateInstance<HealthModel>();
        newInstance.CurrentHealth = original.CurrentHealth;
        newInstance.m_LabelName   = original.m_LabelName;
        return newInstance;
    }

    // ✨ 추가 : OnEnable에서 자동 초기화 (ScriptableObject 활성화 시 체력 복원)
    private void OnEnable() { Restore(); }

    // ✨ 추가 : OnValidate로 Inspector 입력 시 범위 검증
    private void OnValidate() { CurrentHealth = Mathf.Clamp(...); }
}
```

**추가된 기능 정리 :**

| 기능 | Legacy | UIToolkit | 설명 |
|------|--------|-----------|------|
| `LabelName` | ❌ 없음 | ✅ 있음 | 체력바 타이틀에 표시할 이름 |
| `CreateInstance()` | ❌ 없음 | ✅ 있음 | 런타임 복사본 생성 (독립 데이터) |
| `OnEnable()` | ❌ 없음 | ✅ 있음 | 씬 시작 시 자동 초기화 |
| `OnValidate()` | ❌ 없음 | ✅ 있음 | Inspector 입력 시 범위 검증 |

---

### 2️⃣ View (UXML + USS)

**📁 파일 :**
- [HealthView.uxml](./UI/HealthView.uxml) - UI 구조 정의
- [HealthBar.uss](./UI/HealthBar.uss) - UI 스타일 정의

> 📖 Legacy에서는 View가 uGUI 컴포넌트(Slider, Text, Button)였습니다.

> UIToolkit에서는 **UXML + USS 파일**이 View 역할을 합니다.

```
┌──────────────────────────────────────────────────────────────────┐
│                    View (UI Toolkit)                              │
├──────────────────────────────────────────────────────────────────┤
│                                                                  │
│  HealthView.uxml (구조) ─── "무엇을 배치할지" (HTML과 유사)      │
│  ├── ProgressBar (name: "health-bar")     ← 체력바               │
│  │   ├── Label (name: "health-bar__status-label") ← 상태 텍스트  │
│  │   └── Label (name: "health-bar__value-label")  ← 수치 텍스트  │
│  └── Button (name: "reset-button")        ← 리셋 버튼            │
│                                                                  │
│  HealthBar.uss (스타일) ─── "어떻게 보일지" (CSS와 유사)         │
│  ├── .health-bar { transition-duration: 0.2s; }                  │
│  ├── .reset-button:hover { scale: 1.1; }                        │
│  └── .status-label { font-size: 34px; }                         │
│                                                                  │
│  UIDocument (컴포넌트) ─── UXML + USS를 화면에 렌더링            │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

**UI 요소 대응표 :**

| 역할 | Legacy (uGUI) | UIToolkit |
|------|---------------|-----------|
| 체력바 | `Slider` | `ProgressBar` |
| 텍스트 | `Text` | `Label` |
| 버튼 | `Button` (uGUI) | `Button` (UIElements) |
| UI 제작 도구 | 씬 Hierarchy + Inspector | **UI Builder** (시각적 에디터) |
| 저장 형식 | 씬 파일에 포함 | UXML(구조) + USS(스타일) 별도 파일 |

---

### 3️⃣ Presenter (중재자)

**📁 파일 :** [HealthPresenter.cs](./Scripts/Presenter/HealthPresenter.cs)

> 📖 기본 역할(Model ↔ View 중재, 이벤트 구독, View 업데이트)은
> [7_MVP(Legacy) README - Presenter](../7_MVP(Legacy)/README.md#3%EF%B8%8F⃣-presenter-프리젠터---중재자)와 동일합니다.

**Legacy와 다른 점 :**

```csharp
public class HealthPresenter : MonoBehaviour
{
    // ✨ 차이 1 : View 참조 방식
    // Legacy : [SerializeField] Slider, Text → Inspector에서 드래그 연결
    // UIToolkit : UIDocument 하나만 연결하고, Q<T>()로 코드에서 검색
    [SerializeField] private UIDocument  m_Document;
    [SerializeField] private HealthModel m_HealthModelAsset;

    private void OnEnable()
    {
        m_Root = m_Document.rootVisualElement;  // ✨ UXML 루트 가져오기

        // ✨ 차이 2 : Q<T>() 쿼리로 UI 요소 검색
        m_HealthBar   = m_Root.Q<ProgressBar>("health-bar");
        m_StatusLabel = m_Root.Q<Label>("health-bar__status-label");
        m_ValueLabel  = m_Root.Q<Label>("health-bar__value-label");

        // ✨ 차이 3 : 버튼 이벤트도 Q<T>()로 찾아서 코드에서 등록
        var resetButton = m_Root.Q<Button>("reset-button");
        resetButton.clicked += RestoreHealth;
    }
}
```

---

### 4️⃣ User Input

**📁 파일 :** [DamageTrigger.cs](./Scripts/User/DamageTrigger.cs)

> 📖 [7_MVP(Legacy) README - ClickDamage](../7_MVP(Legacy)/README.md#📌-핵심-코드-4--null-조건부-연산자-clickdamage)와 동일한 역할입니다.
>
> 마우스 클릭 → Raycast → Presenter.ApplyDamage() 호출.

---

## 📊 코드 구조

### 폴더 구조

```
7_MVP_UIToolkit/
├── Scripts/
│   ├── Model/                               (데이터 계층)
│   │   └── HealthModel.cs                 ← Model : ScriptableObject 기반 체력 데이터
│   │
│   ├── Presenter/                           (중재자 계층)
│   │   └── HealthPresenter.cs             ← Presenter : UIDocument + Model 연결
│   │
│   └── User/                                (사용자 입력)
│       └── DamageTrigger.cs               ← User Input : 마우스 클릭 → 데미지
│
├── UI/                                      (View 계층 - UI Toolkit)
│   ├── HealthView.uxml                    ← View 구조 (HTML과 유사)
│   └── HealthBar.uss                      ← View 스타일 (CSS와 유사)
│
├── Data/
│   └── HealthData.asset                   ← Model 데이터 에셋 파일
│
├── Prefabs/
│   ├── DemoPresenter.prefab               ← UIDocument + Presenter + Trigger 집약
│   ├── HealthPresenter.prefab             ← 체력 프리젠터 프리팹
│   └── ...                                ← 카메라, 라이트 등
│
├── MVP_UIToolkit.unity                      (데모 씬)
└── README.md                                ← 📍 현재 문서
```

> 📖 클래스 다이어그램과 의존 관계도는
> [7_MVP(Legacy) README - 코드 구조](../7_MVP(Legacy)/README.md#-코드-구조)와 동일합니다.
> (Model ← Presenter → View, User → Presenter)

---

## 🔄 실행 흐름

### 초기화 흐름

> 📖 전체적인 흐름(이벤트 구독 → 초기화 → View 업데이트)은
> [7_MVP(Legacy) README - 초기화 흐름](../7_MVP(Legacy)/README.md#1%EF%B8%8F⃣-초기화-흐름)과 동일합니다.

**다른 점만 정리 :**

```
[게임 시작]
    ⬇️
HealthPresenter.OnEnable()
    ⬇️
┌───────────────────────────────────────────────────────────┐
│ ✨ UIToolkit 초기화 (Legacy에는 없는 단계들)               │
│                                                           │
│ 1. NullRefChecker.Validate(this)   → 필수 필드 검증      │
│ 2. m_Document.rootVisualElement    → UXML 루트 가져오기   │
│ 3. Q<ProgressBar>("health-bar")    → UI 요소 검색         │
│    Q<Label>("status-label")                               │
│    Q<Label>("value-label")                                │
│ 4. Q<Button>("reset-button")      → 리셋 버튼 이벤트 등록│
│ 5. m_HealthModelAsset.HealthChanged += OnHealthChanged    │
│ 6. UpdateUI()                      → 최초 동기화          │
└───────────────────────────────────────────────────────────┘
```

**Legacy와의 차이 :**
- Legacy : `Start()`에서 초기화
- UIToolkit : `OnEnable()`에서 초기화 (ScriptableObject의 OnEnable과 맞추기 위해)
- Legacy : `[SerializeField]`로 이미 View 참조가 있음
- UIToolkit : `Q<T>()`로 View 요소를 런타임에 검색해야 함

---

### 데미지 / 리셋 흐름

> 📖 [7_MVP(Legacy) README - 데미지 흐름](../7_MVP(Legacy)/README.md#2%EF%B8%8F⃣-데미지-흐름-마우스-클릭)과 동일합니다.
>
> ```
> 데미지 : User(클릭) → DamageTrigger → Presenter.ApplyDamage() → Model → 이벤트 → Presenter → View
> 리셋   : User(클릭) → View(Button) → Presenter.RestoreHealth() → Model → 이벤트 → Presenter → View
> ```

---

## 💻 주요 코드 분석

> 📖 이벤트 구독/해제, Null 조건부 연산자 등 공통 분석은
> [7_MVP(Legacy) README - 주요 코드 분석](../7_MVP(Legacy)/README.md#-주요-코드-분석)을 참고하세요.

여기서는 **Legacy에 없는 UIToolkit 고유 코드**만 분석합니다.

### 📌 핵심 코드 1 : Q<T>() 쿼리로 UI 요소 검색

**위치 :** HealthPresenter.cs:38-46

```csharp
// UIDocument의 루트 요소를 가져온다
m_Root = m_Document.rootVisualElement;

// UXML 안에서 name 속성으로 UI 요소를 검색하여 C# 변수에 할당
m_HealthBar   = m_Root.Q<ProgressBar>("health-bar");
m_StatusLabel = m_Root.Q<Label>("health-bar__status-label");
m_ValueLabel  = m_Root.Q<Label>("health-bar__value-label");
```

**이해 포인트 :**
- `Q<T>("name")`은 **HTML의 `document.querySelector`** 와 동일한 개념
- UXML에서 `name="health-bar"`로 지정한 요소를 **타입 + 이름**으로 검색
- Legacy처럼 Inspector에서 드래그 앤 드롭할 필요 없이, **코드에서 직접 연결**
- `className:` 파라미터를 쓰면 **CSS 클래스명**으로도 검색 가능

```
UXML (HTML과 비교)                          C# (JavaScript와 비교)
───────────────────────                     ───────────────────────
<ProgressBar name="health-bar">             m_Root.Q<ProgressBar>("health-bar")
<Label name="health-bar__status-label">     m_Root.Q<Label>("health-bar__status-label")

HTML : <div id="health-bar">               JS : document.querySelector("#health-bar")
```

---

### 📌 핵심 코드 2 : ScriptableObject의 CreateInstance

**위치 :** HealthModel.cs:31-39

```csharp
// 런타임에 원본 에셋의 복사본을 생성
public static HealthModel CreateInstance(HealthModel original)
{
    var newInstance = CreateInstance<HealthModel>();

    newInstance.CurrentHealth = original.CurrentHealth;
    newInstance.m_LabelName   = original.m_LabelName;
    return newInstance;
}
```

**이해 포인트 :**
- ScriptableObject는 **에셋 파일**이므로, 런타임에 수정하면 **원본 데이터가 변경**될 수 있음
- `CreateInstance()`로 복사본을 만들면 각 오브젝트가 **독립적인 데이터**로 동작
- 예 : 적 A, 적 B가 같은 HealthData.asset을 참조해도, 복사본을 쓰면 체력이 따로 관리됨

```
원본 에셋 : HealthData.asset (HP: 100)
    │
    ├── CreateInstance() → 복사본 A (HP: 100) → 적 A 전용
    └── CreateInstance() → 복사본 B (HP: 100) → 적 B 전용

적 A가 데미지를 받아도 적 B의 체력에는 영향 없음!
```

---

## ⚖️ 장단점

> 📖 MVP 패턴 자체의 장단점은 [7_MVP(Legacy) README - 장단점](../7_MVP(Legacy)/README.md#-장단점)과 동일합니다.

여기서는 **UIToolkit 버전만의 추가 장단점**을 정리합니다.

### ✅ Legacy 대비 좋아진 점

**1. 씬 독립적인 Model (ScriptableObject)**
- 에셋 파일로 존재하여 씬 전환에도 데이터 유지
- 여러 씬에서 같은 데이터 공유 가능

**2. UI Builder 기반 View 제작**
- UI Builder에서 시각적으로 제작, UXML(구조) + USS(스타일)로 자동 저장
- 구조와 스타일이 분리되어 관리 및 재사용이 용이

**3. Inspector 의존성 제거 (Q<T>() 쿼리)**
- View 요소를 코드에서 직접 검색하여 연결 끊김 문제 해결
- 프리팹 수정 시 참조 소실 걱정 없음

### ❌ Legacy 대비 추가된 복잡도

**1. UI Toolkit 학습 곡선**
- UXML, USS, UIDocument, VisualElement 등 새로운 개념 학습 필요
- uGUI보다 진입 장벽이 높음

**2. 초기화 과정 복잡**
- Legacy : `SerializeField`로 바로 사용
- UIToolkit : `rootVisualElement` → `Q<T>()` 검색 과정 필요

**3. 디버깅 난이도**
- Q<T>()가 null을 반환하면 이름 오타인지 추적이 어려울 수 있음
- uGUI는 Inspector에서 연결 상태가 바로 보임

---

## 🎓 학습 정리

### 핵심 요약

```
MVP 패턴의 구조와 흐름          → Legacy와 100% 동일
Model을 무엇으로 만들었느냐     → MonoBehaviour → ScriptableObject
View를 무엇으로 만들었느냐      → uGUI → UI Toolkit (UI Builder → UXML + USS)
View에 어떻게 접근하느냐        → [SerializeField] → Q<T>() 쿼리
```

### Legacy → UIToolkit 전환 시 변경 포인트

```
1. Model : MonoBehaviour → ScriptableObject
   └── CreateAssetMenu, CreateInstance, OnValidate 추가

2. View : Slider/Text/Button → UXML + USS
   └── UIDocument 컴포넌트로 로드

3. Presenter :
   └── SerializeField → Q<T>() 쿼리로 View 요소 검색
   └── Start() → OnEnable() 초기화 시점 변경

4. User Input : 동일 (Raycast → Presenter 호출)
```

### 관련 패턴

| 패턴 | 관계 |
|------|------|
| **MVP (Legacy)** | 동일한 패턴을 uGUI로 구현한 기본 버전 |
| **MVVM (UIToolkit)** | UI Toolkit + **데이터 바인딩**으로 수동 UpdateUI 제거 (다음 학습) |

### 마무리

MVP UIToolkit은 **패턴 자체는 Legacy와 동일**하지만, **Unity의 최신 UI 시스템과 ScriptableObject**를 활용하여 더 깔끔하고 유지보수하기 좋은 구조를 제공합니다.

**기억할 점 :**
- ✅ ScriptableObject Model로 씬 독립적인 데이터 관리
- ✅ UI Builder에서 시각적으로 UI 제작 → UXML(구조) + USS(스타일)로 자동 저장
- ✅ Q<T>()로 Inspector 의존 없이 코드에서 UI 요소 검색
- ⚠️ 여전히 Presenter가 수동으로 UpdateUI()를 호출해야 함 → **MVVM의 데이터 바인딩이 이를 해결**
- 🎯 다음 단계 : [7_MVVM_UIToolkit](../7_MVVM_UIToolkit/)에서 데이터 바인딩 학습

---

**작성일 :** 2026.02.03
**참고 자료 :**
- Unity Korea - Level Up Your Code with Design Patterns and SOLID
- [7_MVP(Legacy) README](../7_MVP(Legacy)/README.md) - MVP 패턴 기본 개념
- [MVC / MVP / MVVM 정리 블로그](https://tae-woong.tistory.com/164)
