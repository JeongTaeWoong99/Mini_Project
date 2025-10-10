# 2D/3D Collision System

Unity에서 2D 오브젝트와 3D 오브젝트 간의 충돌 감지를 구현한 미니 프로젝트입니다.

데이브 더 다이버(Dave the Diver)를 플레이해보고, 충돌을 어떻게 구현했을까 궁금하여, 기사를 찾아보고, 구현해 보았습니다.

여러 가지 방법을 시도해 보았고, 그중 가장 잘 구현된 방법으로 마무리 및 정리함.

하지만, 아직도 제대로 회전하는 3D 오브젝트를 2D 콜라이더로 커버하는데 한계를 느낌.

추후, 다시 해볼 예정...

---

## 📋 프로젝트 개요

### 문제점
Unity에서 2D 콜라이더와 3D 콜라이더는 기본적으로 상호작용하지 않습니다:
- **2D Physics** : BoxCollider2D, PolygonCollider2D, Rigidbody2D
- **3D Physics** : BoxCollider, MeshCollider, Rigidbody

2D 플레이어가 3D 월드의 오브젝트(물고기, 적 등)와 충돌하려면 특별한 처리가 필요합니다.

### 해결 방법
**카메라 기반 투영(Shadow Casting)** 방식으로 3D 오브젝트를 2D 평면에 투영하여 2D 콜라이더를 자동 생성합니다.

---

## 🎮 데이브 더 다이버 기사 분석

### 기사 출처
[인벤 - 데이브 더 다이버 개발기: 2D와 3D의 조화](https://www.inven.co.kr/webzine/news/?news=295852)

### 핵심 기술 분석

#### 1. Shadow Casting 방식
```
3D 오브젝트 → 카메라 투영 → 2D 평면 재투영 → PolygonCollider2D 생성
```

- 3D 메쉬의 실루엣(윤곽선)을 카메라 시점에서 추출
- 카메라에서 보이는 모습 그대로를 2D 콜라이더로 변환
- 회전/이동 시에도 카메라 시점 기준으로 동적 업데이트

#### 2. 구현 과제
기사에서 언급된 기술적 도전 과제:
- Unity의 2D/3D 물리 엔진 분리 문제
- Perspective 카메라와 Orthographic 물리의 정렬 문제
- 3D 오브젝트 회전 시 2D 콜라이더 동기화
- 성능 최적화 (매 프레임 계산 부담)

#### 3. 적용 방법
1. **3D Bounds 추출** : Renderer.bounds의 8개 코너 사용
2. **카메라 투영** : WorldToScreenPoint로 화면 좌표 계산
3. **2D 평면 재투영** : ScreenToWorldPoint로 플레이어 Z 평면에 투영
4. **Convex Hull** : 투영된 점들의 외곽선 추출
5. **PolygonCollider2D 적용** : 로컬 좌표로 변환 후 콜라이더 업데이트

---

## 🛠️ 구현 상세

### 실루엣 Edge 추출 알고리즘

현재 구현은 **실루엣 Edge 방식**을 사용합니다:

```csharp
// 1. 각 삼각형의 앞/뒤면 판정
Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0);
bool isFrontFacing = Vector3.Dot(normal, toCamera) > 0;

// 2. Edge 공유 삼각형 확인
//    - 한쪽은 앞면, 한쪽은 뒷면 → 실루엣 Edge!

// 3. 실루엣 Edge의 끝점만 투영
WorldToScreenPoint → ScreenToWorldPoint
```

### Convex Hull (Graham Scan)

투영된 점들의 외곽선을 O(n log n) 시간에 계산:

```csharp
// 1. 가장 아래/왼쪽 점을 기준점으로 설정
// 2. 기준점 기준으로 각도 정렬 (Cross Product)
// 3. Graham Scan으로 Convex Hull 추출
```

### 성능 최적화

1. **점 간소화** : 그리드 기반 중복 제거 (100개 이상일 때만)
2. **업데이트 간격** : 매 프레임 대신 3~5 프레임마다
3. **디버그 비활성화** : 기본값 false로 설정

---

## 📁 스크립트 설명

### Fish3DCollider2D.cs
**3D 오브젝트에 2D 콜라이더를 자동 생성하는 핵심 스크립트**

#### 주요 기능:
- 카메라 기반 투영으로 3D → 2D 변환
- 실루엣 Edge 추출로 정확한 외곽선 계산
- PolygonCollider2D 자동 생성/업데이트
- 플레이어 Z 위치 자동 감지

#### Inspector 설정:
```
[Projection Settings]
- Target Camera : 투영 카메라 (null이면 Camera.main)
- Projection Plane Z : 2D 투영 평면 Z 위치
- Auto Detect Player Z : 플레이어 Z 자동 감지

[Collider Settings]
- Use Polygon Collider : PolygonCollider2D 사용 여부
- Polygon Points : 폴리곤 점 개수 (32 권장)
- Size Multiplier : 콜라이더 크기 배율
- Is Trigger : Trigger로 사용

[Update Settings]
- Update Every Frame : 매 프레임 업데이트 (false 권장)
- Update Interval : 업데이트 간격 (3~5 프레임)
```

---

## ⚙️ 성능 고려사항

### 최적화 팁

1. **업데이트 간격 조정**
   - `updateEveryFrame = false`
   - `updateInterval = 3~5` (프레임)

2. **디버그 비활성화**
   - `showDebug = false`
   - `showGizmos = false`
   - `showProjection = false`

3. **폴리곤 점 개수**
   - 32개 권장 (정확도와 성능의 균형)
   - 너무 많으면 Convex Hull 계산 부담 증가

4. **Bounds 기반 폴백**
   - 메쉬 접근 실패 시 자동으로 Bounds 기반 폴리곤 사용
   - Read/Write 비활성화 상태에서도 작동