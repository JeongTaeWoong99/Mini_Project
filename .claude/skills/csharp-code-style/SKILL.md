---
name: csharp-code-style
description: 이 Unity 프로젝트에서 C# 스크립트(.cs)를 작성하거나 수정할 때 적용하는 사용자 코딩 스타일 규칙. 변수 정렬, 콜론 공백, 중괄호 위치(Allman), 멤버 배치 순서를 정의한다. C# 코드를 새로 작성·편집·리팩터링할 때 반드시 참고할 것.
---

# C# 코딩 스타일 가이드

이 프로젝트의 모든 C# 스크립트는 아래 규칙을 따른다.

## 1. 멤버 배치 순서 (표준 형식)

클래스 내부 멤버는 항상 다음 순서로 배치한다.

1. **필드 / 프로퍼티** (변수) — 최상단
2. `//---` 구분선
3. **생명주기 메서드** (Awake, OnEnable, Start, OnValidate, Update, FixedUpdate, OnTrigger/OnCollision 등 Unity 메시지)
4. **일반 메서드** (public → protected → private)

- 필드를 클래스 하단에 두지 않는다. **변수가 먼저, 메서드가 나중**이다.
- 필드 블록과 메서드 블록 사이에는 `//---...` 구분선을 둔다.
- 핵심 동작 메서드(예 : Update 에서 호출되는 override 로직)는 일반 메서드 영역의 맨 앞에 둘 수 있다.

```csharp
public class Example : MonoBehaviour
{
    [SerializeField] private float speed = 5f; // 필드 : 최상단

//----------------------------------------------------------------------------------------------------------------------

    private void Awake()   { }                 // 생명주기 메서드
    private void OnValidate() { }

    public void DoSomething() { }              // 일반 메서드
}
```

## 2. 중괄호 위치 (Allman 스타일)

여는 중괄호 `{` 는 **항상 다음 줄**에 둔다. 클래스 · 메서드 · `if` · `for` · `while` · `switch` 등 **모든 블록**에 적용한다.

```csharp
// ✅ 올바른 형식
protected override void UpdateInternalV()
{
    if (inputReader.ReadJump())
    {
        velocity.y = 5;
    }
}

// ❌ 잘못된 형식 (K&R)
protected override void UpdateInternalV() {
    if (inputReader.ReadJump()) {
        velocity.y = 5;
    }
}
```

- 조건이 한 줄이어도 중괄호를 생략하지 않고 Allman 형식으로 명시한다.

## 3. 변수 정렬 (`=` · `//` 수직 정렬)

연속된 변수 선언/할당에서 `=` 기호와 주석 `//` 를 같은 열에 정렬한다.

```csharp
[Header("Movement Settings")]
[SerializeField] private float thrustPower     = 5f;    // 추진력
[SerializeField] private float maxSpeed        = 10f;   // 최대 속도
[SerializeField] private float arrivalDistance = 1f;    // 도착 판정 거리

private Vector3 velocity;                 // 현재 속도
private float   currentThrust;            // 현재 추진력
private bool    isMoving      = false;
```

- 타입과 변수명 사이 공백을 조정해 변수명을 정렬한다. (`float thrustPower`, `bool  smoothRotation`)
- 같은 블록 내 모든 `=` 가 같은 열에 오도록 공백을 추가한다.
- 주석 시작(`//`)도 수직 정렬한다. 값이 길어지면 주석 위치도 맞춰 조정한다.
- 관련 설정은 `[Header]` 로 그룹화하고, 그룹 사이는 빈 줄로 구분한다.
- SerializeField · private 변수 · 메서드 내 지역 변수 · 연속 할당문 모두 동일하게 적용한다.

## 4. 콜론(`:`) 앞뒤 공백

모든 콜론 사용 시 앞뒤에 공백을 둔다. (코드 주석, Markdown 문서, 커밋 메시지 등 모든 텍스트)

```markdown
**기간 :** 2025.10.11 ~ 2025.10.12
**작성자 :** Claude
```

```csharp
// 추진력 : 5f 로 설정
m_moveAction = currentMap.FindAction("Move", throwIfNotFound : false);

switch (i)
{
    case 0  : return Input.GetKeyDown(KeyCode.Z);
    default : return false;
}
```

**예외 :**
- URL/URI : `https://example.com` (공백 없음)
- 시간 표기 : `12:30`, `14:00` (공백 없음)
- 네임스페이스/패키지 : `UnityEngine.UI` (점 사용, 해당 없음)

## 5. 들여쓰기

- 각 파일의 기존 들여쓰기 방식(탭/스페이스)을 일관되게 유지한다.
- 기존 코드를 수정할 때는 주변 코드의 스타일을 따른다.

## 6. 동작 보존 (리팩터링 시 주의)

- 클래스명 · 메서드명 · 변수명(특히 `[SerializeField]` 직렬화 필드), 네임스페이스, 로직은 함부로 바꾸지 않는다.
  직렬화 필드명을 바꾸면 Prefab/Scene 인스펙터 연결이 끊긴다. (부득이할 경우 `[FormerlySerializedAs]` 사용)
- `#if ENABLE_INPUT_SYSTEM` 등 전처리 분기는 그대로 유지한다.
- 스타일 정리는 **포맷과 주석**에 한정하고, 동작을 변경하지 않는다.
