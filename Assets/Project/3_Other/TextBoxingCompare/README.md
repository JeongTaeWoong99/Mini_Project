# 유니티 텍스트 출력 방법 비교

**작성일 :** 2026-05-31
**씬 :** TextBoxingCompare.unity

---

## 개요

유니티에서 숫자를 텍스트로 출력할 때 선택할 수 있는 5가지 방법을 비교한다.

각 방법이 내부적으로 어떻게 동작하는지, GC 할당이 몇 회 발생하는지 정리한다.

예제는 30~240 FPS 범위를 표시하는 상황을 기준으로 한다.

---

## 방법 1 : 문자열 연결 (Boxing 발생)

```csharp
text.text = "FPS : " + fps; // fps는 int
```

**한 줄 요약 :** 정수를 string과 `+`로 직접 연결해서, `int`가 `object`로 박싱되어 GC 부담을 주는 방법.

### 내부 동작

C# 컴파일러는 `string + int` 조합에서 `String.Concat(object, object)` 오버로드를 선택한다.
`int`는 `object`가 아니므로, IL(중간 언어) 레벨에서 `box` 명령어가 먼저 실행된다.

```
// IL (단순화)
ldstr   "FPS : "
ldloc   fps
box     System.Int32                  ← int → object 박싱     (힙 할당 #1)
call    String.Concat(object, object) ← 연결된 string 생성     (힙 할당 #2)
stfld   text.text
```

### 단점

- 매 호출마다 **힙 할당 2회** (박싱된 임시 객체 + 연결 결과 string)
- 박싱된 임시 객체는 즉시 버려지므로 GC 압력을 높이는 순수 낭비

---

## 방법 2 : ToString() 직접 호출

```csharp
text.text = "FPS : " + fps.ToString();
```

**한 줄 요약 :** `ToString()`으로 박싱 없이 string으로 변환 후 연결하는 방법. 박싱은 없지만 string 할당 2회는 그대로.

### 내부 동작

`fps.ToString()`처럼 **값 타입 변수에서 직접 메서드를 호출**하면,
컴파일러는 IL에 `constrained.` 접두사를 붙인다.

```
// IL (단순화)
ldstr    "FPS : "
ldloca.s fps
constrained. System.Int32              ← 값 타입 + 메서드 직접 구현 여부 확인
callvirt System.Object::ToString()     ← (할당 #1) box 없이 직접 호출, string 반환
call     String.Concat(string, string) ← (할당 #2) 연결된 string 생성
stfld    text.text
```

`constrained.callvirt` 의 동작 규칙 :

| 조건 | 동작 |
|------|------|
| 값 타입이 해당 메서드를 **직접 오버라이드**함 | box 없이 직접 호출 |
| 값 타입이 해당 메서드를 오버라이드하지 않음   | box 후 호출 (boxing 발생) |
| `System.Int32`는 `ToString()`을 직접 오버라이드 | → **박싱 없음** |

### 방법 1과의 차이

| 항목 | 방법 1 (+ 연결) | 방법 2 (ToString) |
|------|-----------------|-------------------|
| 박싱 | 발생 (임시 object) | **없음** |
| string 할당 | 2회 | 2회 |
| IL 명령 | `box` + `Concat(object,object)` | `constrained.callvirt` + `Concat(string,string)` |

힙 할당 **횟수는 동일**하지만, 방법 1은 박싱된 임시 객체라는 **쓸모없는 추가 할당**이 있다.

### 단점

- 문자열 연결(`+`) 자체는 여전히 새 string 객체를 생성함
- 매 호출마다 힙 할당 2회 (ToString 결과 + 연결 결과)

---

## 방법 3 : 문자열 캐싱 (String Caching)

```csharp
// Start에서 한 번만
_fpsCache = new string[maxFPS + 1];
for (int i = minFPS; i <= maxFPS; i++)
    _fpsCache[i] = "FPS : " + i.ToString();

// Update에서
text.text = _fpsCache[fps];
```

**한 줄 요약 :** Start에서 값 범위(30~240)의 모든 string을 미리 만들어두고, Update에서 인덱스로 꺼내 재사용하는 방법.

### 내부 동작

값 범위(30~240)의 모든 string을 **Start에서 미리 생성**해 배열에 보관한다.
Update에서는 인덱스로 참조를 꺼내 `.text`에 할당하기만 한다.

```
// Update IL (단순화)
ldloc   _fpsCache
ldloc   fps
ldelem  string               ← 배열 참조 읽기, 힙 할당 없음
stfld   text.text
```

### 방법 2와의 차이

방법 2는 **매 호출마다** string을 새로 만든다.
방법 3은 Start에서 만든 string을 **계속 재참조**한다.
런타임 문자열 변환 자체가 없다.

### 단점

- 값 범위를 **미리 알아야** 함 (범위 외 값은 처리 불가)
- 범위가 넓을수록 메모리 사용량 증가 (30~240 : 211개 string)
- 범위 밖의 값이 들어오면 별도 처리가 필요함

---

## 방법 4 : TMP_Text.SetText()

```csharp
tmpText.SetText("FPS : {0}", fps);
```

**한 줄 요약 :** TMP 내부 `char[]` 버퍼에 숫자를 직접 기록해서, string 객체 없이 텍스트를 갱신하는 방법.

### 내부 동작

TMP(TextMeshPro)는 내부에 `char[]` 배열 버퍼(`m_TextBackingArray`)를 보유한다.
`SetText(string, float/int)` 오버로드는 이 버퍼에 **직접 숫자를 문자로 변환**해 기록한다.

```
// TMP 내부 동작 (단순화)
1. 포맷 문자열 "FPS : {0}" 의 "{0}"을 찾음
2. fps (int)를 내부 메서드로 char[] 버퍼에 직접 변환 기록
   → int → string 변환 없음, 박싱 없음
3. 내부 char[] 버퍼를 기반으로 메시 데이터 갱신
   → 새 string 객체 생성 없음
```

### 방법 3과의 차이

| 항목 | 방법 3 (캐싱) | 방법 4 (SetText) |
|------|--------------|-----------------|
| 런타임 할당 | 0회 | 0회 |
| 사전 작업 | 캐시 배열 생성 필요 | 없음 |
| 값 범위 제한 | 있음 | **없음** |
| 적용 가능 컴포넌트 | TMP, Legacy Text | **TMP 전용** |

### 단점

- **TMP(TextMeshPro) 전용**, Legacy Text(`UnityEngine.UI.Text`)에는 사용 불가
- 포맷이 `{0}`, `{1}` 형식으로 제한됨
- `float` 인자를 받으므로 `int`를 넘겨도 float으로 처리됨 (정밀도 주의)

---

## 방법 5 : StringBuilder

```csharp
// Start에서 한 번만
_sb = new StringBuilder(32);

// Update에서
_sb.Clear();
_sb.Append("FPS : ");
_sb.Append(fps);         // Append(int) 오버로드 → 박싱 없음
text.text = _sb.ToString();
```

**한 줄 요약 :** 내부 `char[]`에 각 조각을 차례로 쌓아두었다가, 마지막 `ToString()` 한 번만으로 string을 1회 생성하는 방법.

### 내부 동작

StringBuilder는 내부 `char[]` 배열을 가지며, `Clear()`는 인덱스만 초기화한다 (할당 없음).

```
Append("FPS : ") → char[]에 복사, 할당 없음
Append(fps)      → Append(int) 오버로드, int를 char[]에 직접 변환, 박싱 없음
ToString()       → char[] 내용으로 새 string 생성, 힙 할당 #1
```

### 방법 2와의 차이

| 항목 | 방법 2 (ToString) | 방법 5 (StringBuilder) |
|------|------------------|------------------------|
| 박싱 | 없음 | 없음 |
| 힙 할당 횟수 | 2회 | **1회** |
| 중간 string 생성 | ToString() 결과 string | **없음** (char[] 직접 사용) |

방법 2는 `fps.ToString()` 결과 string + `Concat` 결과 string = 2회 할당.
방법 5는 `sb.ToString()` 결과 string = **1회만** 할당.

### 단점

- `sb.ToString()` 시점에 string 할당은 **피할 수 없음** (`.text`에 string이 필요하기 때문)
- 방법 3, 4보다 코드가 길고 verbose함
- StringBuilder를 `new` 하지 않고 반드시 **재사용**해야 의미 있음

---

## 전체 비교 요약

| 방법 | 박싱 | 런타임 할당 | 범위 제한 | TMP 전용 | 코드 복잡도 |
|------|------|-------------|-----------|---------|------------|
| 1. 문자열 연결 | **있음** | **2회** | 없음 | 아니오 | 낮음 |
| 2. ToString() | 없음 | 2회 | 없음 | 아니오 | 낮음 |
| 3. 문자열 캐싱 | 없음 | **0회** | **있음** | 아니오 | 중간 |
| 4. TMP SetText | 없음 | **0회** | **없음** | **예** | 낮음 |
| 5. StringBuilder | 없음 | 1회 | 없음 | 아니오 | 중간 |

---

## 권장 사용 상황

- **일반적인 경우, TMP 사용 중** → 방법 4 (`SetText`)
- **값 범위가 정해진 경우, 최고 성능 필요** → 방법 3 (캐싱)
- **Legacy Text 사용 중, 최소 개선** → 방법 5 (StringBuilder) 또는 방법 2 (ToString)
- **방법 1은 가능하면 지양** (GC 압력 불필요하게 증가)
