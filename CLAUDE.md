# Claude Code 코딩 스타일 가이드

## 변수 정렬 규칙

### 기본 원칙
- **모든 변수 선언 및 할당**에서 `=` 기호를 기준으로 수직 정렬한다.
- 주석이 있는 경우 `//` 기호도 수직으로 정렬한다.
- SerializeField, private 변수, 메서드 내 지역 변수 모두 동일한 규칙 적용

### 예시

#### 1. SerializeField 변수

```csharp
[Header("Movement Settings - RocketController 3D 버전")]
[SerializeField] private float thrustPower     = 5f;    // 추진력 (RocketController와 동일)
[SerializeField] private float maxSpeed        = 10f;   // 최대 속도 (RocketController와 동일)
[SerializeField] private float drag            = 0.95f; // 우주에서의 감속 (RocketController와 동일)
[SerializeField] private float arrivalDistance = 1f;    // 도착 판정 거리

[Header("Rotation Settings - RocketController 3D 버전")]
[SerializeField] private bool    smoothRotation   = true;                  // 부드러운 회전 (RocketController와 동일)
[SerializeField] private float   rotationDamping  = 5f;                    // 회전 감쇠 (RocketController와 동일)
[SerializeField] private float   inputThreshold   = 0.1f;                  // 입력 임계값 (RocketController와 동일)
[SerializeField] private Vector3 rotationOffset   = new Vector3(0, 90, 0); // 회전 오프셋 (RocketController의 -90f와 동일 개념)
```

#### 2. Private 변수

```csharp
// Private variables
private Rigidbody rb;
private Vector3   movementInput;         // 이동 입력 (경로 방향으로 자동 계산)
private Vector3   velocity;              // 현재 속도
private float     currentThrust;         // 현재 추진력
private int       currentWaypointIndex = 0;    // 현재 목표 포인트 인덱스
private bool      isMoving             = false;
```

#### 3. 메서드 내 지역 변수 및 할당문

```csharp
void Start()
{
    rb.useGravity     = false; // 중력 없음 (2D의 gravityScale = 0f와 동일)
    rb.linearDamping  = 0f;    // Rigidbody의 드래그 사용 안함 (직접 제어)
    rb.angularDamping = 0f;    // 각속도 드래그 사용 안함
}
```

### 정렬 규칙 상세

1. **타입과 변수명 사이 공백 조정**
   - 변수명들이 수직으로 정렬되도록 타입 뒤에 공백을 추가
   - 예: `float thrustPower`, `bool  smoothRotation`, `Vector3 velocity`

2. **등호(=) 수직 정렬**
   - 같은 블록 내 모든 변수의 `=` 기호가 같은 열에 위치하도록 공백 추가
   - SerializeField, private 변수, 메서드 내 할당문 모두 적용

3. **주석 정렬**
   - 주석 시작 위치(`//`)를 수직으로 정렬
   - 값이 길어지면 주석도 그에 맞춰 위치 조정

4. **Header로 그룹화**
   - 관련된 설정들은 `[Header]` 속성으로 그룹화
   - 각 그룹 간에는 빈 줄로 구분

5. **적용 범위**
   - SerializeField 변수 블록
   - Private 변수 블록
   - 메서드 내 지역 변수 선언
   - 메서드 내 연속된 할당문

---

## 콜론(:) 앞뒤 공백 규칙

### 기본 원칙
- **모든 콜론(:) 사용 시** 앞뒤로 공백을 둔다.
- 코드 주석, Markdown 문서, 모든 텍스트 작성에 동일하게 적용

### 예시

#### ✅ 올바른 형식

```markdown
**기간 :** 2025.10.11 ~ 2025.10.12
**작성자 :** Claude
**설명 :** 이것은 올바른 형식입니다.
```

```csharp
// 추진력 : 5f로 설정
// 최대 속도 : 10f
// 설명 : 우주에서의 이동
```

#### ❌ 잘못된 형식

```markdown
**기간:** 2025.10.11 ~ 2025.10.12         // 콜론 뒤 공백 없음
**작성자:**Claude                         // 콜론 앞뒤 공백 없음
** 기간 :**2025.10.11 ~ 2025.10.12       // 콜론 뒤 공백 없음
```

```csharp
// 추진력: 5f로 설정                       // 콜론 뒤 공백 없음
// 최대 속도:10f                           // 콜론 앞뒤 공백 없음
```

### 적용 범위
1. **Markdown 파일 작성**
   - README.MD, CLAUDE.md 등 모든 문서
   - 제목, 설명, 목록 등 모든 텍스트

2. **코드 주석**
   - C# 스크립트의 모든 주석
   - 단일 행 주석 (`//`), 다중 행 주석 (`/* */`)

3. **기타 텍스트**
   - 커밋 메시지
   - 문서화 주석
   - 모든 텍스트 작성

### 예외 사항
- **URL/URI**: `https://example.com` (콜론 뒤 공백 없음)
- **시간 표기**: `12:30`, `14:00` (콜론 앞뒤 공백 없음)
- **네임스페이스/패키지**: `UnityEngine.UI` (점 사용, 해당 없음)

---

## 향후 추가될 규칙들
- (여기에 앞으로 발견되는 코딩 스타일을 추가해 나갈 예정)
