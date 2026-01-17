# 🏭 Factory Pattern (팩토리 패턴)

## 📋 목차
- [패턴 개요](#-패턴-개요)
- [왜 Factory Pattern이 필요한가?](#-왜-factory-pattern이-필요한가)
- [핵심 구성요소](#-핵심-구성요소)
- [코드 구조](#-코드-구조)
- [실행 흐름](#-실행-흐름)
- [주요 코드 분석](#-주요-코드-분석)
- [장단점](#-장단점)
- [단점 극복 - 실무에서의 활용](#-단점-극복---실무에서의-활용)
- [실제 사용 사례](#-실제-사용-사례)
- [학습 정리](#-학습-정리)

---

## 🎯 패턴 개요

**Factory Pattern**은 **생성 패턴(Creational Pattern)** 중 하나로, 

객체 생성 로직을 캡슐화하여 클라이언트 코드가 구체적인 클래스에 의존하지 않도록 하는 패턴입니다.

### 📌 핵심 개념

```
객체를 직접 new로 만들지 말고,
공장(Factory)에게 만들어달라고 요청하라!
```

**일반적인 방법 :**
```csharp
// 직접 객체 생성
public class Game : MonoBehaviour
{
    void SpawnEnemy()
    {
        // 구체적인 클래스에 직접 의존
        GameObject enemy = Instantiate(zombiePrefab);
        Zombie zombie = enemy.GetComponent<Zombie>();
        zombie.Initialize();
    }
}
// ❌ 문제 : 강한 결합, 확장 어려움, 새 적 추가 시 코드 수정 필요
```

**Factory Pattern :**
```csharp
// 팩토리에게 생성 요청
public class Game : MonoBehaviour
{
    [SerializeField] Factory[] enemyFactories;

    void SpawnEnemy()
    {
        // 인터페이스에만 의존
        IProduct enemy = enemyFactories[0].GetProduct(position);
    }
}
// ✅ 장점 : 느슨한 결합, 쉬운 확장, 새 적 추가 시 코드 수정 불필요!
```

---

## 🤔 왜 Factory Pattern이 필요한가?

### 문제 상황

게임에서 다양한 적을 생성할 때, 일반적으로 이렇게 작성합니다 :

```csharp
public class EnemySpawner : MonoBehaviour
{
    public GameObject zombiePrefab;
    public GameObject skeletonPrefab;
    public GameObject ghostPrefab;

    public void SpawnEnemy(string type, Vector3 position)
    {
        if (type == "zombie")
        {
            GameObject obj = Instantiate(zombiePrefab, position, Quaternion.identity);
            obj.GetComponent<Zombie>().Initialize();
        }
        else if (type == "skeleton")
        {
            GameObject obj = Instantiate(skeletonPrefab, position, Quaternion.identity);
            obj.GetComponent<Skeleton>().Initialize();
        }
        else if (type == "ghost")
        {
            GameObject obj = Instantiate(ghostPrefab, position, Quaternion.identity);
            obj.GetComponent<Ghost>().Initialize();
        }
        // 새로운 적 추가 시 여기에 계속 추가해야 함... 😱
    }
}
```

**이 코드의 문제점 :**

❌ **강한 결합 (Tight Coupling)**
   - EnemySpawner가 모든 적 타입(Zombie, Skeleton, Ghost)을 알아야 함
   - 한 컴포넌트가 모든 구체적인 클래스에 의존

❌ **개방-폐쇄 원칙 위반 (OCP Violation)**
   - 새로운 적 타입 추가 시 EnemySpawner 코드 수정 필요
   - if-else 체인이 계속 길어짐

❌ **단일 책임 원칙 위반 (SRP Violation)**
   - EnemySpawner가 모든 적의 생성 로직을 담당
   - 각 적의 고유한 초기화 로직도 알아야 함

❌ **재사용 불가능**
   - 특정 적 타입에 강하게 결합됨
   - 다른 프로젝트에서 재사용 어려움

### Factory Pattern의 해결책

✅ **느슨한 결합 (Loose Coupling)**
   - 클라이언트는 Factory와 IProduct 인터페이스만 알면 됨
   - 구체적인 제품 클래스를 몰라도 됨

✅ **개방-폐쇄 원칙 준수 (OCP)**
   - 새로운 제품 추가 시 기존 코드 수정 불필요
   - 새 Factory와 Product 클래스만 추가하면 됨

✅ **단일 책임 원칙 준수 (SRP)**
   - 각 Factory는 자신의 제품만 생성
   - 생성 로직이 분리되어 관리 용이

✅ **높은 재사용성**
   - Factory와 Product가 독립적
   - 다른 프로젝트에 쉽게 적용

---

## 🏗️ 핵심 구성요소

Factory Pattern은 다음 핵심 요소들로 구성됩니다 :

### 1️⃣ IProduct (제품 인터페이스)

**📁 파일 :** [IProduct.cs](./Scripts/Pattern/IProduct.cs)

```csharp
public interface IProduct
{
    // 공통 속성 및 메서드를 여기에 추가
    public string ProductName { get; set; }

    // 각 구체적인 제품(Concrete Product)에서 커스터마이징
    public void Initialize();
}
```

**역할 :**
- 모든 제품이 구현해야 하는 공통 인터페이스
- 클라이언트가 구체적인 제품 타입을 몰라도 사용 가능하게 함
- 느슨한 결합의 핵심

**특징 :**
- 공통 속성 : `ProductName`
- 공통 메서드 : `Initialize()`
- 구체적인 제품들이 이를 구현

---

### 2️⃣ Factory (추상 팩토리)

**📁 파일 :** [Factory.cs](./Scripts/Pattern/Factory.cs)

```csharp
public abstract class Factory : MonoBehaviour
{
    // 제품 인스턴스를 가져오는 추상 메서드
    public abstract IProduct GetProduct(Vector3 position);

    // 모든 팩토리에서 공유하는 메서드
    public string GetLog(IProduct product)
    {
        string logMessage = "Factory : created product " + product.ProductName;
        return logMessage;
    }
}
```

**역할 :**
- 모든 구체적인 팩토리의 기본 클래스
- 제품 생성 메서드의 시그니처 정의
- 공통 기능 제공 (로깅 등)

**특징 :**
- `abstract` 클래스 : 직접 인스턴스화 불가
- `GetProduct()` : 각 팩토리가 구현해야 하는 추상 메서드
- `GetLog()` : 모든 팩토리에서 공유하는 유틸리티 메서드

---

### 3️⃣ Concrete Product (구체적인 제품)

**📁 파일 :** [ProductA.cs](./Scripts/ExampleUsage/ProductA.cs), [ProductB.cs](./Scripts/ExampleUsage/ProductB.cs)

```csharp
public class ProductA : MonoBehaviour, IProduct
{
    [SerializeField] private string productName = "ProductA";

    public string ProductName { get => productName; set => productName = value; }

    private ParticleSystem particleSystem;

    public void Initialize()
    {
        // 고유한 초기화 로직을 여기에 추가
        gameObject.name  = productName;
        particleSystem   = GetComponentInChildren<ParticleSystem>();

        if (particleSystem == null)
            return;

        particleSystem.Stop();
        particleSystem.Play();
    }
}
```

**역할 :**
- IProduct 인터페이스를 구현하는 실제 제품
- 각 제품만의 고유한 초기화 로직 포함
- 팩토리에 의해 생성됨

**특징 :**
- ProductA : 파티클 시스템 재생
- ProductB : 오디오 소스 재생
- 각 제품은 고유한 동작을 가짐

---

### 4️⃣ Concrete Factory (구체적인 팩토리)

**📁 파일 :** [ConcreteFactoryA.cs](./Scripts/ExampleUsage/ConcreteFactoryA.cs), [ConcreteFactoryB.cs](./Scripts/ExampleUsage/ConcreteFactoryB.cs)

```csharp
public class ConcreteFactoryA : Factory
{
    // 프리팹 생성에 사용
    [SerializeField] private ProductA productPrefab;

    public override IProduct GetProduct(Vector3 position)
    {
        // 프리팹 인스턴스를 생성하고 제품 컴포넌트를 가져옴
        GameObject instance   = Instantiate(productPrefab.gameObject, position, Quaternion.identity);
        ProductA   newProduct = instance.GetComponent<ProductA>();

        // 각 제품은 고유한 로직을 포함
        newProduct.Initialize();

        return newProduct;
    }
}
```

**역할 :**
- Factory를 상속받아 구체적인 제품 생성 로직 구현
- 특정 제품 타입만 생성
- 프리팹 인스턴스화 및 초기화 담당

**특징 :**
- ConcreteFactoryA : ProductA만 생성
- ConcreteFactoryB : ProductB만 생성
- 각 팩토리는 자신의 제품에 대한 생성 로직만 알면 됨

---

## 📊 코드 구조

### 폴더 구조

```
1_Factory/
├── Scripts/
│   ├── Pattern/                        (핵심 패턴 구현)
│   │   ├── IProduct.cs                ← 제품 인터페이스
│   │   └── Factory.cs                 ← 추상 팩토리
│   │
│   └── ExampleUsage/                  (사용 예시)
│       ├── ProductA.cs                ← 구체적인 제품 A (파티클)
│       ├── ProductB.cs                ← 구체적인 제품 B (사운드)
│       ├── ConcreteFactoryA.cs        ← ProductA 전용 팩토리
│       ├── ConcreteFactoryB.cs        ← ProductB 전용 팩토리
│       └── ClickToCreate.cs           ← 클라이언트 (사용 예제)
│
└── README.md                           ← 📍 현재 문서
```

### 클래스 다이어그램

```
                    ┌─────────────────────┐
                    │    <<interface>>    │
                    │      IProduct       │
                    ├─────────────────────┤
                    │ + ProductName       │
                    │ + Initialize()      │
                    └──────────┬──────────┘
                               │ implements
              ┌────────────────┴────────────────┐
              │                                 │
    ┌─────────▼─────────┐           ┌──────────▼────────┐
    │     ProductA      │           │     ProductB      │
    ├───────────────────┤           ├───────────────────┤
    │ - particleSystem  │           │ - audioSource     │
    │ + Initialize()    │           │ + Initialize()    │
    └───────────────────┘           └───────────────────┘
              ▲                               ▲
              │ creates                       │ creates
              │                               │
    ┌─────────┴─────────┐           ┌─────────┴─────────┐
    │ ConcreteFactoryA  │           │ ConcreteFactoryB  │
    ├───────────────────┤           ├───────────────────┤
    │ + GetProduct()    │           │ + GetProduct()    │
    └─────────┬─────────┘           └─────────┬─────────┘
              │ extends                       │ extends
              └────────────┬──────────────────┘
                           │
                ┌──────────▼──────────┐
                │  <<abstract>>       │
                │     Factory         │
                ├─────────────────────┤
                │ + GetProduct()      │
                │ + GetLog()          │
                └──────────┬──────────┘
                           │ uses
                ┌──────────▼──────────┐
                │   ClickToCreate     │
                │     (Client)        │
                ├─────────────────────┤
                │ - factories[]       │
                │ + GetProductAtClick │
                └─────────────────────┘
```

---

## 🔄 실행 흐름

### 전체 흐름

```
[사용자 입력]
    ⬇️
마우스 클릭
    ⬇️
┌─────────────────────────────────────┐
│ ClickToCreate.GetProductAtClick()   │
│   - 랜덤 Factory 선택               │
│   - Raycast로 클릭 위치 감지        │
└─────────────────────────────────────┘
    ⬇️
┌─────────────────────────────────────┐
│ Factory.GetProduct(position)        │
│   - Instantiate()로 프리팹 생성     │
│   - GetComponent<Product>()         │
│   - product.Initialize() 호출       │
└─────────────────────────────────────┘
    ⬇️
┌─────────────────────────────────────┐
│ IProduct 반환                       │
│   - 구체적인 타입을 몰라도 사용!    │
└─────────────────────────────────────┘
    ⬇️
✅ 생성된 제품 리스트에 추가
```

### 상세 흐름

```
1️⃣ 사용자가 마우스 클릭
         │
         ▼
2️⃣ ClickToCreate.Update()
         │
         ├── Input.GetMouseButtonDown(0) 확인
         │
         ▼
3️⃣ ClickToCreate.GetProductAtClick()
         │
         ├── 랜덤 Factory 선택 : factories[Random.Range(0, length)]
         ├── Ray 생성 : Camera.main.ScreenPointToRay()
         ├── Raycast 실행 : Physics.Raycast()
         │
         ▼
4️⃣ selectedFactory.GetProduct(hitInfo.point + offset)
         │
         ├── Instantiate(productPrefab, position, rotation)
         ├── instance.GetComponent<ProductA or B>()
         ├── newProduct.Initialize()
         │
         ▼
5️⃣ IProduct 반환
         │
         ├── createdProducts.Add(component.gameObject)
         │
         ▼
6️⃣ 완료! 제품이 씬에 생성됨
```

---

## 💻 주요 코드 분석

### 📌 핵심 코드 1 : 제품 인터페이스 정의

**위치 :** IProduct.cs:11-17

```csharp
public interface IProduct
{
    // ✅ 핵심 1 : 공통 속성
    public string ProductName { get; set; }

    // ✅ 핵심 2 : 공통 메서드
    public void Initialize();
}
```

**이해 포인트 :**
- 모든 제품의 **공통 계약** 정의
- 클라이언트는 IProduct만 알면 됨
- 구체적인 ProductA, ProductB를 몰라도 사용 가능
- **느슨한 결합**의 핵심

---

### 📌 핵심 코드 2 : 추상 팩토리 정의

**위치 :** Factory.cs:10-21

```csharp
public abstract class Factory : MonoBehaviour
{
    // ✅ 핵심 1 : 추상 메서드 - 하위 클래스에서 반드시 구현
    public abstract IProduct GetProduct(Vector3 position);

    // ✅ 핵심 2 : 공통 기능 - 모든 팩토리에서 공유
    public string GetLog(IProduct product)
    {
        string logMessage = "Factory : created product " + product.ProductName;
        return logMessage;
    }
}
```

**이해 포인트 :**
- `abstract` : 직접 인스턴스화 불가, 상속용
- `GetProduct()` : **템플릿 메서드** - 각 팩토리가 구현
- `GetLog()` : 공통 유틸리티 - 코드 중복 방지
- **반환 타입이 IProduct** : 느슨한 결합 유지

---

### 📌 핵심 코드 3 : 구체적인 팩토리 구현

**위치 :** ConcreteFactoryA.cs:11-27

```csharp
public class ConcreteFactoryA : Factory
{
    // ✅ 핵심 1 : 생성할 프리팹 참조
    [SerializeField] private ProductA productPrefab;

    public override IProduct GetProduct(Vector3 position)
    {
        // ✅ 핵심 2 : 프리팹 인스턴스화
        GameObject instance   = Instantiate(productPrefab.gameObject, position, Quaternion.identity);
        ProductA   newProduct = instance.GetComponent<ProductA>();

        // ✅ 핵심 3 : 제품 초기화
        newProduct.Initialize();

        // ✅ 핵심 4 : IProduct로 반환 (업캐스팅)
        return newProduct;
    }
}
```

**이해 포인트 :**
- 팩토리는 **자신의 제품만** 알면 됨
- `Instantiate()` : Unity의 프리팹 생성
- `Initialize()` : 제품별 고유 초기화
- **IProduct로 반환** : 클라이언트에게 구체적인 타입 숨김

---

### 📌 핵심 코드 4 : 클라이언트 코드

**위치 :** ClickToCreate.cs:14-52

```csharp
public class ClickToCreate : MonoBehaviour
{
    // ✅ 핵심 1 : Factory 타입으로 참조 (구체적인 팩토리 타입 아님!)
    [SerializeField] private Factory[] factories;

    private List<GameObject> createdProducts = new List<GameObject>();

    private void GetProductAtClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // ✅ 핵심 2 : 랜덤 팩토리 선택
            Factory    selectedFactory = factories[Random.Range(0, factories.Length)];
            Ray        ray             = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerToClick) && selectedFactory != null)
            {
                // ✅ 핵심 3 : 팩토리에게 생성 요청 (구체적인 타입 모름!)
                IProduct product = selectedFactory.GetProduct(hitInfo.point + offset);

                // ✅ 핵심 4 : IProduct로 사용
                if (product is Component component)
                {
                    createdProducts.Add(component.gameObject);
                }
            }
        }
    }
}
```

**이해 포인트 :**
- `Factory[]` : **추상 타입**으로 참조 (ConcreteFactoryA가 아님!)
- 클라이언트는 **어떤 팩토리인지 몰라도** 됨
- `IProduct product` : **인터페이스**로 받음
- 클라이언트는 **ProductA인지 ProductB인지 몰라도** 됨
- 이것이 **느슨한 결합**!

---

## ⚖️ 장단점

### ✅ 장점

**1. 느슨한 결합 (Loose Coupling)**
- 클라이언트가 구체적인 제품 클래스를 몰라도 됨
- 인터페이스를 통한 통신
- 변경에 유연하게 대응

**2. 개방-폐쇄 원칙 (OCP) 준수**
- 새로운 제품/팩토리 추가 시 기존 코드 수정 불필요
- 확장에는 열려있고, 수정에는 닫혀있음

**3. 단일 책임 원칙 (SRP) 준수**
- 각 팩토리는 자신의 제품 생성만 담당
- 생성 로직이 분리되어 관리 용이

**4. 캡슐화**
- 복잡한 생성 로직을 팩토리 내부에 숨김
- 클라이언트 코드 간결화

**5. 유연한 제품 교체**
- Inspector에서 팩토리 교체만으로 다른 제품 생성
- 런타임에 동적으로 팩토리 변경 가능

### ❌ 단점

**1. 클래스 증가**
- 새 제품마다 Factory + Product 클래스 추가 필요
- 간단한 경우 오히려 복잡해질 수 있음

**2. 추상화 오버헤드**
- 인터페이스를 통한 간접 호출
- 매우 성능이 중요한 경우 고려 필요

**3. 병렬 클래스 계층**
- Product 계층과 Factory 계층이 병렬로 증가
- ProductA ↔ ConcreteFactoryA
- ProductB ↔ ConcreteFactoryB

**4. 과도한 사용 시 복잡도 증가**
- 모든 객체 생성에 팩토리를 쓰면 과도함
- 적절한 상황에서만 사용 필요

---

## 🔧 단점 극복 - 실무에서의 활용

### 📊 클래스 증가 문제

원본 Factory Pattern의 가장 큰 단점은 **클래스 수가 급격히 증가**한다는 점입니다 :

```
📊 클래스 수 계산 :

기본 뼈대 : IProduct + Factory              = 2개
제품 N개  : (Product + ConcreteFactory) × N = 2N개
─────────────────────────────────────────────────
총합 : 2 + 2N 개

예시 :
• 제품 3개  → 2 + 6  = 8개 클래스
• 제품 10개 → 2 + 20 = 22개 클래스 😱
```

실무에서는 이 문제를 해결하기 위해 **간소화된 버전**을 사용합니다.

---

### 💡 해결 방법 1 : Simple Factory (단순 팩토리)

**팩토리 하나**가 모든 제품을 생성하는 방식 :

```csharp
public enum ProductType { A, B, C }

// 팩토리 1개로 모든 제품 생성!
public class ProductFactory : MonoBehaviour
{
    [SerializeField] private ProductA productAPrefab;
    [SerializeField] private ProductB productBPrefab;
    [SerializeField] private ProductC productCPrefab;

    public IProduct CreateProduct(ProductType type, Vector3 position)
    {
        return type switch
        {
            ProductType.A => CreateAndInitialize(productAPrefab, position),
            ProductType.B => CreateAndInitialize(productBPrefab, position),
            ProductType.C => CreateAndInitialize(productCPrefab, position),
            _ => null
        };
    }

    private T CreateAndInitialize<T>(T prefab, Vector3 position) where T : Component, IProduct
    {
        T instance = Instantiate(prefab, position, Quaternion.identity);
        instance.Initialize();
        return instance;
    }
}
```

**클래스 수 :** `1 + N` (팩토리 1개 + 제품 N개)
- 제품 10개 → 1 + 10 = **11개** (vs 원본 22개)

| 장점 | 단점 |
|------|------|
| ✅ 클래스 수 대폭 감소 | ❌ OCP 위반 - 새 제품 추가 시 팩토리 수정 필요 |
| ✅ 구조가 단순함 | ❌ switch/if-else 체인 증가 |
| ✅ 이해하기 쉬움 | ❌ 팩토리가 모든 제품을 알아야 함 |

**추천 상황 :** 제품 종류가 적고, 자주 변경되지 않는 경우

---

### 💡 해결 방법 2 : 제네릭 팩토리

**제네릭을 활용**하여 팩토리 클래스를 하나로 통합 :

```csharp
// 제네릭으로 팩토리 1개만!
public class GenericFactory<T> : MonoBehaviour where T : Component, IProduct
{
    [SerializeField] private T prefab;

    public IProduct CreateProduct(Vector3 position)
    {
        T instance = Instantiate(prefab, position, Quaternion.identity);
        instance.Initialize();
        return instance;
    }
}

// 사용 예시 - Inspector에서 프리팹만 할당
public class ProductAFactory : GenericFactory<ProductA> { }
public class ProductBFactory : GenericFactory<ProductB> { }
```

**클래스 수 :** `1 + N + N` (제네릭 팩토리 1개 + 래퍼 N개 + 제품 N개)
- 하지만 래퍼 클래스는 한 줄이므로 실질적 복잡도 감소

| 장점 | 단점 |
|------|------|
| ✅ 코드 중복 제거 | ❌ Unity Inspector 제약으로 래퍼 필요 |
| ✅ OCP 준수 | ❌ 제네릭 개념 이해 필요 |
| ✅ 타입 안정성 보장 | |

---

### 💡 해결 방법 3 : ScriptableObject 기반 팩토리 (Unity 추천)

**ScriptableObject**를 활용한 데이터 주도 방식 :

```csharp
// 팩토리를 ScriptableObject로!
[CreateAssetMenu(fileName = "ProductFactory", menuName = "Factory/Product")]
public class ProductFactorySO : ScriptableObject
{
    [SerializeField] private GameObject prefab;

    public IProduct CreateProduct(Vector3 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        IProduct product    = instance.GetComponent<IProduct>();
        product.Initialize();
        return product;
    }
}

// 사용하는 클라이언트
public class ProductSpawner : MonoBehaviour
{
    [SerializeField] private ProductFactorySO[] factories;

    public void SpawnRandom(Vector3 position)
    {
        ProductFactorySO factory = factories[Random.Range(0, factories.Length)];
        IProduct product = factory.CreateProduct(position);
    }
}
```

**클래스 수 :** `1 + N` (팩토리 SO 1개 + 제품 N개)
- **코드 변경 없이** Inspector에서 프리팹만 교체하면 새 제품 추가 가능!

| 장점 | 단점 |
|------|------|
| ✅ 클래스 수 최소화 | ❌ ScriptableObject 이해 필요 |
| ✅ OCP 준수 | ❌ 복잡한 생성 로직에는 부적합 |
| ✅ 디자이너 친화적 (Inspector 설정) | |
| ✅ 에셋으로 관리 가능 | |

**추천 상황 :** Unity 프로젝트에서 가장 많이 사용되는 방식

---

### 💡 해결 방법 4 : 딕셔너리 기반 팩토리

**등록 방식**으로 동적 확장 지원 :

```csharp
public class RegistrableFactory : MonoBehaviour
{
    private Dictionary<string, GameObject> prefabRegistry = new Dictionary<string, GameObject>();

    // 런타임에 제품 등록
    public void RegisterProduct(string key, GameObject prefab)
    {
        prefabRegistry[key] = prefab;
    }

    public IProduct CreateProduct(string key, Vector3 position)
    {
        if (prefabRegistry.TryGetValue(key, out GameObject prefab))
        {
            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            IProduct product    = instance.GetComponent<IProduct>();
            product.Initialize();
            return product;
        }
        return null;
    }
}
```

| 장점 | 단점 |
|------|------|
| ✅ 런타임 동적 등록 가능 | ❌ 타입 안정성 낮음 (문자열 키) |
| ✅ 매우 유연함 | ❌ 오타 시 런타임 에러 |
| ✅ 모드/DLC 지원에 적합 | |

---

### 📊 방식별 비교 정리

| 방식 | 클래스 수 | OCP | 복잡도 | Unity 친화성 | 추천 상황 |
|------|----------|-----|--------|-------------|-----------|
| **원본 Factory Pattern** | 2 + 2N | ✅ | 높음 | 보통 | 학습, 대규모 프로젝트 |
| **Simple Factory** | 1 + N | ❌ | 낮음 | 보통 | 소규모, 고정된 제품 |
| **제네릭 팩토리** | 1 + 2N | ✅ | 중간 | 보통 | 코드 중복 제거 |
| **ScriptableObject** | 1 + N | ✅ | 낮음 | **매우 높음** | **Unity 프로젝트 (추천)** |
| **딕셔너리 기반** | 1 + N | ✅ | 중간 | 보통 | 동적 확장, 모드 지원 |

---

### 🎯 결론

```
╔═════════════════════════════════════════════════════════════╗
║                                                             ║
║  📚 원본 Factory Pattern                                    ║
║     → "교과서적인 패턴" - 구조를 이해하기 위한 학습용       ║
║                                                             ║
║  🏭 실무에서는                                              ║
║     → Simple Factory 또는 ScriptableObject 방식을           ║
║       더 많이 사용! (클래스 수를 줄이기 위해)               ║
║                                                             ║
╚═════════════════════════════════════════════════════════════╝
```

**핵심은 동일합니다 :**
- 객체 생성을 **캡슐화**
- 클라이언트와 구체 클래스의 **결합도 감소**
- 상황에 맞게 **적절한 방식 선택**

---

## 🎮 실제 사용 사례

### 1️⃣ 적(Enemy) 생성 시스템

```csharp
// 적 인터페이스
public interface IEnemy
{
    string EnemyName { get; }
    int    Health    { get; }
    void   Attack();
    void   TakeDamage(int damage);
}

// 적 팩토리 (추상)
public abstract class EnemyFactory : MonoBehaviour
{
    public abstract IEnemy SpawnEnemy(Vector3 position);
}

// 좀비 팩토리
public class ZombieFactory : EnemyFactory
{
    [SerializeField] private Zombie zombiePrefab;

    public override IEnemy SpawnEnemy(Vector3 position)
    {
        GameObject obj  = Instantiate(zombiePrefab.gameObject, position, Quaternion.identity);
        Zombie     zombie = obj.GetComponent<Zombie>();
        zombie.Initialize();
        return zombie;
    }
}

// 스켈레톤 팩토리
public class SkeletonFactory : EnemyFactory
{
    [SerializeField] private Skeleton skeletonPrefab;

    public override IEnemy SpawnEnemy(Vector3 position)
    {
        GameObject obj      = Instantiate(skeletonPrefab.gameObject, position, Quaternion.identity);
        Skeleton   skeleton = obj.GetComponent<Skeleton>();
        skeleton.Initialize();
        return skeleton;
    }
}

// 클라이언트 : 스포너
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private EnemyFactory[] enemyFactories;

    public void SpawnRandomEnemy(Vector3 position)
    {
        // 어떤 적인지 몰라도 됨!
        EnemyFactory factory = enemyFactories[Random.Range(0, enemyFactories.Length)];
        IEnemy enemy = factory.SpawnEnemy(position);
        Debug.Log($"Spawned : {enemy.EnemyName}");
    }
}
```

---

### 2️⃣ 아이템 생성 시스템

```csharp
public interface IItem
{
    string ItemName { get; }
    void   Use();
}

public abstract class ItemFactory : MonoBehaviour
{
    public abstract IItem CreateItem(Vector3 position);
}

// 무기 팩토리
public class WeaponFactory : ItemFactory
{
    [SerializeField] private Sword swordPrefab;

    public override IItem CreateItem(Vector3 position)
    {
        // 무기 생성 로직
        return Instantiate(swordPrefab, position, Quaternion.identity);
    }
}

// 포션 팩토리
public class PotionFactory : ItemFactory
{
    [SerializeField] private HealthPotion potionPrefab;

    public override IItem CreateItem(Vector3 position)
    {
        // 포션 생성 로직
        return Instantiate(potionPrefab, position, Quaternion.identity);
    }
}
```

---

### 3️⃣ UI 팝업 시스템

```csharp
public interface IPopup
{
    void Show();
    void Hide();
}

public abstract class PopupFactory : MonoBehaviour
{
    public abstract IPopup CreatePopup(Transform parent);
}

// 확인 팝업 팩토리
public class ConfirmPopupFactory : PopupFactory
{
    [SerializeField] private ConfirmPopup prefab;

    public override IPopup CreatePopup(Transform parent)
    {
        return Instantiate(prefab, parent);
    }
}

// 알림 팝업 팩토리
public class AlertPopupFactory : PopupFactory
{
    [SerializeField] private AlertPopup prefab;

    public override IPopup CreatePopup(Transform parent)
    {
        return Instantiate(prefab, parent);
    }
}
```

---

### 4️⃣ 파티클 이펙트 시스템

```csharp
public interface IEffect
{
    void Play();
    void Stop();
}

public abstract class EffectFactory : MonoBehaviour
{
    public abstract IEffect CreateEffect(Vector3 position);
}

// 폭발 이펙트 팩토리
public class ExplosionEffectFactory : EffectFactory
{
    [SerializeField] private ExplosionEffect prefab;

    public override IEffect CreateEffect(Vector3 position)
    {
        ExplosionEffect effect = Instantiate(prefab, position, Quaternion.identity);
        effect.Initialize();
        return effect;
    }
}

// 힐 이펙트 팩토리
public class HealEffectFactory : EffectFactory
{
    [SerializeField] private HealEffect prefab;

    public override IEffect CreateEffect(Vector3 position)
    {
        HealEffect effect = Instantiate(prefab, position, Quaternion.identity);
        effect.Initialize();
        return effect;
    }
}
```

---

## 🎓 학습 정리

### 핵심 개념

**팩토리 패턴의 본질 :**
```
객체 생성을 캡슐화하여
클라이언트와 구체적인 클래스 사이의 결합도를 낮춘다
```

### 핵심 구조

```
클라이언트 (ClickToCreate)
     │
     │ uses (추상 타입으로!)
     ▼
Factory (추상) ◄────── ConcreteFactoryA, B (구현)
     │                          │
     │ returns                  │ creates
     ▼                          ▼
IProduct (인터페이스) ◄────── ProductA, B (구현)
```

### 확장 시나리오

**새로운 제품(ProductC) 추가 시 :**
```
✅ 추가할 파일 :
   • ProductC.cs         (IProduct 구현)
   • ConcreteFactoryC.cs (Factory 상속)

✅ 수정할 파일 : 없음!
   → OCP(개방-폐쇄 원칙) 준수!
```

**새로운 제품군(무기) 추가 시 :**
```
✅ 추가할 파일 :
   • IWeapon.cs          (새 인터페이스)
   • WeaponFactory.cs    (새 추상 팩토리)
   • Sword.cs, Gun.cs    (IWeapon 구현)
   • SwordFactory.cs, GunFactory.cs (WeaponFactory 상속)

✅ 수정할 파일 : 없음!
   → 기존 시스템과 독립적!
```

### 언제 사용해야 할까?

**✅ 팩토리 패턴을 사용하면 좋은 경우 :**
- 객체 생성 로직이 **복잡**한 경우
- **다양한 타입**의 객체를 생성해야 하는 경우
- 클라이언트가 **구체적인 클래스**를 몰라야 하는 경우
- **새로운 타입**이 자주 추가될 것으로 예상되는 경우
- **프리팹 기반** 시스템 (Unity)

**❌ 팩토리 패턴을 피해야 하는 경우 :**
- 객체 생성이 **단순**한 경우
- 생성할 객체 타입이 **하나**뿐인 경우
- 확장 가능성이 **낮은** 경우
- 성능이 **매우 중요**한 경우

### 관련 패턴

**Abstract Factory :**
- 팩토리의 팩토리
- 관련된 객체들의 **군(family)**을 생성

**Builder :**
- 복잡한 객체를 **단계적으로** 생성
- 같은 생성 과정으로 다른 표현 가능

**Prototype :**
- 기존 객체를 **복제**하여 새 객체 생성
- Clone() 메서드 활용

### 마무리

팩토리 패턴은 **Unity에서 매우 유용한 패턴**입니다.

**기억할 점 :**
- ✅ 객체 생성 로직을 **캡슐화**
- ✅ **느슨한 결합**으로 유연한 시스템
- ✅ **OCP 원칙** 준수 - 확장에 열려있음
- ⚠️ 클래스 수 증가에 주의
- 🎯 적절한 상황에서 사용

---

**작성일 :** 2026.01.17
**참고 자료 :** Unity Korea - Level Up Your Code with Design Patterns and SOLID
