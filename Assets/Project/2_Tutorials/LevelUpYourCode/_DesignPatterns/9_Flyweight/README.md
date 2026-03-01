# 🪶 Flyweight Pattern (플라이웨이트 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [왜 Flyweight Pattern이 필요한가?](#-왜-flyweight-pattern이-필요한가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [실제 사용 사례](#-실제-사용-사례)
- [Object Pool과의 비교](#-object-pool과의-비교)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**Flyweight Pattern**은 **구조 패턴(Structural Pattern)** 중 하나로, 대량의 유사한 객체가 공유할 수 있는 데이터를 분리·공유함으로써 **메모리 사용량을 최소화**하는 패턴입니다.

"플라이웨이트(flyweight)"는 권투의 최경량급을 의미하며, 이름 그대로 **객체를 최대한 가볍게 만드는 것**이 핵심입니다.

### 📌 핵심 개념

```
변하지 않는 공유 데이터(내재적 데이터)는 한 곳에 보관하고,
인스턴스마다 달라지는 데이터(외재적 데이터)만 각 객체가 보유한다!
```

**핵심 용어 :**

| 용어 | 설명 | 예시 |
|------|------|------|
| **내재적 데이터 (Intrinsic)** | 모든 인스턴스가 공유하는 불변 데이터 | 함선 이름, 속도, 공격력, 아이콘 |
| **외재적 데이터 (Extrinsic)** | 인스턴스마다 고유하게 달라지는 데이터 | 함선 체력, 현재 위치 |

**일반적인 방법 :**
```csharp
// 100개 함선 각자가 동일한 데이터를 복사해서 보관
public class UnrefactoredShip : MonoBehaviour
{
    public string    UnitName;     // 100개 모두 동일한 값
    public float     Speed;        // 100개 모두 동일한 값
    public int       AttackPower;  // 100개 모두 동일한 값
    public Texture2D IconA;        // 100개 모두 동일한 텍스처 참조
    // ...
    public float Health;           // 인스턴스마다 다름
}
// ❌ 문제 : 동일한 데이터가 100개의 컴포넌트에 중복 저장됨
```

**Flyweight Pattern :**
```csharp
// 공유 데이터는 ScriptableObject 하나에 보관, Ship은 참조만
public class Ship : MonoBehaviour
{
    [SerializeField] private ShipData m_SharedData; // 참조만! 모두 동일한 에셋
    
    [SerializeField] private float    m_Health;     // 인스턴스마다 고유
}
// ✅ 장점 : ShipData 에셋 1개를 100개 Ship이 공유, 메모리 절약!
```

---

## 🤔 왜 Flyweight Pattern이 필요한가?

### 문제 상황

게임에서 동일한 종류의 유닛을 대량으로 생성할 때, 일반적으로 이렇게 작성합니다 :

```csharp
public class UnrefactoredShipFactory : MonoBehaviour
{
    public void GenerateShips(int rows, int columns)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                UnrefactoredShip newShip = Instantiate(m_ShipPrefab, ...);

                // 모든 Ship에 동일한 데이터를 매번 복사
                newShip.UnitName    = m_UnitName;       // 복사
                newShip.Description = m_Description;    // 복사
                newShip.Speed       = m_Speed;           // 복사
                newShip.AttackPower = m_AttackPower;     // 복사
                newShip.Defense     = m_Defense;         // 복사
                newShip.IconA       = m_IconA;           // 복사
                newShip.IconB       = m_IconB;           // 복사
                newShip.IconC       = m_IconC;           // 복사
                newShip.Health      = m_MaxHealth;       // 복사
            }
        }
    }
}
```

**이 코드의 문제점 :**

❌ **메모리 낭비**
   - 100개 Ship이 모두 동일한 UnitName, Speed, Texture를 각자 보유
   - 인스턴스 수가 늘어날수록 메모리 사용량이 선형 증가

❌ **데이터 불일치 위험**
   - UnitName을 바꾸려면 100개 인스턴스를 모두 수정해야 함
   - 일부만 수정 시 데이터 불일치 발생 가능

❌ **확장성 부족**
   - 함선 종류가 늘어날수록 관리해야 할 데이터 복사 코드 증가
   - 새 속성 추가 시 Factory와 Ship 양쪽 모두 수정 필요

### Flyweight Pattern의 해결책

✅ **공유 데이터 분리**
   - 불변 데이터(UnitName, Speed 등)는 `ShipData` ScriptableObject 하나에 보관
   - 모든 Ship 인스턴스가 동일한 `ShipData` 에셋을 참조

✅ **메모리 절약**
   - ShipData 에셋 1개 + Ship 인스턴스 100개 × (Health만)
   - 대량 생성 시 메모리 효율이 극적으로 향상됨

✅ **중앙 집중 관리**
   - ShipData 하나만 수정하면 모든 Ship에 즉시 반영
   - 데이터 불일치 가능성이 사라짐

---

## 🏗️ 핵심 구성요소

Flyweight Pattern은 다음 3가지 핵심 요소로 구성됩니다 :

### 1️⃣ Flyweight (플라이웨이트 / 공유 데이터)

**📁 파일 :** [Refactored/ShipData.cs](./Scripts/Refactored/ShipData.cs)

```csharp
[CreateAssetMenu(fileName = "ShipData", menuName = "Flyweight/ShipData", order = 1)]
public class ShipData : ScriptableObject
{
    [Header("공유 데이터")]
    [Tooltip("모든 함선 인스턴스에서 공유되는 문자열 데이터")]
    public string UnitName;

    [Tooltip("모든 함선 인스턴스에서 변경되지 않는 내재적(intrinsic) 속도 데이터")]
    public float Speed;

    [Tooltip("모든 함선 인스턴스에서 변경되지 않는 내재적(intrinsic) 공격력 데이터")]
    public int AttackPower;

    [Tooltip("모든 함선 인스턴스에서 변경되지 않는 내재적(intrinsic) 방어력 데이터")]
    public int Defense;

    public Texture2D IconA;
    public Texture2D IconB;
    public Texture2D IconC;
}
```

**역할 :**
- 모든 함선 인스턴스가 공유하는 **내재적(Intrinsic) 데이터** 보관소
- 유니티에서는 `ScriptableObject` 기반으로 에셋 파일로 저장되어 메모리에 단 1개만 존재
- 읽기 전용으로 취급하여 인스턴스 간 데이터 불일치 방지

**ScriptableObject를 사용하는 이유 :**
```
일반 클래스 방식                 ScriptableObject 방식
─────────────────────          ─────────────────────────────
인스턴스마다 데이터 복사  vs     에셋 1개를 모든 인스턴스가 참조
메모리 : N × (데이터 크기)       메모리 : 1 × (데이터 크기) + N × (참조)
```

---

### 2️⃣ Context (컨텍스트 / 공유 데이터 사용 객체)

**📁 파일 :** [Refactored/Ship.cs](./Scripts/Refactored/Ship.cs)

```csharp
public class Ship : MonoBehaviour
{
    [Header("공유 데이터")]
    [Tooltip("공유 ShipData ScriptableObject 참조")]
    [SerializeField] private ShipData m_SharedData;  // ← 참조만 보관 (가볍다!)

    [Header("고유 데이터")]
    [Tooltip("이 외재적(extrinsic) 체력 데이터는 다른 함선 인스턴스와 공유되지 않음")]
    [SerializeField] private float m_Health;         // ← 인스턴스마다 고유

    // 공유 데이터로 함선 초기화 또는 업데이트
    public void Initialize(ShipData data, float health)
    {
        m_SharedData = data;
        m_Health     = health;
    }
}
```

**역할 :**
- 공유 `ShipData`에 대한 **참조**와 고유한 `Health` 값만 보유
- `Initialize()`로 외부에서 공유 데이터를 주입받아 사용
- 실제 데이터 없이 참조만 들고 있으므로 **경량 객체**가 됨

**내재적 vs 외재적 분리 :**
```
Ship 인스턴스가 보유하는 것
┌─────────────────────────────────────┐
│  ShipData 참조 (포인터, ~8 bytes)    │  ← 내재적 데이터 접근용
│  Health       (float, 4 bytes)       │  ← 외재적 데이터
└─────────────────────────────────────┘

ShipData (단 1개, 모든 Ship이 공유)
┌─────────────────────────────────────┐
│  UnitName, Description, Speed ...    │  ← 실제 내재적 데이터
│  IconA, IconB, IconC (텍스처 참조)   │
└─────────────────────────────────────┘
```

---

### 3️⃣ Flyweight Factory (팩토리 / 공유 데이터를 재사용하여 생성)

**📁 파일 :** [Refactored/ShipFactory.cs](./Scripts/Refactored/ShipFactory.cs)

```csharp
public class ShipFactory : MonoBehaviour
{
    [SerializeField] private Ship     m_ShipPrefab;
    [SerializeField] private ShipData m_SharedData;  // ← 하나의 에셋

    public void GenerateShips(int rows, int columns)
    {
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 position = new Vector3(i * m_Spacing, 0, j * m_Spacing);

                Ship newShip = Instantiate(m_ShipPrefab, position, Quaternion.identity, transform);

                // 모든 Ship이 동일한 m_SharedData를 공유!
                newShip.Initialize(m_SharedData, 100);

                newShip.name = $"Ship_{i * columns + j}";
            }
        }
    }
}
```

**역할 :**
- 모든 Ship 인스턴스에 **동일한 `ShipData` 참조**를 주입
- 공유 데이터를 복사하지 않고 **참조만 전달**하는 핵심 역할
- Unrefactored 버전의 "데이터 복사" 방식과 정반대

---

## 📊 코드 구조

### 폴더 구조

```
9_Flyweight/
├── Scripts/
│   ├── Refactored/
│   │   ├── ShipData.cs           ← Flyweight : 공유 데이터 ScriptableObject
│   │   ├── Ship.cs               ← Context   : 참조 + 고유 데이터만 보유
│   │   └── ShipFactory.cs        ← Factory   : 공유 데이터를 재사용하여 생성
│   │
│   ├── Unrefactored/
│   │   ├── UnrefactoredShip.cs         ← 패턴 적용 전 : 모든 데이터 직접 보유
│   │   └── UnrefactoredShipFactory.cs  ← 패턴 적용 전 : 데이터를 매번 복사
│   │
│   ├── SineWaveMover.cs          ← 공통 : 사인파 운동 컴포넌트
│   ├── ToggleFlyweight.cs        ← 공통 : 적용 전/후 토글
│   └── ToggleLabel.cs            ← 공통 : UI 라벨
│
└── README.md                     ← 📍 현재 문서
```

### 클래스 다이어그램

```
┌────────────────────────────────────┐
│           ShipData                  │  ← Flyweight (공유 데이터)
│      (ScriptableObject)             │     단 1개만 존재
├────────────────────────────────────┤
│  + UnitName    : string             │
│  + Description : string             │
│  + Speed       : float              │
│  + AttackPower : int                │
│  + Defense     : int                │
│  + IconA/B/C   : Texture2D          │
└────────────────────────────────────┘
              △
              │ 참조 (Reference)
              │ 복사 아님!
    ┌─────────┼─────────┐
    ▼         ▼         ▼
┌────────┐ ┌────────┐ ┌────────┐
│ Ship_0  │ │ Ship_1  │ │ Ship_N  │  ← Context (경량 인스턴스)
├────────┤ ├────────┤ ├────────┤
│ Health  │ │ Health  │ │ Health  │  ← 고유 데이터만 보유
│ = 100   │ │ = 75    │ │ = 50    │
└────────┘ └────────┘ └────────┘
              △
              │ 생성 및 초기화
              │
┌────────────────────────────────────┐
│           ShipFactory               │  ← Flyweight Factory
├────────────────────────────────────┤
│  - m_ShipPrefab  : Ship             │
│  - m_SharedData  : ShipData         │  ← 하나의 에셋을 주입
├────────────────────────────────────┤
│  + GenerateShips(rows, columns)     │
└────────────────────────────────────┘
```

### 적용 전/후 메모리 비교

```
Unrefactored (패턴 적용 전) - 10×10 = 100개
┌──────────────────────────────────────┐
│ Ship_0  : UnitName + Speed + ... + Health  │ ← 전체 데이터
│ Ship_1  : UnitName + Speed + ... + Health  │ ← 전체 데이터 복사
│ Ship_2  : UnitName + Speed + ... + Health  │ ← 전체 데이터 복사
│   ...                                      │
│ Ship_99 : UnitName + Speed + ... + Health  │ ← 전체 데이터 복사
└──────────────────────────────────────┘
총 메모리 : 100 × (모든 필드 크기)

Refactored (패턴 적용 후) - 10×10 = 100개
┌──────────────────────────────────────┐
│ ShipData (1개) : UnitName + Speed + ...    │ ← 공유 데이터 1회 저장
├──────────────────────────────────────┤
│ Ship_0  : [ShipData 참조] + Health         │ ← 경량!
│ Ship_1  : [ShipData 참조] + Health         │ ← 경량!
│   ...                                      │
│ Ship_99 : [ShipData 참조] + Health         │ ← 경량!
└──────────────────────────────────────┘
총 메모리 : 1 × (공유 데이터) + 100 × (참조 + Health)
```

---

## 🔄 실행 흐름

### 1️⃣ 초기화 흐름 (Refactored)

```
[게임 시작]
    ⬇️
ShipFactory.Start()
    ⬇️
GenerateShips(10, 10) 호출
    ⬇️
┌──────────────────────────────────────┐
│ 100번 반복 (i : 0~9, j : 0~9)        │
│   ① 위치 계산                        │
│   ② Ship 프리팹 인스턴스화            │
│   ③ newShip.Initialize(m_SharedData, │
│      100)                            │
│      → m_SharedData : 동일 참조 전달 │ ← 핵심!
│      → m_Health     : 100 설정       │
│   ④ SineWaveMover 추가               │
└──────────────────────────────────────┘
    ⬇️
✅ 100개 Ship 생성 완료
   공유 데이터는 ShipData 에셋 1개뿐
```

### 2️⃣ 적용 전/후 토글 흐름

```
[Toggle 버튼 클릭]
    ⬇️
ToggleFlyweight.Toggle()
    ⬇️
┌──────────────────────────────────────┐
│ isFlyweightActive = m_Flyweight      │
│                     .activeSelf      │
│                                      │
│ SetActive(!isFlyweightActive)        │
│   → m_NonFlyweight.SetActive(!state) │
│   → m_Flyweight.SetActive(state)     │
└──────────────────────────────────────┘
    ⬇️
ToggleLabel.UpdateLabel()
    ⬇️
✅ Refactored ↔ Unrefactored 전환
   (메모리 사용량 차이를 시각적으로 비교)
```

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : 내재적/외재적 데이터 분리 (Ship)

**위치 :** Ship.cs:10-25

```csharp
public class Ship : MonoBehaviour
{
    [Header("공유 데이터")]
    [SerializeField] private ShipData m_SharedData; // 내재적 데이터 : 참조만

    [Header("고유 데이터")]
    [SerializeField] private float m_Health;        // 외재적 데이터 : 직접 보유

    public void Initialize(ShipData data, float health)
    {
        m_SharedData = data;    // 참조 주입
        m_Health     = health;  // 고유 값 설정
    }
}
```

**이해 포인트 :**
- `m_SharedData`는 실제 데이터가 아닌 **참조(Reference)** 만 저장
- `m_Health`만이 인스턴스마다 다른 값을 가지는 진짜 고유 데이터
- 데이터를 직접 소유하지 않고 참조함으로써 경량 객체가 됨

---

### 📌 핵심 코드 2 : 공유 데이터 주입 (ShipFactory)

**위치 :** ShipFactory.cs:36-38

```csharp
// 모든 Ship에 동일한 m_SharedData 참조를 전달
Ship newShip = Instantiate(m_ShipPrefab, position, Quaternion.identity, transform);
newShip.Initialize(m_SharedData, 100);
```

**이해 포인트 :**
- `m_SharedData`는 단 하나의 ScriptableObject 에셋
- `Instantiate()` 는 Ship 컴포넌트(껍데기)만 100개 생성
- `Initialize()` 는 데이터를 복사하지 않고 같은 에셋의 참조를 전달

---

### 📌 핵심 코드 3 : 데이터 복사 vs 참조 비교

**Unrefactored 방식 (비효율) :**
```csharp
// UnrefactoredShipFactory.cs - 매번 데이터 복사
newShip.UnitName    = m_UnitName;    // ← 문자열 복사
newShip.Description = m_Description; // ← 문자열 복사
newShip.Speed       = m_Speed;        // ← 값 복사
newShip.AttackPower = m_AttackPower;  // ← 값 복사
newShip.Defense     = m_Defense;      // ← 값 복사
newShip.IconA       = m_IconA;        // ← 참조 복사 (Texture)
newShip.IconB       = m_IconB;        // ← 참조 복사 (Texture)
newShip.IconC       = m_IconC;        // ← 참조 복사 (Texture)
newShip.Health      = m_MaxHealth;    // ← 값 복사
```

**Refactored 방식 (효율적) :**
```csharp
// ShipFactory.cs - ShipData 참조 하나만 전달
newShip.Initialize(m_SharedData, 100);
// ✅ 공유 데이터는 ShipData 에셋 1개로 끝!
```

---

### 📌 핵심 코드 4 : ScriptableObject가 Flyweight인 이유

```csharp
// ShipData.cs
[CreateAssetMenu(fileName = "ShipData", menuName = "Flyweight/ShipData")]
public class ShipData : ScriptableObject { ... }
```

**ScriptableObject의 특성 :**
```
ScriptableObject 에셋
  → 프로젝트에 파일로 저장 (.asset)
  → 씬에 독립적으로 존재
  → 여러 컴포넌트가 같은 에셋을 참조 가능
  → 에디터에서 변경 시 모든 참조 객체에 즉시 반영

→ 이 특성이 Flyweight의 "공유 데이터" 개념과 완벽히 일치!
```

---

## ⚖️ 장단점

### ✅ 장점

**1. 메모리 절약**
- 대량의 유사 객체 생성 시 메모리 사용량이 극적으로 감소
- 공유 데이터가 클수록, 인스턴스가 많을수록 효과 증가

**2. 중앙 집중 데이터 관리**
- 공유 데이터 수정 시 모든 인스턴스에 자동 반영
- 데이터 불일치 버그 원천 차단

**3. Unity ScriptableObject와 자연스러운 조합**
- Unity의 ScriptableObject가 Flyweight의 자연스러운 구현체
- Inspector에서 시각적으로 에셋 교체 가능

**4. 성능 향상**
- 메모리 감소 → 가비지 컬렉션 부담 감소
- CPU 캐시 효율 향상

**5. 초기화 단순화**
- `Initialize(sharedData, health)` 한 줄로 초기화 완료
- 개별 필드를 일일이 설정하는 번거로움 제거

### ❌ 단점

**1. 코드 복잡도 증가**
- 내재적/외재적 데이터를 분리하는 설계 작업 필요
- 소수의 객체에서는 오히려 복잡도가 증가

**2. 공유 데이터 불변성 주의**
- 공유 데이터를 런타임에 수정하면 모든 인스턴스에 영향
- 인스턴스별 데이터 변경이 필요한 경우 외재적 데이터로 이동해야 함

**3. 디버깅 어려움**
- 공유 데이터가 어디서 변경되었는지 추적이 어려울 수 있음

**4. 적용 범위 제한**
- 객체 수가 적거나 공유 데이터 비율이 낮으면 효과 미미
- 대량 생성(수십~수천 개)이 아니면 복잡도만 증가

---

## 🎮 실제 사용 사례

### 1️⃣ Unity - 파티클 시스템

```csharp
// Unity의 Particle System 내부 동작 원리
// ParticleSystemRenderer가 여러 파티클에 동일한 Mesh와 Material을 공유

// 개발자 사용 예시
public class BulletPool : MonoBehaviour
{
    // 공유 데이터 (Flyweight)
    [SerializeField] private Mesh     m_BulletMesh;
    [SerializeField] private Material m_BulletMaterial;

    // 각 Bullet은 위치/회전만 고유하게 보유
    // → GPU Instancing으로 동일 Mesh/Material 일괄 렌더링
}
```

---

### 2️⃣ 타일맵 시스템

```csharp
// 공유 데이터 (Flyweight) - 타일 종류별 하나만 존재
[CreateAssetMenu]
public class TileData : ScriptableObject
{
    public Sprite   Sprite;       // 타일 이미지
    public bool     IsWalkable;   // 이동 가능 여부
    public Material Material;     // 렌더 머티리얼
    public int      MoveCost;     // 이동 비용
}

// Context - 각 타일 위치별 인스턴스
public class Tile
{
    public TileData SharedData; // 참조만 (공유)
    public Vector2  Position;   // 고유 위치
    public int      TeamOwner;  // 고유 점령 팀
}

// 1000×1000 타일맵에서 타일 종류가 10가지라면
// TileData 에셋 10개만 존재하고 100만 개 Tile이 참조
```

---

### 3️⃣ 텍스트 렌더링 (글꼴 시스템)

```csharp
// 운영체제/게임엔진 내부의 글꼴 시스템도 Flyweight
// 각 문자(Character)는 하나의 Flyweight 객체

// 공유 데이터 (Flyweight) - 동일 글꼴의 같은 글자는 공유
public class GlyphData
{
    public Texture2D Bitmap;     // 글리프 이미지 (공유)
    public Vector2   Size;       // 글자 크기 (공유)
    public float     Advance;    // 다음 글자까지 거리 (공유)
}

// Context - 각 문자 위치별 인스턴스
public struct CharInstance
{
    public GlyphData SharedGlyph; // 참조 (공유)
    public Vector2   ScreenPos;   // 화면 위치 (고유)
    public Color     Color;       // 글자 색 (고유)
}
```

---

### 4️⃣ RTS 게임 - 대규모 유닛 관리

```csharp
// 공유 데이터 (Flyweight) - 유닛 종류별 하나
[CreateAssetMenu]
public class UnitTemplate : ScriptableObject
{
    public string    UnitName;
    public Mesh      Mesh;
    public Material  Material;
    public float     MaxHealth;
    public float     AttackDamage;
    public float     MoveSpeed;
    public AudioClip AttackSound;
}

// Context - 각 유닛 인스턴스
public class Unit : MonoBehaviour
{
    [SerializeField] private UnitTemplate m_Template;  // 공유
    private float                         m_CurrentHealth; // 고유
    private Vector3                       m_Target;        // 고유
    private int                           m_Team;          // 고유
}

// RTS에서 같은 종류 유닛 500개 → UnitTemplate 에셋 1개만 필요!
```

---

## 🔍 Object Pool과의 비교

Flyweight와 Object Pool은 모두 **최적화 패턴**이지만 목적이 다릅니다 :

| 항목 | Flyweight Pattern | Object Pool Pattern |
|------|-------------------|---------------------|
| **목적** | 메모리 사용량 절감 | 객체 생성/파괴 비용 절감 |
| **방법** | 공유 데이터를 분리하여 공유 | 사용 완료된 객체를 재사용 |
| **문제 해결** | 동일 데이터의 중복 저장 | 잦은 Instantiate/Destroy의 GC 부담 |
| **객체 수** | 많은 객체가 동시에 존재 | 실제 필요한 수만 존재, 나머지는 풀에 대기 |
| **Unity 예시** | ScriptableObject 공유 | Bullet/Effect 오브젝트 재사용 |

**함께 사용하면 더욱 강력 :**

```csharp
// Object Pool + Flyweight 결합 예시
public class OptimizedBulletSystem : MonoBehaviour
{
    // Flyweight : 모든 총알이 공유하는 데이터
    [SerializeField] private BulletData m_SharedBulletData;

    // Object Pool : 총알 인스턴스 재사용
    private Queue<Bullet> m_BulletPool = new Queue<Bullet>();

    public Bullet GetBullet()
    {
        Bullet bullet = m_BulletPool.Count > 0
            ? m_BulletPool.Dequeue()   // 풀에서 재사용
            : Instantiate(m_Prefab);   // 없으면 새로 생성

        // Flyweight : 공유 데이터 참조 주입
        bullet.Initialize(m_SharedBulletData, startPos, direction);
        return bullet;
    }
}
// ✅ GC 부담 없음 (Object Pool) + 메모리 절약 (Flyweight)!
```

---

## 🎓 학습 정리

### 핵심 개념

**플라이웨이트 패턴의 본질 :**
```
객체를 내재적 데이터(공유)와 외재적 데이터(고유)로 분리하여
공유 데이터는 단 하나만 만들고 모든 인스턴스가 참조하게 한다
```

### 주요 구성 요소

```
Flyweight (플라이웨이트)
  ↓
  ShipData - 공유 데이터 보관, ScriptableObject 에셋

Context (컨텍스트)
  ↓
  Ship - Flyweight 참조 + 고유 데이터(Health)만 보유

Flyweight Factory (팩토리)
  ↓
  ShipFactory - 동일한 Flyweight를 모든 Context에 주입
```

### 설계 기준

```
이 데이터가 내재적인가? (모든 인스턴스에서 동일한가?)
  ↓ YES → Flyweight(공유 객체)에 넣는다
  ↓ NO  → Context(인스턴스)가 직접 보유한다
```

### 언제 사용해야 할까?

**✅ Flyweight Pattern을 사용하면 좋은 경우 :**
- **동일한 종류의 객체**가 **수십~수천 개** 이상 필요할 때
- 각 인스턴스가 **공통 데이터를 많이** 보유할 때
- 메모리 사용량이 실제 문제가 될 때 (모바일, 대규모 시뮬레이션)
- **ScriptableObject**로 데이터를 관리하는 Unity 프로젝트

**❌ Flyweight Pattern을 피해야 하는 경우 :**
- 객체 수가 적어 메모리 절약 효과가 미미할 때
- 모든 데이터가 인스턴스마다 달라 공유할 데이터가 없을 때
- 공유 데이터를 런타임에 자주 변경해야 할 때

### 관련 패턴

| 패턴 | 관계 |
|------|------|
| **Object Pool** | 둘 다 최적화 패턴, 함께 사용하면 시너지 효과 |
| **Factory** | Flyweight Factory가 공유 객체를 관리하고 주입 |
| **Singleton** | 공유 데이터가 하나뿐이라는 점에서 유사하지만, Singleton은 전역 접근에 초점 |

### 마무리

**기억할 점 :**
- ✅ 공유할 수 있는 데이터(Intrinsic)와 고유한 데이터(Extrinsic)를 명확히 구분
- ✅ Unity에서 `ScriptableObject`는 Flyweight의 자연스러운 구현 수단
- ✅ 대량 객체 생성 시 Object Pool과 조합하면 최적의 성능
- ⚠️ 소규모 프로젝트에서는 설계 복잡도만 증가할 수 있음
- 🎯 타일맵, 파티클, 대규모 유닛 시스템에서 특히 유용

---

**작성일 :** 2026.03.01
**참고 자료 :** Unity Korea - Level Up Your Code with Design Patterns and SOLID
