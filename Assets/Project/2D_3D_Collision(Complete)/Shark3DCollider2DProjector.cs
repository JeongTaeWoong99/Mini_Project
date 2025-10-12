using UnityEngine;
using System.Collections.Generic;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// 3D 상어의 2D 콜라이더들을 2D 플레이어 평면에 투영하고 스케일을 보정하는 스크립트
    /// 원근 카메라 환경에서 정확한 2D-3D 충돌 감지를 위해 사용
    /// </summary>
    public class Shark3DCollider2DProjector : MonoBehaviour
    {
        [Header("Camera & Player Settings")] [SerializeField]
        private Camera mainCamera = null; // 메인 카메라 (원근 투영)

        [SerializeField] private Transform player2DTransform = null; // 2D 플레이어 Transform

        [Header("Collider Settings")] [SerializeField]
        private bool autoFindColliders = true; // 시작 시 자동으로 자식 콜라이더 찾기

        [Header("Z Axis Range Settings")] [SerializeField]
        private bool  checkZRange      = true;  // Z축 범위 체크 활성화
        [SerializeField] private float activeZMin      = -5f;  // 활성 Z값 최소 (SharkManager와 동일)
        [SerializeField] private float activeZMax      = 5f;   // 활성 Z값 최대 (SharkManager와 동일)
        [SerializeField] private bool  showZRangeDebug = false; // Z축 범위 디버그 로그

        [Header("Debug Options")] [SerializeField]
        private bool debugMode = true; // 디버그 모드

        [SerializeField] private Color debugLineColor  = Color.yellow; // 디버그 라인 색상

        // Private variables
        private List<Collider2DData> collider2DList = new List<Collider2DData>(); // 2D 콜라이더 데이터 리스트
        private float player2DDepth; // 카메라로부터 2D 평면까지의 거리

        /// <summary>
        /// 2D 콜라이더의 데이터를 저장하는 클래스
        /// </summary>
        [System.Serializable]
        private class Collider2DData
        {
            public Transform  transform;             // 콜라이더 Transform
            public Collider2D collider2D;            // Collider2D 컴포넌트
            public Vector3    originalLocalScale;    // 원래 로컬 스케일
            public Vector3    originalLocalPosition; // 원래 로컬 위치 (부모 기준)
            public Vector3    offsetFromShark;       // 샤크 루트로부터의 오프셋 (월드 공간)
            public string     name;                  // 디버그용 이름

            public Collider2DData(Transform trans, Transform sharkRoot)
            {
                transform             = trans;
                collider2D            = trans.GetComponent<Collider2D>();
                originalLocalScale    = trans.localScale;
                originalLocalPosition = trans.localPosition;

                // 샤크 루트로부터의 월드 공간 오프셋 저장 (Gimbal Lock 회피!)
                offsetFromShark = trans.position - sharkRoot.position;
                name            = trans.name;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Unity Lifecycle Methods
        // ═══════════════════════════════════════════════════════════

        void Start()
        {
            // 메인 카메라 자동 할당
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
            
            if (player2DTransform == null)
            {
                player2DTransform = FindFirstObjectByType<RocketController>().transform;
            }

            // 자동으로 자식 콜라이더들 찾기
            if (autoFindColliders)
            {
                FindAllColliders2D();
            }

            // 2D 플레이어 평면 깊이 계산
            if (player2DTransform != null && mainCamera != null)
            {
                player2DDepth = Mathf.Abs(mainCamera.transform.position.z - player2DTransform.position.z);
                Debug.Log($"[Shark3DProjector] Player 2D Depth: {player2DDepth}");
            }
            else
            {
                Debug.LogWarning("[Shark3DProjector] Player2DTransform 또는 MainCamera가 할당되지 않았습니다!");
            }

            Debug.Log($"[Shark3DProjector] 총 {collider2DList.Count}개의 2D 콜라이더를 찾았습니다.");
        }

        void Update()
        {
            if (mainCamera == null || player2DTransform == null) return;

            // Z축 범위 체크 및 콜라이더 활성화/비활성화
            if (checkZRange)
            {
                UpdateColliderActivation();
            }

            // 모든 콜라이더를 2D 평면에 투영
            ProjectAllCollidersTo2DPlane();
        }

        // ═══════════════════════════════════════════════════════════
        // Z Axis Range Check
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// Z축 범위에 따라 콜라이더 활성화/비활성화
        /// </summary>
        void UpdateColliderActivation()
        {
            // 샤크의 현재 Z 위치
            float sharkZ = transform.position.z;

            // Z축 범위 체크
            bool isInRange = sharkZ >= activeZMin && sharkZ <= activeZMax;

            // 모든 콜라이더 활성화/비활성화
            foreach (Collider2DData data in collider2DList)
            {
                if (data.collider2D == null) continue;

                // 콜라이더 활성화 상태가 변경되었을 때만 로그 출력
                if (data.collider2D.enabled != isInRange)
                {
                    data.collider2D.enabled = isInRange;

                    if (showZRangeDebug)
                    {
                        Debug.Log($"[Shark3DProjector] 샤크 [{transform.name}] Z={sharkZ:F2} → 콜라이더 [{data.name}] {(isInRange ? "활성화" : "비활성화")}");
                    }
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Collider Management
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 모든 자식 오브젝트에서 Collider2D 컴포넌트를 가진 오브젝트를 찾아 리스트에 추가
        /// </summary>
        void FindAllColliders2D()
        {
            collider2DList.Clear();

            // GetComponentsInChildren으로 모든 자식의 Collider2D 찾기
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>(true);

            foreach (Collider2D col in colliders)
            {
                Collider2DData data = new Collider2DData(col.transform, transform); // transform = 샤크 루트
                collider2DList.Add(data);

                if (debugMode)
                {
                    Debug.Log($"[Shark3DProjector] 콜라이더 발견: {data.name} | Offset: {data.offsetFromShark}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Projection & Scale Adjustment
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 모든 2D 콜라이더를 2D 평면에 투영하고 스케일을 보정
        /// </summary>
        void ProjectAllCollidersTo2DPlane()
        {
            foreach (Collider2DData data in collider2DList)
            {
                if (data.transform == null) continue;

                // 1. 샤크의 현재 월드 위치 + 회전된 오프셋으로 3D 위치 계산
                //    Quaternion 사용으로 Gimbal Lock 회피!
                Vector3 rotatedOffset = transform.rotation * data.offsetFromShark;
                Vector3 worldPos3D    = transform.position + rotatedOffset;

                // 2. 3D 월드 좌표를 스크린 좌표로 변환
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos3D);

                // 3. 스크린 좌표를 2D 평면의 월드 좌표로 변환
                Vector3 projectedWorldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, player2DDepth));

                // 4. 위치 업데이트 (화면상 위치는 유지, Z는 2D 평면에 고정)
                data.transform.position = projectedWorldPos;

                // 5. 스케일 보정 (원근 효과 보정)
                float scaleRatio = CalculateScaleRatio(worldPos3D);
                data.transform.localScale = data.originalLocalScale * scaleRatio;
            }
        }

        /// <summary>
        /// 원근 카메라의 깊이 효과에 따른 스케일 비율 계산
        /// </summary>
        /// <param name="objectWorldPos">3D 오브젝트의 월드 위치</param>
        /// <returns>스케일 보정 비율</returns>
        float CalculateScaleRatio(Vector3 objectWorldPos)
        {
            // 카메라로부터 3D 오브젝트까지의 거리
            float object3DDistance = Vector3.Distance(mainCamera.transform.position, objectWorldPos);

            // 거리 비율 계산: (카메라 ↔ 2D평면 거리) / (카메라 ↔ 3D오브젝트 거리)
            float scaleRatio = player2DDepth / object3DDistance;

            return scaleRatio;
        }

        // ═══════════════════════════════════════════════════════════
        // Debug Visualization
        // ═══════════════════════════════════════════════════════════

        void OnDrawGizmos()
        {
            if (!debugMode || mainCamera == null || player2DTransform == null) return;

            // 카메라 위치
            Vector3 cameraPos = mainCamera.transform.position;

            foreach (Collider2DData data in collider2DList)
            {
                if (data.transform == null) continue;

                Vector3 colliderPos = data.transform.position;

                // 카메라 → 콜라이더 라인 (노란색)
                Gizmos.color = debugLineColor;
                Gizmos.DrawLine(cameraPos, colliderPos);

                // 콜라이더 크기만큼 그리드 그리기 (초록색 포인트 제거!)
                DrawColliderGrid(data);

                // 콜라이더 이름 표시
#if UNITY_EDITOR
                UnityEditor.Handles.Label(colliderPos + Vector3.up * 0.5f, $"{data.name}\nScale: {data.transform.localScale.x:F2}"
                );
#endif
            }

            // 2D 플레이어 평면 표시 (파란색)
            if (player2DTransform != null && mainCamera != null)
            {
                Gizmos.color = Color.blue;

                // 플레이어 위치를 중심으로 그리기 (플레이어 따라 이동)
                Vector3 planeCenter = player2DTransform.position;

                // 해당 깊이에서의 카메라 화면 크기 계산 (원근 카메라)
                float distance      = player2DDepth;
                float frustumHeight = 2.0f * distance * Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                float frustumWidth  = frustumHeight * mainCamera.aspect;

                // 화면 크기에 맞춰서 평면 그리기
                Gizmos.DrawWireCube(planeCenter, new Vector3(frustumWidth, frustumHeight, 0.1f));
            }
        }

        /// <summary>
        /// 콜라이더 크기만큼 채워진 그리드를 그려서 시각화
        /// </summary>
        void DrawColliderGrid(Collider2DData data)
        {
            // 콜라이더 타입 확인
            Collider2D col2D = data.transform.GetComponent<Collider2D>();
            if (col2D == null) return;

            // X축 90도 회전 여부 확인
            bool isRotated90 = data.name.Contains("x rotation 90") || data.name.Contains("+ x rotation 90") || data.name.Contains("rotation 90");
            
            // 색상 선택: 정상(파란색) / 90도 회전(주황)
            Color fillColor = isRotated90 ? new Color(1f, 0.5f, 0f, 0.5f) : new Color(0f, 0.5f, 1f, 0.5f);
            Color lineColor = isRotated90 ? new Color(1f, 0.5f, 0f, 1f)   : new Color(0f, 0.5f, 1f, 1f);

            Vector3    center   = data.transform.position;
            Vector3    scale    = data.transform.localScale;
            Quaternion rotation = data.transform.rotation;

            // BoxCollider2D인 경우
            BoxCollider2D boxCol = col2D as BoxCollider2D;
            if (boxCol != null)
            {
                Vector2 size   = boxCol.size;
                float   width  = size.x * scale.x;
                float   height = size.y * scale.y;

                DrawFilledBox(center, width, height, rotation, fillColor, lineColor);
                return;
            }

            // CircleCollider2D인 경우
            CircleCollider2D circleCol = col2D as CircleCollider2D;
            if (circleCol != null)
            {
                float radius = circleCol.radius * Mathf.Max(scale.x, scale.y);
                DrawFilledCircle(center, radius, rotation, fillColor, lineColor);
                return;
            }
        }

        /// <summary>
        /// 채워진 박스 그리기 (회전 적용)
        /// </summary>
        void DrawFilledBox(Vector3 center, float width, float height, Quaternion rotation, Color fillColor,
            Color lineColor)
        {
            // 박스의 4개 꼭지점 (로컬 좌표)
            Vector3[] corners = new Vector3[4]
            {
                new Vector3(-width  / 2, -height / 2, 0),
                new Vector3( width  / 2, -height / 2, 0),
                new Vector3( width  / 2,  height / 2, 0),
                new Vector3(-width  / 2,  height / 2, 0)
            };

            // 회전 적용 후 월드 좌표로 변환
            for (int i = 0; i < 4; i++)
            {
                corners[i] = center + rotation * corners[i];
            }

            // 채워진 면 그리기 (삼각형 2개)
#if UNITY_EDITOR
            UnityEditor.Handles.color = fillColor; // Handles 색상 명시적 설정
            UnityEditor.Handles.DrawSolidRectangleWithOutline(corners, fillColor, lineColor);
            UnityEditor.Handles.color = Color.white; // 색상 리셋
#endif

            // 그리드 선 그리기 (가로/세로 각 5개)
            Gizmos.color = lineColor;
            int gridLines = 5;

            for (int i = 0; i <= gridLines; i++)
            {
                float t = (float)i / gridLines;

                // 가로선
                Vector3 hStart = Vector3.Lerp(corners[0], corners[3], t);
                Vector3 hEnd   = Vector3.Lerp(corners[1], corners[2], t);
                Gizmos.DrawLine(hStart, hEnd);

                // 세로선
                Vector3 vStart = Vector3.Lerp(corners[0], corners[1], t);
                Vector3 vEnd   = Vector3.Lerp(corners[3], corners[2], t);
                Gizmos.DrawLine(vStart, vEnd);
            }
        }

        /// <summary>
        /// 채워진 원 그리기 (회전 적용)
        /// </summary>
        void DrawFilledCircle(Vector3 center, float radius, Quaternion rotation, Color fillColor, Color lineColor)
        {
            int segments = 32;

            // 원의 점들 계산 (회전 적용)
            Vector3[] points = new Vector3[segments];
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
                Vector3 localPoint = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                points[i] = center + rotation * localPoint;
            }

            // 채워진 원 그리기 (삼각형 팬)
#if UNITY_EDITOR
            UnityEditor.Handles.color = fillColor; // Handles 색상 명시적 설정
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                Vector3[] triangle = new Vector3[] { center, points[i], points[next] };
                UnityEditor.Handles.DrawAAConvexPolygon(triangle);
            }

            UnityEditor.Handles.color = Color.white; // 색상 리셋
#endif

            // 테두리
            Gizmos.color = lineColor;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                Gizmos.DrawLine(points[i], points[next]);
            }

            // 십자 그리드 (회전 적용)
            Vector3 right = rotation * new Vector3(radius, 0, 0);
            Vector3 up    = rotation * new Vector3(0, radius, 0);

            Gizmos.DrawLine(center - right, center + right);
            Gizmos.DrawLine(center - up, center + up);

            // 대각선 그리드 (회전 적용)
            Vector3 diag1 = rotation * new Vector3(radius / Mathf.Sqrt(2), radius / Mathf.Sqrt(2), 0);
            Vector3 diag2 = rotation * new Vector3(radius / Mathf.Sqrt(2), -radius / Mathf.Sqrt(2), 0);

            Gizmos.DrawLine(center - diag1, center + diag1);
            Gizmos.DrawLine(center - diag2, center + diag2);
        }
    }
}