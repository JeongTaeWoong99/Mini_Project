# 🎮 MVVM Pattern - UI Toolkit (Model-View-ViewModel 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [MVP와 무엇이 달라졌는가?](#-mvp와-무엇이-달라졌는가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실무에서의 MVVM 확장](#-실무에서의-mvvm-확장)
- [확장 시나리오](#-확장-시나리오)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**MVVM (Model-View-ViewModel)** 은 MVP의 발전형 아키텍처 패턴으로, **데이터 바인딩**을 통해 View와 ViewModel을 자동으로 동기화하는 것이 핵심입니다.

MVP에서 Presenter가 수동으로 `UpdateUI()`를 호출해야 했던 것과 달리, MVVM에서는 **데이터가 바뀌면 시스템이 알아서 UI를 업데이트**합니다.

> ⚠️ Unity에서 MVVM 패턴을 구현하려면 **UI Toolkit을 필수적으로 사용**해야 합니다.
> UI Toolkit이 제공하는 **런타임 데이터 바인딩** 기능이 MVVM의 핵심이기 때문입니다.

### 📌 핵심 개념

```
MVP  : Model이 바뀌면 → 이벤트 발생 → Presenter가 UpdateUI() 호출 → View 업데이트
MVVM : Model이 바뀌면 → 데이터 바인딩이 감지 → 시스템이 자동으로 View 업데이트
```

**MVP (수동 업데이트) :**
```csharp
// Presenter가 직접 View를 업데이트해야 함
private void UpdateUI()
{
    m_HealthBar.value  = healthRatio * 100f;
    m_StatusLabel.text = health.CurrentHealth.ToString();
    m_ValueLabel.text  = health.CurrentHealth.ToString();
    // ... 값 하나 바뀔 때마다 전부 수동 작성
}
```

**MVVM (자동 업데이트) :**
```csharp
// UpdateUI() 메서드가 없음! 바인딩만 설정하면 끝.
private void SetDataBindings()
{
    healthBarProgress.dataSource = m_HealthModelAsset;
    healthBarProgress.SetBinding("style.backgroundColor", binding);
    // → 이후 Model 값이 바뀌면 시스템이 알아서 UI 업데이트
}
```

---

## 🔄 MVP와 무엇이 달라졌는가?

### 한눈에 비교

| 항목 | **MVP (UIToolkit)** | **MVVM (UIToolkit)** |
|------|---------------------|----------------------|
| **중재자** | Presenter | ViewModel |
| **View 업데이트** | `UpdateUI()` 수동 호출 | **데이터 바인딩** 자동 업데이트 |
| **이벤트 구독** | `HealthChanged += OnHealthChanged` | ❌ **불필요** (바인딩이 자동 감지) |
| **데이터 변환** | Presenter 코드 내부에서 직접 변환 | **ConverterGroup**으로 변환 로직 분리 |
| **바인딩 설정** | ❌ 없음 | UI Builder에서 설정 + 코드에서 설정 |
| **코드량** | 많음 (수동 업데이트 코드) | **적음** (업데이트 코드 제거) |

---

### 1️⃣ 핵심 차이 : UpdateUI() 제거 (데이터 바인딩)

**이것이 MVP → MVVM의 가장 큰 변화입니다.**

**MVP Presenter :**
```csharp
// ❌ Model이 변경될 때마다 이 메서드가 호출되어야 함
private void UpdateUI()
{
    float healthRatio = (float)m_HealthModelAsset.CurrentHealth / m_HealthModelAsset.MaxHealth;
    Color healthColor = Color.Lerp(Color.red, Color.green, healthRatio);

    m_HealthBar.value                       = healthRatio * 100f;
    healthBarProgress.style.backgroundColor = new StyleColor(healthColor);
    m_StatusLabel.text                      = /* 상태 계산 로직 */;
    m_StatusLabel.style.color               = new StyleColor(healthColor);
    m_ValueLabel.text                       = m_HealthModelAsset.CurrentHealth.ToString();
}
// → 이벤트 구독도 필요 : m_Health.HealthChanged += OnHealthChanged;
```

**MVVM ViewModel :**
```csharp
// ✅ UpdateUI()가 없음! SetDataBindings()에서 바인딩만 설정하면 끝.
private void SetDataBindings()
{
    healthBarProgress.dataSource = m_HealthModelAsset;
    healthBarProgress.SetBinding("style.backgroundColor", binding);
    // → 이후 Model.CurrentHealth가 바뀌면 시스템이 알아서 UI 업데이트
}
// → 이벤트 구독도 불필요! (HealthChanged 이벤트 자체가 삭제됨)
```

```
MVP 흐름 :
Model 변경 → HealthChanged 이벤트 → Presenter.OnHealthChanged() → UpdateUI() → View 업데이트
[4단계, 수동]

MVVM 흐름 :
Model 변경 → 데이터 바인딩 시스템이 자동 감지 → View 업데이트
[2단계, 자동]
```

---

### 2️⃣ 핵심 차이 : ConverterGroup (데이터 변환 분리)

MVP에서는 Presenter의 `UpdateUI()` 안에 데이터 변환 로직이 섞여 있었습니다.

MVVM에서는 **ConverterGroup**으로 변환 로직을 별도로 분리합니다 :

```csharp
// Model에서 ConverterGroup 등록 (한 번만 등록하면 어디서든 재사용)
[InitializeOnLoadMethod]
public static void RegisterConverters()
{
    var converter = new ConverterGroup("Int to HealthBar");

    // int → StyleColor 변환 (체력 → 색상)
    converter.AddConverter((ref int value) =>
        new StyleColor(Color.Lerp(Color.red, Color.green, value / (float)k_MaxHealth)));

    // int → string 변환 (체력 → 상태 텍스트)
    converter.AddConverter((ref int value) =>
    {
        float ratio = value / (float)k_MaxHealth;
        return ratio switch
        {
            >= 0 and < 1.0f / 3.0f           => "Danger",
            >= 1.0f / 3.0f and < 2.0f / 3.0f => "Neutral",
            _                                 => "Good"
        };
    });

    ConverterGroups.RegisterConverterGroup(converter);
}
```

**MVP에서의 변환 :** Presenter 코드 안에 직접 작성 → Presenter에 종속

**MVVM에서의 변환 :** ConverterGroup에 등록 → **어디서든 재사용 가능**, UI Builder에서도 선택 가능

---

### 3️⃣ 핵심 차이 : HealthChanged 이벤트 삭제

**MVP Model :**
```csharp
public event Action HealthChanged;    // ✅ 이벤트 있음

public void Decrement(int amount)
{
    CurrentHealth = Mathf.Clamp(...);
    HealthChanged?.Invoke();           // ✅ 수동으로 이벤트 발행
}
```

**MVVM Model :**
```csharp
// public event Action HealthChanged; // ❌ 이벤트 삭제 (주석 처리됨)

public void Decrement(int amount)
{
    CurrentHealth = Mathf.Clamp(...);
    // ❌ Invoke 없음! 데이터 바인딩이 값 변경을 자동 감지
}
```

**왜 이벤트가 필요 없는가?**
- 데이터 바인딩 시스템이 `CurrentHealth` 필드의 값 변화를 **매 프레임 자동으로 감지**
- Presenter처럼 이벤트를 받아서 수동으로 UI를 갱신할 필요가 없음
- Model이 더 깨끗해짐 (순수 데이터 + 조작 메서드만 남음)

---

### 4️⃣ 핵심 차이 : UI Builder에서 바인딩 설정

MVP에서는 모든 View 업데이트가 코드(`UpdateUI()`)에서 이루어졌습니다.

MVVM에서는 **대부분의 바인딩을 UI Builder에서 시각적으로 설정**합니다 :

```
UI Builder에서 설정한 바인딩 (코드 작성 불필요) :
├── ProgressBar.value    ← CurrentHealth (바인딩)
├── ProgressBar.title    ← LabelName (바인딩)
├── StatusLabel.text     ← CurrentHealth + ConverterGroup "Int to HealthBar" (바인딩)
├── StatusLabel.color    ← CurrentHealth + ConverterGroup "Int to HealthBar" (바인딩)
└── ValueLabel.text      ← CurrentHealth (바인딩)

코드에서 설정한 바인딩 (SetDataBindings) :
└── HealthBarProgress.style.backgroundColor ← CurrentHealth + Converter (코드 바인딩)
```

> 💡 즉, **대부분의 바인딩은 UI Builder에서 설정**하고, UI Builder에서 설정하기 어려운
> 바인딩(예 : 내부 요소의 스타일)만 코드에서 `SetBinding()`으로 보충합니다.

---

## 🏗️ 핵심 구성요소

### 1️⃣ Model (ScriptableObject)

**📁 파일 :** [HealthModel.cs](./Scripts/Model/HealthModel.cs)

> 📖 기본 역할(데이터 보유, Clamp 범위 보장, CreateInstance)은
> [7_MVP_UIToolkit README - Model](../7_MVP_UIToolkit/README.md#1%EF%B8%8F⃣-model-scriptableobject)과 동일합니다.

**MVP Model과 다른 점 :**

| 항목 | MVP Model | MVVM Model |
|------|-----------|------------|
| `HealthChanged` 이벤트 | ✅ 있음 | ❌ **삭제** (바인딩이 자동 감지) |
| `Invoke()` 호출 | ✅ 있음 (각 메서드에서) | ❌ **없음** |
| `CurrentHealth` 접근 제한 | `private` (프로퍼티로 접근) | **`public` 필드** (바인딩 접근 필요) |
| `LabelName` 접근 제한 | `private` + 프로퍼티 | **`public` 필드** (바인딩 접근 필요) |
| `RegisterConverters()` | ❌ 없음 | ✅ **추가** (ConverterGroup 등록) |
| `[InitializeOnLoadMethod]` | ❌ 없음 | ✅ **추가** (에디터 로드 시 자동 실행) |

> ⚠️ MVVM에서 `CurrentHealth`와 `LabelName`이 `public` 필드인 이유 :
> 데이터 바인딩 시스템이 해당 필드에 직접 접근해야 하기 때문입니다.

---

### 2️⃣ View (UXML + USS + 바인딩)

**📁 파일 :**
- [HealthView.uxml](./UI/HealthView.uxml) - UI 구조 + **바인딩 설정**
- [HealthBar.uss](./UI/HealthBar.uss) - UI 스타일

> 📖 UI Builder, UXML, USS의 기본 개념은
> [7_MVP_UIToolkit README - View](../7_MVP_UIToolkit/README.md#2%EF%B8%8F⃣-view-uxml--uss)와 동일합니다.

**MVP View와 다른 점 : UXML에 바인딩이 포함됨**

```xml
<!-- MVP의 UXML : 구조만 정의 -->
<engine:Label name="health-bar__value-label" />

<!-- MVVM의 UXML : 구조 + 바인딩까지 정의 -->
<engine:Label name="health-bar__value-label">
    <Bindings>
        <engine:DataBinding
            property="text"
            data-source-path="CurrentHealth"
            binding-mode="ToTarget" />
    </Bindings>
</engine:Label>
```

이 바인딩은 **UI Builder에서 시각적으로 설정**한 것이 UXML에 자동 저장된 결과입니다.

**바인딩 설정 내용 :**

| UI 요소 | 바인딩 대상 | 변환 | 설정 위치 |
|---------|------------|------|----------|
| `ProgressBar.value` | `CurrentHealth` | 없음 (int → float 자동) | UI Builder |
| `ProgressBar.title` | `LabelName` | 없음 (string 그대로) | UI Builder |
| `StatusLabel.text` | `CurrentHealth` | `ConverterGroup "Int to HealthBar"` | UI Builder |
| `StatusLabel.color` | `CurrentHealth` | `ConverterGroup "Int to HealthBar"` | UI Builder |
| `ValueLabel.text` | `CurrentHealth` | 없음 (int → string 자동) | UI Builder |
| `ProgressBar 배경색` | `CurrentHealth` | 코드에서 Converter 추가 | **코드** |

---

### 3️⃣ ViewModel (중재자)

**📁 파일 :** [HealthViewModel.cs](./Scripts/ViewModel/HealthViewModel.cs)

> 📖 기본 역할(Model ↔ View 중재, 리셋/데미지 명령 전달)은
> [7_MVP_UIToolkit README - Presenter](../7_MVP_UIToolkit/README.md#3%EF%B8%8F⃣-presenter-중재자)와 동일합니다.

**MVP Presenter와 비교하면 얼마나 코드가 줄었는가?**

```
MVP Presenter 가 하는 일 :                    MVVM ViewModel 가 하는 일 :
─────────────────────────                     ─────────────────────────
✅ Q<T>()로 UI 요소 전부 검색                  ✅ Q<T>()로 버튼만 검색
✅ 이벤트 구독 (HealthChanged)                 ❌ 불필요 (삭제)
✅ 이벤트 해제 (OnDisable)                     ❌ 불필요 (삭제)
✅ UpdateUI() 메서드 작성                      ❌ 불필요 (삭제)
✅ 데이터 변환 코드 작성                        ❌ 불필요 (ConverterGroup이 담당)
✅ Damage/Heal/Reset 명령 전달                 ✅ 동일
                                               ✅ SetDataBindings() (바인딩 설정)
```

**제거된 코드 :**
- `HealthChanged += OnHealthChanged` (이벤트 구독)
- `HealthChanged -= OnHealthChanged` (이벤트 해제)
- `OnHealthChanged()` (이벤트 핸들러)
- `UpdateUI()` 메서드 전체 (수동 업데이트)
- `m_HealthBar`, `m_StatusLabel`, `m_ValueLabel` 변수 (UI 요소 참조)

**추가된 코드 :**
- `SetDataBindings()` : 코드에서만 가능한 바인딩 설정 (내부 요소 스타일)

---

### 4️⃣ User Input

**📁 파일 :** [DamageTrigger.cs](./Scripts/User/DamageTrigger.cs)

> 📖 [7_MVP_UIToolkit README - User Input](../7_MVP_UIToolkit/README.md#4%EF%B8%8F⃣-user-input)과 동일합니다.
>
> `HealthPresenter` → `HealthViewModel`으로 참조 타입만 변경.

---

## 📊 코드 구조

### 폴더 구조

```
7_MVVM_UIToolkit/
├── Scripts/
│   ├── Model/                               (데이터 계층)
│   │   └── HealthModel.cs                 ← Model : ScriptableObject + ConverterGroup
│   │
│   ├── ViewModel/                           (중재자 계층)
│   │   └── HealthViewModel.cs             ← ViewModel : 바인딩 설정 (UpdateUI 없음!)
│   │
│   └── User/                                (사용자 입력)
│       └── DamageTrigger.cs               ← User Input : 마우스 클릭 → 데미지
│
├── UI/                                      (View 계층 - UI Toolkit)
│   ├── HealthView.uxml                    ← View 구조 + 바인딩 설정 (UI Builder에서 생성)
│   └── HealthBar.uss                      ← View 스타일 (UI Builder에서 생성)
│
├── Data/
│   └── HealthData.asset                   ← Model 데이터 에셋 파일
│
├── Prefabs/
│   ├── DemoPresenter.prefab               ← UIDocument + ViewModel + Trigger 집약
│   └── ...
│
├── MVVM_UIToolkit.unity                     (데모 씬)
└── README.md                                ← 📍 현재 문서
```

> 📖 클래스 다이어그램과 의존 관계도는
> [7_MVP(Legacy) README - 코드 구조](../7_MVP(Legacy)/README.md#-코드-구조)와 동일한 구조입니다.
> (Model ← ViewModel → View, User → ViewModel)

---

## 🔄 실행 흐름

### 초기화 흐름

```
[게임 시작]
    ⬇️
HealthViewModel.OnEnable()
    ⬇️
┌───────────────────────────────────────────────────────────┐
│ 1. NullRefChecker.Validate(this)  → 필수 필드 검증       │
│ 2. m_Document.rootVisualElement   → UXML 루트 가져오기    │
│ 3. RegisterElements()             → 리셋 버튼 이벤트 등록 │
│ 4. SetDataBindings()              → ✨ 데이터 바인딩 설정 │
│                                                           │
│ ⚠️ MVP와 비교하여 없는 것들 :                             │
│    ❌ Q<ProgressBar>, Q<Label> (UI 요소 검색 불필요)      │
│    ❌ HealthChanged 이벤트 구독 (불필요)                   │
│    ❌ UpdateUI() 호출 (불필요)                             │
└───────────────────────────────────────────────────────────┘
    ⬇️
✅ 바인딩 완료! 이후 Model 변경 시 자동 업데이트
```

---

### 데미지 흐름 (MVP vs MVVM 비교)

```
┌─────────────────────────────────────────────────────────────────┐
│  MVP 흐름 (수동)                                                 │
│                                                                 │
│  User(클릭) → Presenter.ApplyDamage()                           │
│            → Model.Decrement()                                  │
│            → HealthChanged?.Invoke()         ← 이벤트 발행       │
│            → Presenter.OnHealthChanged()     ← 이벤트 수신       │
│            → Presenter.UpdateUI()            ← 수동 업데이트     │
│            → View (slider.value = ...)       ← 수동 값 설정      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│  MVVM 흐름 (자동)                                                │
│                                                                 │
│  User(클릭) → ViewModel.ApplyDamage()                           │
│            → Model.Decrement()                                  │
│            → (데이터 바인딩이 값 변경 자동 감지)                   │
│            → View 자동 업데이트                                   │
│                                                                 │
│  ✅ 이벤트 발행/수신 없음                                        │
│  ✅ UpdateUI() 호출 없음                                         │
│  ✅ 수동 값 설정 없음                                            │
└─────────────────────────────────────────────────────────────────┘
```

### 리셋 흐름

> 📖 [7_MVP_UIToolkit README - 리셋 흐름](../7_MVP_UIToolkit/README.md#데미지--리셋-흐름)과 동일한 구조입니다.
>
> `User(버튼) → View → ViewModel.RestoreHealth() → Model.Restore() → 바인딩 자동 업데이트`

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : SetDataBindings() - 코드에서 바인딩 설정

**위치 :** HealthViewModel.cs:54-81

```csharp
private void SetDataBindings()
{
    var healthBar         = m_Root.Q<ProgressBar>("health-bar");
    var healthBarProgress = healthBar?.Q<VisualElement>(className: "unity-progress-bar__progress");

    if (healthBarProgress != null)
    {
        // 1. 데이터 소스 지정 : 이 UI 요소가 참조할 데이터 객체
        healthBarProgress.dataSource = m_HealthModelAsset;

        // 2. 바인딩 생성 : 어떤 프로퍼티를, 어떤 방향으로 바인딩할지 정의
        var binding = new DataBinding
        {
            dataSourcePath = new PropertyPath(nameof(HealthModel.CurrentHealth)),
            bindingMode    = BindingMode.ToTarget,  // Model → View 단방향
        };

        // 3. 컨버터 추가 : int 값을 StyleColor로 변환
        binding.sourceToUiConverters.AddConverter((ref int value) =>
            new StyleColor(Color.Lerp(Color.red, Color.green,
                (float)value / (float)m_HealthModelAsset.MaxHealth)));

        // 4. 바인딩 등록 : UI 요소의 특정 프로퍼티에 바인딩 연결
        healthBarProgress.SetBinding("style.backgroundColor", binding);
    }
}
```

**이해 포인트 :**
- `dataSource` : 이 UI 요소가 바라볼 **데이터 객체** (ScriptableObject)
- `dataSourcePath` : 데이터 객체 안에서 **어떤 필드**를 바인딩할지 (`CurrentHealth`)
- `bindingMode = BindingMode.ToTarget` : **Model → View 단방향** (Model이 변하면 View가 따라감)
- `sourceToUiConverters` : 데이터 타입 변환 (int → StyleColor)
- `SetBinding("style.backgroundColor", binding)` : **어떤 UI 프로퍼티**에 바인딩할지

```
바인딩의 구조 :
    ┌──────────┐              ┌──────────────┐              ┌───────────────┐
    │  Model   │  ──────────▶ │  Converter   │  ──────────▶ │   View UI     │
    │          │   자동 감지   │              │   자동 변환   │               │
    │  int 90  │              │  int → Color │              │  배경색: 주황  │
    └──────────┘              └──────────────┘              └───────────────┘
     dataSource              sourceToUiConverters           SetBinding(...)
```

---

### 📌 핵심 코드 2 : ConverterGroup - 데이터 변환 재사용

**위치 :** HealthModel.cs:46-72

```csharp
[InitializeOnLoadMethod]
public static void RegisterConverters()
{
    float HealthRatio(int health) => health / (float)k_MaxHealth;

    // ConverterGroup 생성 : 이름으로 UI Builder에서 선택 가능
    var converter = new ConverterGroup("Int to HealthBar");

    // 변환 1 : int → StyleColor (체력 → 색상)
    converter.AddConverter((ref int value) =>
        new StyleColor(Color.Lerp(Color.red, Color.green, HealthRatio(value))));

    // 변환 2 : int → string (체력 → 상태 텍스트)
    converter.AddConverter((ref int value) =>
    {
        float healthRatio = HealthRatio(value);
        return healthRatio switch
        {
            >= 0 and < 1.0f / 3.0f           => "Danger",
            >= 1.0f / 3.0f and < 2.0f / 3.0f => "Neutral",
            _                                 => "Good"
        };
    });

    // 등록 : UI Builder에서 "Int to HealthBar"로 접근 가능
    ConverterGroups.RegisterConverterGroup(converter);
}
```

**이해 포인트 :**
- `[InitializeOnLoadMethod]` : Unity 에디터가 로드될 때 **자동으로 실행**되는 메서드
- `ConverterGroup("이름")` : 이름을 지정하면 **UI Builder에서 해당 이름으로 선택** 가능
- 같은 ConverterGroup에 **여러 변환을 등록** 가능 (int → Color, int → string 등)
- 출력 타입에 따라 **자동으로 적절한 변환이 선택**됨
  - `text` 프로퍼티에 바인딩 → `int → string` 변환 사용
  - `style.color` 프로퍼티에 바인딩 → `int → StyleColor` 변환 사용

```
ConverterGroup "Int to HealthBar"
├── int → StyleColor : Color.Lerp(red, green, ratio)   ← style.color에 바인딩 시 사용
└── int → string     : "Danger" / "Neutral" / "Good"   ← text에 바인딩 시 사용
```

---

### 📌 핵심 코드 3 : UXML의 Bindings 태그

**위치 :** HealthView.uxml:5-8

```xml
<engine:Label name="health-bar__status-label">
    <Bindings>
        <!-- text 프로퍼티를 CurrentHealth에 바인딩, ConverterGroup으로 변환 -->
        <engine:DataBinding
            property="text"
            data-source-path="CurrentHealth"
            binding-mode="ToTarget"
            source-to-ui-converters="Int to HealthBar" />

        <!-- style.color도 같은 CurrentHealth에 바인딩 -->
        <engine:DataBinding
            property="style.color"
            data-source-path="CurrentHealth"
            binding-mode="ToTarget"
            source-to-ui-converters="Int to HealthBar" />
    </Bindings>
</engine:Label>
```

**이해 포인트 :**
- 이 XML은 **직접 작성한 것이 아니라 UI Builder에서 바인딩을 설정하면 자동 생성**되는 코드
- `property` : 바인딩할 **UI 프로퍼티** (text, style.color, value 등)
- `data-source-path` : Model에서 가져올 **데이터 필드** (CurrentHealth)
- `binding-mode="ToTarget"` : **Model → View** 단방향
- `source-to-ui-converters` : 적용할 **ConverterGroup 이름**
- 하나의 데이터(`CurrentHealth`)에 **여러 프로퍼티를 동시에 바인딩** 가능

---

## ⚖️ 장단점

> 📖 아키텍처 패턴(MVP/MVVM) 공통 장단점은 [7_MVP(Legacy) README - 장단점](../7_MVP(Legacy)/README.md#-장단점)을 참고하세요.

### ✅ MVP 대비 좋아진 점

**1. UpdateUI() 제거 - 코드량 대폭 감소**
- View 업데이트 코드를 작성할 필요 없음
- UI 요소가 추가되어도 ViewModel 코드 수정 불필요 (UI Builder에서 바인딩만 추가)

**2. 이벤트 구독/해제 불필요**
- `HealthChanged` 이벤트 자체가 삭제됨
- 이벤트 해제 누락으로 인한 메모리 누수 위험 제거

**3. ConverterGroup으로 변환 로직 재사용**
- 변환 로직을 한 번 등록하면 여러 바인딩에서 재사용 가능
- UI Builder에서 이름으로 선택하여 적용

**4. UI Builder에서 바인딩 시각적 설정**
- 코드 작성 없이 UI Builder에서 바인딩을 드래그 앤 드롭으로 설정
- 어떤 데이터가 어떤 UI에 연결되었는지 한눈에 확인 가능

### ❌ MVP 대비 추가된 복잡도

**1. 데이터 바인딩 개념 학습 곡선**
- DataBinding, BindingMode, PropertyPath, ConverterGroup 등 새로운 개념
- MVP의 직관적인 `slider.value = ...`보다 이해하기 어려움

**2. 초기 설계 복잡도**
- ConverterGroup 등록, 바인딩 설정 등 초기 설정이 필요
- 간단한 UI에서는 오히려 MVP가 빠를 수 있음

**3. 바인딩 비용**
- 데이터 바인딩 시스템이 매 프레임 값 변경을 감지하는 **런타임 비용** 존재
- 바인딩 수가 많아지면 성능에 영향을 줄 수 있음

**4. 디버깅 방식의 차이**
- MVP는 `UpdateUI()`에 브레이크포인트를 걸어 코드 레벨에서 직접 디버깅 가능
- MVVM은 바인딩이 자동으로 동작하므로 코드 추적 방식의 디버깅이 어려움
- 대신, Unity는 **UI Toolkit Debugger** (`Window > UI Toolkit > Debugger`)를 제공
  - UI 요소의 계층 구조, 스타일, 프로퍼티를 **실시간으로 검사** 가능
  - 바인딩 설정은 **UI Builder의 Inspector**에서 확인 가능
- 즉, "디버깅이 불가능한 것"이 아니라 **디버깅 방식이 다름** (코드 추적 → UI 검사 도구)

---

## 🏭 실무에서의 MVVM 확장

### 기본 원칙 : "기능(Feature) 단위로 분리"

실무에서는 **기능(Feature) 단위**로 Model-ViewModel-View 세트를 만듭니다.

```
실무 MVVM 구조 예시 :

Features/
├── Health/                          ← 체력 기능
│   ├── Model/  HealthModel.cs
│   ├── ViewModel/  HealthViewModel.cs
│   └── View/  HealthView.uxml
│
├── Inventory/                       ← 인벤토리 기능
│   ├── Model/  InventoryModel.cs
│   ├── ViewModel/  InventoryViewModel.cs
│   └── View/  InventoryView.uxml
│
├── Quest/                           ← 퀘스트 기능
│   ├── Model/  QuestModel.cs
│   ├── ViewModel/  QuestViewModel.cs
│   └── View/  QuestView.uxml
│
└── Minimap/                         ← 미니맵 기능
    ├── Model/  MinimapModel.cs
    ├── ViewModel/  MinimapViewModel.cs
    └── View/  MinimapView.uxml
```

### 핵심 규칙

```
✅ 1 기능(Feature) = 1 Model + 1 ViewModel + 1 View (기본 단위)
✅ Model은 순수 데이터만 보유 (UI 로직 없음)
✅ ViewModel은 하나의 View만 담당 (단일 책임)
✅ View는 ViewModel만 알고, Model은 직접 모름
```

### 확장 시 자주 발생하는 관계 패턴

| 패턴 | 설명 | 예시 |
|------|------|------|
| **1:1:1** | 기본. 한 기능에 하나씩 | Health, Inventory 각각 |
| **1 Model : N Views** | 같은 데이터를 여러 화면에서 표시 | HUD 체력바 + 캐릭터창 체력바 |
| **N Models : 1 ViewModel** | 여러 데이터를 하나의 화면에서 조합 | 캐릭터 정보창 (체력 + 스탯 + 장비) |

---

## 📈 확장 시나리오

### 📋 시나리오 목차
- [시나리오 1 : 기존 UI에 요소 추가](#시나리오-1--기존-ui에-요소-추가)
- [시나리오 2 : 새로운 기능의 UI 추가](#시나리오-2--새로운-기능의-ui-추가)
- [시나리오 3 : 같은 데이터를 여러 화면에서 표시](#시나리오-3--같은-데이터를-여러-화면에서-표시)
- [시나리오 4 : 여러 데이터를 하나의 화면에서 조합](#시나리오-4--여러-데이터를-하나의-화면에서-조합)

---

### 시나리오 1 : 기존 UI에 요소 추가

> **예시 :** 기존 체력바 옆에 "체력 퍼센트 텍스트" 추가

```
변경 범위 :
├── Model          → ❌ 변경 없음 (CurrentHealth 데이터 이미 존재)
├── ViewModel      → ❌ 변경 없음 (바인딩이 자동 처리)
└── View (UXML)    → ✅ UI Builder에서 Label 추가 + 바인딩 설정
```

**이것이 MVVM의 가장 큰 장점입니다.**

MVP였다면 :
```csharp
// MVP : 요소 추가할 때마다 Presenter 코드 수정 필요
private Label m_PercentLabel;                                    // ← 변수 추가

private void UpdateUI()
{
    // ... 기존 코드 ...
    m_PercentLabel.text = $"{healthRatio * 100:F0}%";            // ← 코드 추가
}
```

MVVM에서는 :
```
UI Builder에서 :
1. Label 추가
2. Data Source 설정 (HealthModel)
3. text 프로퍼티에 CurrentHealth 바인딩
4. 필요하면 ConverterGroup 적용 (int → "85%" 형식)
→ 코드 변경 0줄!
```

```
⭐ 핵심 : 같은 Model의 데이터를 표시하는 UI 추가는 코드 변경이 필요 없다!
```

---

### 시나리오 2 : 새로운 기능의 UI 추가

> **예시 :** 게임에 "실드(Shield)" 시스템 추가

**이 경우에는 새로운 Model + ViewModel + View 세트가 필요합니다.**

```
추가되는 파일 :
├── Model/      ShieldModel.cs           ← 새로 생성 (ScriptableObject)
├── ViewModel/  ShieldViewModel.cs       ← 새로 생성 (MonoBehaviour)
└── View/       ShieldView.uxml          ← 새로 생성 (UI Builder에서 제작)
```

**ShieldModel.cs (예시) :**
```csharp
[CreateAssetMenu(fileName = "ShieldData", menuName = "MVVM/ShieldModel")]
public class ShieldModel : ScriptableObject
{
    public const int k_MaxShield = 50;

    public int    CurrentShield = k_MaxShield;
    public string LabelName     = "Shield";

    public void Decrement(int amount)
    {
        CurrentShield = Mathf.Clamp(CurrentShield - amount, 0, k_MaxShield);
    }

    // ConverterGroup 등록
    [InitializeOnLoadMethod]
    public static void RegisterConverters()
    {
        var converter = new ConverterGroup("Int to ShieldBar");
        converter.AddConverter((ref int value) =>
            new StyleColor(Color.Lerp(Color.gray, Color.cyan,
                value / (float)k_MaxShield)));
        ConverterGroups.RegisterConverterGroup(converter);
    }
}
```

**ShieldViewModel.cs (예시) :**
```csharp
public class ShieldViewModel : MonoBehaviour
{
    [SerializeField] private UIDocument  m_Document;
    [SerializeField] private ShieldModel m_ShieldModelAsset;

    private VisualElement m_Root;

    private void OnEnable()
    {
        m_Root = m_Document.rootVisualElement;
        SetDataBindings();
    }

    private void SetDataBindings()
    {
        // UI Builder에서 대부분 설정, 코드 바인딩이 필요한 것만 여기서
    }

    public void ApplyDamage(int amount) => m_ShieldModelAsset.Decrement(amount);
}
```

```
⭐ 핵심 : 완전히 새로운 데이터를 다루는 UI는 Model + ViewModel + View 세트가 필요하다!
         기존 코드는 건드리지 않는다. (OCP 원칙 : 확장에는 열려있고, 수정에는 닫혀있다)
```

---

### 시나리오 3 : 같은 데이터를 여러 화면에서 표시

> **예시 :** HUD 체력바 + 캐릭터 정보창 체력바 (같은 HealthModel 데이터)

```
구조 :
                    ┌─ HealthViewModel_HUD     ─── HudHealthView.uxml     (HUD 체력바)
HealthModel.asset ──┤
                    └─ HealthViewModel_Panel   ─── PanelHealthView.uxml   (캐릭터창 체력바)

즉, 1 Model : 2 ViewModels : 2 Views
```

```
추가되는 파일 :
├── Model          → ❌ 변경 없음 (같은 HealthModel.asset 공유)
├── ViewModel      → ✅ HealthViewModel_Panel.cs 추가
└── View           → ✅ PanelHealthView.uxml 추가 (UI Builder에서 제작)
```

**왜 ViewModel이 별도로 필요한가?**
- 각 View의 **UI 요소가 다르기 때문** (HUD는 간단한 바, 캐릭터창은 상세 정보)
- 각 View에서 **코드 바인딩이 필요한 부분이 다를 수 있음**
- 각 View의 **버튼/상호작용이 다를 수 있음**

> 💡 단, 두 View가 거의 동일하다면 하나의 ViewModel을 공유할 수도 있습니다.
> 실무에서는 **View의 복잡도에 따라** 판단합니다.

```
⭐ 핵심 : 같은 ScriptableObject를 dataSource로 설정하면,여러 View가 같은 데이터를 자동으로 동기화한다!
          Model 변경 시 → 모든 View가 동시에 업데이트됨.
```

---

### 시나리오 4 : 여러 데이터를 하나의 화면에서 조합

> **예시 :** "캐릭터 정보창"에 체력 + 스탯 + 장비 정보를 모두 표시

```
구조 :
HealthModel.asset  ──┐
                     │
StatModel.asset    ──┼── CharacterInfoViewModel ─── CharacterInfoView.uxml
                     │
EquipmentModel.asset─┘

즉, 3 Models : 1 ViewModel : 1 View
```

```
파일 구조 :
├── Model/
│   ├── HealthModel.cs       ← 기존 (변경 없음)
│   ├── StatModel.cs         ← 새로 생성
│   └── EquipmentModel.cs    ← 새로 생성
│
├── ViewModel/
│   └── CharacterInfoViewModel.cs    ← 새로 생성 (3개의 Model 참조)
│
└── View/
    └── CharacterInfoView.uxml       ← 새로 생성 (UI Builder에서 제작)
```

**CharacterInfoViewModel.cs (예시) :**
```csharp
public class CharacterInfoViewModel : MonoBehaviour
{
    [SerializeField] private UIDocument     m_Document;
    [SerializeField] private HealthModel    m_HealthModel;     // 체력 데이터
    [SerializeField] private StatModel      m_StatModel;       // 스탯 데이터
    [SerializeField] private EquipmentModel m_EquipmentModel;  // 장비 데이터

    private VisualElement m_Root;

    private void OnEnable()
    {
        m_Root = m_Document.rootVisualElement;

        // 각 UI 섹션에 다른 dataSource 연결
        SetHealthBindings();
        SetStatBindings();
        SetEquipmentBindings();
    }

    private void SetHealthBindings()
    {
        var healthSection        = m_Root.Q<VisualElement>("health-section");
        healthSection.dataSource = m_HealthModel;    // 체력 섹션 → HealthModel
        // 나머지는 UI Builder에서 바인딩 설정
    }

    private void SetStatBindings()
    {
        var statSection        = m_Root.Q<VisualElement>("stat-section");
        statSection.dataSource = m_StatModel;        // 스탯 섹션 → StatModel
    }

    private void SetEquipmentBindings()
    {
        var equipSection        = m_Root.Q<VisualElement>("equipment-section");
        equipSection.dataSource = m_EquipmentModel;  // 장비 섹션 → EquipmentModel
    }
}
```

```
⭐ 핵심 : UI 요소별로 서로 다른 dataSource를 설정할 수 있다!
         하나의 View 안에서 여러 Model의 데이터를 조합하여 표시 가능.
```

---

### 시나리오 종합 정리

| 시나리오 | Model | ViewModel | View | 코드 변경량 |
|---------|-------|-----------|------|------------|
| **1. 기존 UI에 요소 추가** | 변경 없음 | 변경 없음 | UI Builder에서 추가 | **0줄** |
| **2. 새로운 기능 추가** | 새로 생성 | 새로 생성 | 새로 생성 | 기존 코드 변경 없음 |
| **3. 같은 데이터, 여러 화면** | 공유 | 추가 (또는 공유) | 새로 생성 | 기존 코드 변경 없음 |
| **4. 여러 데이터, 하나의 화면** | 기존 재사용 | 새로 생성 | 새로 생성 | 기존 코드 변경 없음 |

> 💡 **공통점 :** 모든 시나리오에서 **기존 코드를 수정하지 않고** 확장합니다. (OCP 원칙!)
>
> 💡 **MVP와의 차이 :** MVP에서는 시나리오 1에서도 Presenter의 `UpdateUI()` 코드 수정이 필요하지만,
> MVVM에서는 UI Builder에서 바인딩만 추가하면 코드 변경이 **0줄**입니다.

---

## 🎓 학습 정리

### 핵심 요약

```
MVVM의 핵심 = 데이터 바인딩
    → Model이 바뀌면 시스템이 알아서 View를 업데이트
    → UpdateUI() 코드 불필요
    → 이벤트 구독/해제 불필요
    → ConverterGroup으로 데이터 변환 분리 및 재사용
```

### MVP → MVVM 전환 시 변경 포인트

```
1. Model :
   └── HealthChanged 이벤트 삭제 (바인딩이 자동 감지)
   └── 필드를 public으로 변경 (바인딩 접근 필요)
   └── RegisterConverters() 추가 (ConverterGroup 등록)

2. View (UXML) :
   └── <Bindings> 태그 추가 (UI Builder에서 설정 → 자동 생성)

3. Presenter → ViewModel :
   └── UpdateUI() 삭제
   └── 이벤트 구독/해제 삭제
   └── UI 요소 변수(ProgressBar, Label) 삭제
   └── SetDataBindings() 추가 (코드 바인딩)

4. User Input : 동일 (Presenter → ViewModel 참조 타입만 변경)
```

### MVP vs MVVM 최종 비교

| 항목 | MVP | MVVM |
|------|-----|------|
| **UI 업데이트** | 수동 (`UpdateUI()`) | 자동 (데이터 바인딩) |
| **이벤트** | 필요 (`HealthChanged`) | 불필요 (삭제) |
| **코드량** | 많음 | 적음 |
| **데이터 변환** | Presenter 코드 내부 | ConverterGroup (분리/재사용) |
| **초기 설정** | 간단 | 복잡 (바인딩 설정) |
| **런타임 비용** | 낮음 (이벤트 기반) | 있음 (매 프레임 감지) |
| **디버깅** | 코드 추적 (브레이크포인트) | UI Toolkit Debugger (UI 검사 도구) |
| **적합한 상황** | 간단한 UI, 빠른 구현 | 복잡한 UI, 많은 데이터 바인딩 |

### 관련 패턴

| 패턴 | 관계 |
|------|------|
| **MVP (Legacy)** | uGUI 기반 수동 업데이트 방식 |
| **MVP (UIToolkit)** | UI Toolkit 기반이지만 여전히 수동 업데이트 |
| **Observer** | MVP의 이벤트 구독이 Observer 패턴. <br/>MVVM은 이를 바인딩으로 대체 |

### 마무리

MVVM 패턴은 **데이터 바인딩으로 View 업데이트 코드를 제거**하는 것이 핵심입니다.

**기억할 점 :**
- ✅ 데이터 바인딩 = Model 값이 바뀌면 시스템이 알아서 View 업데이트
- ✅ UpdateUI() 코드와 이벤트 구독이 모두 제거되어 코드가 깔끔해짐
- ✅ ConverterGroup으로 데이터 변환 로직을 분리하고 재사용
- ✅ UI Builder에서 시각적으로 바인딩 설정 가능
- ⚠️ 바인딩 비용(매 프레임 감지)과 초기 설계 복잡도가 트레이드오프
- ⚠️ Unity에서 MVVM을 쓰려면 UI Toolkit이 필수
- 🎯 간단한 UI → MVP, 복잡한 UI + 많은 데이터 → MVVM이 적합

---

**작성일 :** 2026.02.03
**참고 자료 :**
- Unity Korea - Level Up Your Code with Design Patterns and SOLID
- [7_MVP(Legacy) README](../7_MVP(Legacy)/README.md) - MVP 패턴 기본 개념
- [7_MVP_UIToolkit README](../7_MVP_UIToolkit/README.md) - UI Toolkit 기반 MVP
- [MVC / MVP / MVVM 정리 블로그](https://tae-woong.tistory.com/164)
