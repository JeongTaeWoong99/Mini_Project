# 2D/3D Collision

## 목차
- [📋 개요](#-개요)
- [🎬 인게임 사진](#-인게임-사진)
- [🔗 관련 링크](#-관련-링크)
- [🎮 데이브 더 다이버 기사 분석](#-데이브-더-다이버-기사-분석)
- [🎮 주요 기능](#-주요-기능)

---

## 📋 개요

**기간 :** 2025.10.10 ~ 2025.10.12

Unity에서 **2D 오브젝트와 3D 오브젝트 간의 충돌 감지**를 구현한 미니 프로젝트입니다.

데이브 더 다이버(Dave the Diver)를 플레이해보고, 충돌을 어떻게 구현했을까 궁금하여, 기사를 찾아보고, 구현해 보았습니다.

---

## 🎬 인게임 사진
<img width="1266" height="711" alt="Image" src="https://github.com/user-attachments/assets/fa5ae2a6-44bd-4d30-98b2-fb06124a542f" />
<img width="1267" height="716" alt="Image" src="https://github.com/user-attachments/assets/f3faea0f-b9bd-4fbb-9f09-69503685f6ec" />

---

## 🔗 관련 링크

| 항목 | 링크                                                                   |
|------|----------------------------------------------------------------------|
| 시연 영상 | [바로가기](https://www.youtube.com/watch?v=XyPw2nOJqVI) |

---

## 🎮 데이브 더 다이버 기사 분석

### 기사 출처
**[인벤 - '데이브 더 다이버'의 상어가 데이브를 물어뜯기까지 3단계](https://m.inven.co.kr/webzine/wznews.php?iskin=wdc&hashtag=%EC%9C%A0%EB%8B%88%ED%8B%B0&idx=295852)**

영상을 보고 싶었지만, 아쉽게도 영상이 없어서, 기사를 참고함.

### 🔧 기사 언급 정리

#### 1️⃣ 교차 콜라이더 배치 (Cross Collider)
상어는 Y축과 X축이 모두 변하는 복잡한 회전을 하기 때문에, **2D 콜라이더를 교차(+)해서 배치함.
- 한 콜라이더가 X축 90도로 눕혀져 있어도 다른 콜라이더가 충돌을 감지
- 머리, 몸통, 꼬리에 각각 십자(+) 형태로 콜라이더 배치

#### 2️⃣ Perspective 카메라 + Physics2D 문제
Unity의 **Physics2D는 Orthographic 기준**이지만, 카메라는 **Perspective 사용**

**문제점:**
```
- 화면에 보이는 위치 ≠ 실제 Physics2D 콜라이더 위치
- 3D 오브젝트의 Z값을 Physics2D가 무시
- 맞았는데 충돌 안 되거나, 안 맞았는데 충돌되는 현상
```

#### 3️⃣ 해결 방법: 콜라이더 투영 (Projector)
**데이브 더 다이버의 솔루션:**
1. **3D 콜라이더를 2D 평면으로 이동**
   - `WorldToScreenPoint` → `ScreenToWorldPoint` 변환
   - 깊이값을 "카메라 ↔ 플레이어" 거리로 고정

2. **스케일 자동 보정**
   - 멀리 있던 오브젝트를 앞으로 당기면 크기가 커짐
   - 비율 계산 수식으로 자동 스케일 조정

**결과:**
```
씬(Scene)에서는 모델과 콜라이더가 따로 떨어져 있지만,
게임플레이에서는 스크립트 보정으로 완벽하게 일치
```

---

## 🎮 주요 기능

이 프로젝트의 핵심 스크립트인 **`Shark3DCollider2DProjector.cs`**에 구현된 기술들입니다.

### 1. 원근 카메라 투영 (Perspective Projection)

```csharp
// Step 1: 3D 월드 좌표를 스크린 좌표로 변환
Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos3D);

// Step 2: 스크린 좌표를 2D 평면의 월드 좌표로 변환
Vector3 projectedWorldPos = mainCamera.ScreenToWorldPoint(
    new Vector3(screenPos.x, screenPos.y, player2DDepth)
);
```

**핵심 원리:**
- 3D 오브젝트의 화면상 위치를 계산
- 해당 위치를 2D 플레이어가 있는 평면(Z = 0)으로 투영
- `player2DDepth`: 카메라 ↔ 플레이어 사이의 고정 거리

---

### 2. 거리 기반 스케일 보정 (Distance-based Scale Adjustment)

```csharp
float CalculateScaleRatio(Vector3 objectWorldPos)
{
    // 카메라 ↔ 3D 오브젝트 거리
    float object3DDistance = Vector3.Distance(mainCamera.transform.position, objectWorldPos);

    // 스케일 비율 = (카메라 ↔ 2D평면) / (카메라 ↔ 3D오브젝트)
    return player2DDepth / object3DDistance;
}
```

**원리:**
```
상어가 멀리 있으면 → object3DDistance ↑ → scaleRatio ↓ → 작아짐
상어가 가까이 있으면 → object3DDistance ↓ → scaleRatio ↑ → 커짐
```

**예시:**
```
카메라 ↔ 플레이어: 10m (고정)
카메라 ↔ 상어: 20m → 비율 10/20 = 0.5 → 50% 크기
카메라 ↔ 상어: 5m  → 비율 10/5  = 2.0 → 200% 크기
```

---

### 3. Quaternion 기반 회전 추적 (Gimbal Lock 회피)

```csharp
// ❌ 잘못된 방법: Euler Angle 직접 계산 (Gimbal Lock 발생)
Vector3 offset = RotateByEuler(data.offsetFromShark, transform.eulerAngles);

// ✅ 올바른 방법: Quaternion 사용
Vector3 rotatedOffset = transform.rotation * data.offsetFromShark;
Vector3 worldPos3D    = transform.position + rotatedOffset;
```

**Gimbal Lock이란?**
- 3D 회전에서 특정 각도(90도)에서 회전축이 겹치는 현상
- Euler Angle (X, Y, Z 순서 회전)로 계산하면 발생
- Quaternion을 사용하면 완벽히 회피 가능

---

### 4. Unity 2D Physics 버그 방지 (90° Rotation Bug)

**Unity 버그:**
```
BoxCollider2D에 X축 또는 Y축 회전 90도가 되면
→ "Shape Count = 0" 오류(콜라이더가 사라져서, 콜라이더 감지가 안 된다는)가 발생하면서,
→ 콜라이더 위치가 (0, 0, 0), 크기가 (1 , 1)로 리셋되는 문제 발생
```

**해결책:**
```csharp
bool IsRotationNear90Degrees(Vector3 eulerAngles)
{
    float x = NormalizeAngle(eulerAngles.x);
    float y = NormalizeAngle(eulerAngles.y);
    float threshold = 1f;  // ±1도 범위

    bool xNear90 = (Mathf.Abs(x - 90f) < threshold) || (Mathf.Abs(x + 90f) < threshold);
    bool yNear90 = (Mathf.Abs(y - 90f) < threshold) || (Mathf.Abs(y + 90f) < threshold);

    return xNear90 || yNear90;
}
```

**동작:**
- X/Y축 회전이 89° ~ 91° 범위에 들어가면 감지
- 해당 콜라이더를 자동으로 비활성화
- 범위를 벗어나면 다시 활성화

---

### 5. 이상값 자동 감지 및 복구 (Abnormal Value Detection)

```csharp
bool ValidateColliderValues(Collider2DData data, Vector3 position, Vector3 scale, float scaleRatio)
{
    // NaN 체크 (0/0 같은 계산 오류)
    if (float.IsNaN(position.x) || float.IsNaN(scale.x)) return false;

    // Infinity 체크 (1/0 같은 계산 오류)
    if (float.IsInfinity(scaleRatio)) return false;

    // 극단값 체크 (position > 100000 등)
    if (Mathf.Abs(position.x) > 100000f) return false;

    // 0 나누기 위험 (scaleRatio가 너무 작음)
    if (Mathf.Abs(scaleRatio) < 0.0001f) return false;

    // 음수 스케일 (물리 엔진 오류 유발)
    if (scale.x < 0f) return false;

    return true;
}
```

**검증 항목:**
- ✅ NaN (Not a Number)
- ✅ Infinity (무한대)
- ✅ 극단적으로 큰 값
- ✅ 극단적으로 작은 값
- ✅ 음수 스케일

이상값 감지 시 → 콜라이더 비활성화 → Unity 크래시 방지

---

### 6. 교차 콜라이더 배치 (Cross Collider Pattern)

**상어 구조:**
```
Shark (3D Model)
├── Head
│   ├── Circle2D/side                  (기본 원형)
│   └── Circle2D/side + x rotation 90  (X축 90도 회전)
├── Body
│   ├── Box2D/side                     (기본 박스)
│   └── Box2D/side + x rotation 90     (X축 90도 회전)
└── Tail
    ├── Circle2D/side                  (기본 원형)
    └── Circle2D/side + x rotation 90  (X축 90도 회전)
```


- 한 콜라이더가 X축 90도로 눕혀져도
- 다른 콜라이더가 충돌 감지
- 십자(+) 형태로 모든 각도 커버

---

### 7. Z축 범위 기반 콜라이더 활성화 (Z-Range Collider Activation)

**문제점:**
```
3D 상어가 Z축 -5 ~ 5 범위를 벗어나면
→ 2D 플레이어와 충돌하면 안 되는데 충돌함
→ 화면 밖 (앞/뒤)에 있는데도 충돌 감지
```

**해결책:**
```csharp
void UpdateColliderActivation()
{
    // 샤크의 현재 Z 위치
    float sharkZ = transform.position.z;

    // Z축 범위 체크 (SharkManager와 동일한 범위)
    bool isInRange = sharkZ >= activeZMin && sharkZ <= activeZMax;

    // 모든 콜라이더 활성화/비활성화
    foreach (Collider2DData data in collider2DList)
    {
        if (data.collider2D == null) continue;

        // 범위 밖이면 콜라이더 비활성화
        data.collider2D.enabled = isInRange;
    }
}
```

**동작:**
- Z축 -5 ~ 5 범위 안 : 콜라이더 활성화 → 충돌 감지 O
- Z축 -5 ~ 5 범위 밖 : 콜라이더 비활성화 → 충돌 감지 X
- 상태 변경 시에만 로그 출력 (성능 최적화)
- `checkZRange = true` 설정으로 활성화/비활성화 가능

**중요:**
- SharkManager의 `activeZMin/Max`와 동일한 값 사용 (기본 -5 ~ 5)
- Z축 범위 밖 상어는 공격 대상에서도 제외됨 (RocketController)

---

### 8. 디버그 시각화 (Debug Visualization)

**OnDrawGizmos()로 실시간 확인:**
- 파란색 그리드 : 일반 콜라이더
- 주황색 그리드 : X축 90도 회전 콜라이더
- 채워진 면 + 격자 무늬로 크기/위치 확인
- Scene 뷰에서 실시간 투영 결과 확인

```csharp
void DrawColliderGrid(Collider2DData data)
{
    // 90도 회전 여부에 따라 색상 변경
    bool isRotated90 = data.name.Contains("x rotation 90");
    Color fillColor = isRotated90 ? Orange : Blue;

    // 채워진 박스 + 그리드 그리기
    DrawFilledBox(center, width, height, rotation, fillColor, lineColor);
}
```
