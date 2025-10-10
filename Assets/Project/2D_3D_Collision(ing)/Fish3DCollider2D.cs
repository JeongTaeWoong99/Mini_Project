using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 데이브더다이브 Shadow Casting 방식:
/// 카메라 시점에서 3D 오브젝트를 2D 평면에 투영하여 콜라이더 생성
/// </summary>
public class Fish3DCollider2D : MonoBehaviour
{
    [Header("Projection Settings - 카메라 기반 투영")]
    [SerializeField] private Camera targetCamera;              // 투영에 사용할 카메라 (null이면 Camera.main)
    [SerializeField] private float  projectionPlaneZ  = 0f;    // 2D 투영 평면의 Z 위치 (2D 플레이어와 같은 값)
    [SerializeField] private bool   autoDetectPlayerZ = true;  // 자동으로 플레이어의 Z 위치 감지

    [Header("Collider Settings")]
    [SerializeField] private bool  usePolygonCollider = true;            // PolygonCollider2D 사용 (권장)
    [SerializeField] private int   polygonPoints      = 32;              // Polygon 점 개수 (32 권장)
    [SerializeField] private float sizeMultiplier     = 1f;              // 콜라이더 크기 배율
    [SerializeField] private bool  isTrigger          = true;            // Trigger로 사용 (권장)
    [SerializeField] private float pointGridSize      = 0.1f;            // 점 간소화 그리드 크기 (성능 최적화)

    [Header("Update Settings")]
    [SerializeField] private bool updateEveryFrame = false;              // 매 프레임 업데이트 (false 권장)
    [SerializeField] private int  updateInterval   = 3;                  // 업데이트 간격 (프레임, 3~5 권장)

    [Header("Debug")]
    [SerializeField] private bool showDebug           = false;           // 디버그 정보 표시
    [SerializeField] private bool showGizmos          = false;           // 기즈모 표시
    [SerializeField] private bool showProjection      = false;           // 투영 과정 표시
    [SerializeField] private bool showProjectedPoints = false;           // 투영된 점들 표시

    // Private variables
    private BoxCollider2D     boxCollider2D;
    private PolygonCollider2D polygonCollider2D;
    private int               frameCount = 0;
    private Camera            cachedCamera;
    private GameObject        playerObject; // 플레이어 오브젝트 (Z 위치 감지용)

    void Start()
    {
        InitializeCamera();
        InitializeCollider();

        // 플레이어 Z 위치 자동 감지
        if (autoDetectPlayerZ)
        {
            DetectPlayerZPosition();
        }

        // 첫 업데이트
        UpdateCollider2D();
    }

    void Update()
    {
        // 플레이어 Z 위치 지속적으로 업데이트
        if (autoDetectPlayerZ && playerObject != null)
        {
            projectionPlaneZ = playerObject.transform.position.z;
        }

        if (!updateEveryFrame)
        {
            frameCount++;
            if (frameCount >= updateInterval)
            {
                frameCount = 0;
                UpdateCollider2D();
            }
        }
        else
        {
            UpdateCollider2D();
        }
    }

    private void InitializeCamera()
    {
        if (targetCamera == null)
        {
            cachedCamera = Camera.main;
            if (cachedCamera == null)
            {
                Debug.LogError("Fish3DCollider2D: 카메라를 찾을 수 없습니다!");
                enabled = false;
            }
        }
        else
        {
            cachedCamera = targetCamera;
        }
    }

    private void DetectPlayerZPosition()
    {
        // RocketController를 가진 오브젝트 찾기
        RocketController player = FindFirstObjectByType<RocketController>();
        if (player != null)
        {
            playerObject     = player.gameObject;
            projectionPlaneZ = player.transform.position.z;

            // if (showDebug)
            // {
            //     Debug.Log($"Fish3DCollider2D: 플레이어 Z 위치 감지 = {projectionPlaneZ}");
            // }
        }
        // else
        // {
        //     if (showDebug)
        //     {
        //         Debug.LogWarning("Fish3DCollider2D: 플레이어를 찾을 수 없습니다. Z=0 사용");
        //     }
        // }
    }

    private void InitializeCollider()
    {
        if (usePolygonCollider)
        {
            polygonCollider2D = GetComponent<PolygonCollider2D>();
            if (polygonCollider2D == null)
            {
                polygonCollider2D           = gameObject.AddComponent<PolygonCollider2D>();
                polygonCollider2D.isTrigger = isTrigger;

                // if (showDebug)
                // {
                //     Debug.Log($"Fish3DCollider2D: {gameObject.name}에 PolygonCollider2D 추가됨");
                // }
            }
        }
        else
        {
            boxCollider2D = GetComponent<BoxCollider2D>();
            if (boxCollider2D == null)
            {
                boxCollider2D           = gameObject.AddComponent<BoxCollider2D>();
                boxCollider2D.isTrigger = isTrigger;

                // if (showDebug)
                // {
                //     Debug.Log($"Fish3DCollider2D: {gameObject.name}에 BoxCollider2D 추가됨");
                // }
            }
        }
    }

    private void UpdateCollider2D()
    {
        if (cachedCamera == null) return;

        if (usePolygonCollider && polygonCollider2D != null)
        {
            UpdatePolygonCollider_CameraProjection();
        }
        else if (!usePolygonCollider && boxCollider2D != null)
        {
            UpdateBoxCollider_CameraProjection();
        }
    }

    /// <summary>
    /// 카메라 기반 투영: 실루엣 Edge를 추출하여 정확한 외곽선 생성
    /// </summary>
    private void UpdatePolygonCollider_CameraProjection()
    {
        // 1. 실루엣 Edge 추출 (앞면/뒷면 경계선만)
        List<Vector3> silhouetteEdges = ExtractSilhouetteEdges();

        if (silhouetteEdges.Count == 0)
        {
            // 실패 시 Bounds 기반 폴리곤 사용
            Vector2[] fallbackPoints = GenerateBoundsBasedPolygon();
            polygonCollider2D.points = fallbackPoints;
            return;
        }

        // 2. 실루엣 Edge를 2D 평면에 투영
        List<Vector2> projectedPoints = ProjectVerticesToPlane(silhouetteEdges);

        // 2.5. 점 간소화 (중복 제거)
        projectedPoints = SimplifyPoints(projectedPoints, pointGridSize);

        // 디버그: 투영된 점들 저장 - 제거됨 (더 이상 사용 안 함)
        // if (showGizmos || showProjectedPoints)
        // {
        //     lastProjectedPoints = new List<Vector2>(projectedPoints);
        // }

        // if (showDebug)
        // {
        //     Debug.Log($"Fish3DCollider2D [{gameObject.name}]: 실루엣 Edge {silhouetteEdges.Count / 2}개 → 투영 후 {projectedPoints.Count}개 점");
        // }

        if (projectedPoints.Count < 3)
        {
            // 투영 실패 시 Bounds 기반 폴리곤 사용
            Vector2[] fallbackPoints = GenerateBoundsBasedPolygon();
            polygonCollider2D.points = fallbackPoints;

            // if (showDebug)
            // {
            //     Debug.LogWarning($"Fish3DCollider2D [{gameObject.name}]: 투영된 점이 {projectedPoints.Count}개로 부족 → Bounds 기반 폴백 사용");
            // }
            return;
        }

        // 3. 외곽선 추출 (Convex Hull 사용 - 실루엣 edge의 끝점들이므로 이제는 정확함)
        Vector2[] outlinePoints = ExtractConvexHull(projectedPoints);

        // if (showDebug)
        // {
        //     Debug.Log($"Fish3DCollider2D [{gameObject.name}]: 최종 Polygon 점 개수 {outlinePoints.Length}개");
        // }

        // 4. 로컬 좌표로 변환
        for (int i = 0; i < outlinePoints.Length; i++)
        {
            // 월드 → 로컬
            Vector3 worldPoint = new Vector3(outlinePoints[i].x, outlinePoints[i].y, projectionPlaneZ);
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            outlinePoints[i]   = new Vector2(localPoint.x, localPoint.y) * sizeMultiplier;
        }

        // 5. 콜라이더에 적용
        polygonCollider2D.points = outlinePoints;
    }

    private void UpdateBoxCollider_CameraProjection()
    {
        // BoxCollider는 간단하게 Bounds 기반
        Vector2[] points = GenerateBoundsBasedPolygon();

        // AABB 계산
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        foreach (Vector2 p in points)
        {
            min.x = Mathf.Min(min.x, p.x);
            min.y = Mathf.Min(min.y, p.y);
            max.x = Mathf.Max(max.x, p.x);
            max.y = Mathf.Max(max.y, p.y);
        }

        Vector2 center = (min + max) * 0.5f;
        Vector2 size   = (max - min) * sizeMultiplier;

        boxCollider2D.offset = center;
        boxCollider2D.size   = size;
    }

    /// <summary>
    /// 실루엣 Edge 추출: 앞면과 뒷면이 만나는 경계선만 추출
    /// 이것이 진짜 Shadow Casting 방식!
    /// </summary>
    private List<Vector3> ExtractSilhouetteEdges()
    {
        List<Vector3> silhouetteVertices = new List<Vector3>();

        if (cachedCamera == null) return silhouetteVertices;

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

        foreach (MeshFilter meshFilter in meshFilters)
        {
            if (meshFilter.sharedMesh == null) continue;

            Mesh mesh = meshFilter.sharedMesh;

            try
            {
                Vector3[]  vertices  = mesh.vertices;
                int[]      triangles = mesh.triangles;
                Transform  meshTransform = meshFilter.transform;
                Vector3    cameraPosition = cachedCamera.transform.position;

                // 1. 각 삼각형의 앞/뒤 판정
                bool[] isFrontFacing = new bool[triangles.Length / 3];

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int triIndex = i / 3;

                    Vector3 v0 = meshTransform.TransformPoint(vertices[triangles[i]]);
                    Vector3 v1 = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
                    Vector3 v2 = meshTransform.TransformPoint(vertices[triangles[i + 2]]);

                    Vector3 normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                    Vector3 toCamera = (cameraPosition - v0).normalized;

                    isFrontFacing[triIndex] = Vector3.Dot(normal, toCamera) > 0;
                }

                // 2. Edge-삼각형 매핑 (각 edge가 어느 삼각형들에 속하는지)
                Dictionary<Edge, List<int>> edgeToTriangles = new Dictionary<Edge, List<int>>();

                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int triIndex = i / 3;

                    // 삼각형의 3개 edge
                    Edge[] edges = new Edge[]
                    {
                        new Edge(triangles[i], triangles[i + 1]),
                        new Edge(triangles[i + 1], triangles[i + 2]),
                        new Edge(triangles[i + 2], triangles[i])
                    };

                    foreach (Edge edge in edges)
                    {
                        if (!edgeToTriangles.ContainsKey(edge))
                        {
                            edgeToTriangles[edge] = new List<int>();
                        }
                        edgeToTriangles[edge].Add(triIndex);
                    }
                }

                // 3. 실루엣 Edge 찾기: 앞면 삼각형과 뒷면 삼각형을 연결하는 edge
                foreach (var pair in edgeToTriangles)
                {
                    List<int> tris = pair.Value;

                    // Edge를 공유하는 삼각형이 2개여야 함 (경계 edge는 1개일 수 있음)
                    if (tris.Count == 2)
                    {
                        // 한쪽은 앞면, 한쪽은 뒷면이면 실루엣!
                        if (isFrontFacing[tris[0]] != isFrontFacing[tris[1]])
                        {
                            Edge edge = pair.Key;

                            // 실루엣 edge의 양 끝점을 월드 좌표로 추가
                            Vector3 v0 = meshTransform.TransformPoint(vertices[edge.v0]);
                            Vector3 v1 = meshTransform.TransformPoint(vertices[edge.v1]);

                            silhouetteVertices.Add(v0);
                            silhouetteVertices.Add(v1);
                        }
                    }
                    // 경계 edge (1개 삼각형만 연결)도 실루엣
                    else if (tris.Count == 1 && isFrontFacing[tris[0]])
                    {
                        Edge edge = pair.Key;

                        Vector3 v0 = meshTransform.TransformPoint(vertices[edge.v0]);
                        Vector3 v1 = meshTransform.TransformPoint(vertices[edge.v1]);

                        silhouetteVertices.Add(v0);
                        silhouetteVertices.Add(v1);
                    }
                }
            }
            catch
            {
                // if (showDebug)
                // {
                //     Debug.LogWarning($"Fish3DCollider2D: {meshFilter.name} 메쉬 접근 실패 (Read/Write 필요)");
                // }
            }
        }

        return silhouetteVertices;
    }

    /// <summary>
    /// Edge 구조체: 두 버텍스 인덱스의 쌍 (순서 무관)
    /// </summary>
    private struct Edge : IEquatable<Edge>
    {
        public int v0;
        public int v1;

        public Edge(int a, int b)
        {
            // 작은 인덱스를 v0에
            if (a < b)
            {
                v0 = a;
                v1 = b;
            }
            else
            {
                v0 = b;
                v1 = a;
            }
        }

        public bool Equals(Edge other)
        {
            return v0 == other.v0 && v1 == other.v1;
        }

        public override bool Equals(object obj)
        {
            return obj is Edge other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (v0 << 16) ^ v1;
        }
    }

    /// <summary>
    /// 점 간소화: 그리드 기반 중복 제거로 성능 최적화
    /// </summary>
    private List<Vector2> SimplifyPoints(List<Vector2> points, float gridSize)
    {
        if (points.Count < 100) // 100개 미만이면 간소화 불필요
            return points;

        Dictionary<Vector2Int, Vector2> grid = new Dictionary<Vector2Int, Vector2>();

        foreach (Vector2 point in points)
        {
            // 그리드 좌표 계산
            Vector2Int gridCoord = new Vector2Int(
                Mathf.FloorToInt(point.x / gridSize),
                Mathf.FloorToInt(point.y / gridSize)
            );

            // 같은 그리드 셀에 이미 점이 있으면 평균 사용
            if (grid.ContainsKey(gridCoord))
            {
                grid[gridCoord] = (grid[gridCoord] + point) * 0.5f;
            }
            else
            {
                grid[gridCoord] = point;
            }
        }

        return new List<Vector2>(grid.Values);
    }

    /// <summary>
    /// 3D 버텍스를 카메라 투영 후 2D 평면에 재투영
    /// </summary>
    private List<Vector2> ProjectVerticesToPlane(List<Vector3> worldVertices)
    {
        List<Vector2> projectedPoints = new List<Vector2>();

        foreach (Vector3 worldVertex in worldVertices)
        {
            // 1. 월드 좌표 → 스크린 좌표 (카메라 시점)
            Vector3 screenPoint = cachedCamera.WorldToScreenPoint(worldVertex);

            // 2. 카메라 뒤에 있는 점은 제외
            if (screenPoint.z < 0) continue;

            // 3. 스크린 좌표 → 2D 평면의 월드 좌표
            // projectionPlaneZ 위치에 투영
            Vector3 projectedWorld = cachedCamera.ScreenToWorldPoint(new Vector3(
                screenPoint.x,
                screenPoint.y,
                cachedCamera.WorldToScreenPoint(new Vector3(0, 0, projectionPlaneZ)).z
            ));

            projectedPoints.Add(new Vector2(projectedWorld.x, projectedWorld.y));
        }

        return projectedPoints;
    }

    /// <summary>
    /// Graham Scan 알고리즘으로 Convex Hull 추출
    /// O(n log n) - Gift Wrapping보다 훨씬 빠름
    /// </summary>
    private Vector2[] ExtractConvexHull(List<Vector2> points)
    {
        if (points.Count < 3)
            return points.ToArray();

        // 1. 가장 아래/왼쪽 점 찾기 (기준점)
        Vector2 pivot = points[0];
        foreach (Vector2 p in points)
        {
            if (p.y < pivot.y || (Mathf.Approximately(p.y, pivot.y) && p.x < pivot.x))
            {
                pivot = p;
            }
        }

        // 2. 기준점 기준으로 각도 정렬
        List<Vector2> sorted = new List<Vector2>(points);
        sorted.Sort((a, b) =>
        {
            if (a == pivot) return -1;
            if (b == pivot) return 1;

            // 각도 비교 (cross product 사용)
            float cross = CrossProduct(pivot, a, b);

            if (Mathf.Abs(cross) < 0.0001f) // 같은 각도
            {
                // 가까운 점 우선
                float distA = Vector2.Distance(pivot, a);
                float distB = Vector2.Distance(pivot, b);
                return distA.CompareTo(distB);
            }

            return cross > 0 ? -1 : 1;
        });

        // 3. Graham Scan
        List<Vector2> hull = new List<Vector2>();
        hull.Add(sorted[0]);
        hull.Add(sorted[1]);

        for (int i = 2; i < sorted.Count; i++)
        {
            Vector2 top = hull[hull.Count - 1];

            // 왼쪽 회전이 아니면 제거
            while (hull.Count > 1 && CrossProduct(hull[hull.Count - 2], hull[hull.Count - 1], sorted[i]) <= 0)
            {
                hull.RemoveAt(hull.Count - 1);
            }

            hull.Add(sorted[i]);
        }

        // 4. 점이 너무 많으면 단순화
        if (hull.Count > polygonPoints)
        {
            return SimplifyHull(hull, polygonPoints);
        }

        return hull.ToArray();
    }

    /// <summary>
    /// Cross Product: (b - a) × (c - a)
    /// 양수: c가 ab의 왼쪽 (반시계)
    /// 음수: c가 ab의 오른쪽 (시계)
    /// </summary>
    private float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
    {
        Vector2 ab = b - a;
        Vector2 ac = c - a;
        return ab.x * ac.y - ab.y * ac.x;
    }

    /// <summary>
    /// Hull 점이 너무 많을 때 단순화
    /// </summary>
    private Vector2[] SimplifyHull(List<Vector2> hull, int targetCount)
    {
        if (hull.Count <= targetCount)
            return hull.ToArray();

        // 일정 간격으로 샘플링
        List<Vector2> simplified = new List<Vector2>();
        float step = (float)hull.Count / targetCount;

        for (int i = 0; i < targetCount; i++)
        {
            int index = Mathf.FloorToInt(i * step);
            simplified.Add(hull[index]);
        }

        return simplified.ToArray();
    }

    /// <summary>
    /// Fallback: Renderer Bounds 기반 폴리곤 생성
    /// </summary>
    private Vector2[] GenerateBoundsBasedPolygon()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            // 기본 폴리곤
            return GenerateDefaultPolygon();
        }

        Bounds combinedBounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
        }

        // Bounds 기반 타원형 폴리곤
        int pointCount = Mathf.Clamp(polygonPoints, 8, 32);
        Vector2[] points = new Vector2[pointCount];

        Vector3 worldCenter = combinedBounds.center;
        float   radiusX     = combinedBounds.size.x * 0.5f;
        float   radiusY     = combinedBounds.size.y * 0.5f;

        for (int i = 0; i < pointCount; i++)
        {
            float angle = (360f / pointCount) * i * Mathf.Deg2Rad;

            Vector3 worldPoint = worldCenter + new Vector3(
                Mathf.Cos(angle) * radiusX,
                Mathf.Sin(angle) * radiusY,
                0
            );

            // 월드 → 로컬
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);
            points[i] = new Vector2(localPoint.x, localPoint.y);
        }

        return points;
    }

    private Vector2[] GenerateDefaultPolygon()
    {
        // 기본 1x1 사각형
        return new Vector2[]
        {
            new Vector2(-0.5f, -0.5f),
            new Vector2( 0.5f, -0.5f),
            new Vector2( 0.5f,  0.5f),
            new Vector2(-0.5f,  0.5f)
        };
    }

    // 디버그용 기즈모
    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            return;
        }

        if (cachedCamera == null) return;

        // 투영 평면 표시
        if (showProjection)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                new Vector3(-100, 0, projectionPlaneZ),
                new Vector3(100, 0, projectionPlaneZ)
            );
            Gizmos.DrawLine(
                new Vector3(0, -100, projectionPlaneZ),
                new Vector3(0, 100, projectionPlaneZ)
            );
        }

        // PolygonCollider2D 점들을 월드 좌표로 표시
        if (polygonCollider2D != null && polygonCollider2D.points.Length > 0)
        {
            Gizmos.color = Color.red;

            Vector2[] points = polygonCollider2D.points;
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 worldPoint1 = transform.TransformPoint(new Vector3(points[i].x, points[i].y, 0));
                Vector3 worldPoint2 = transform.TransformPoint(new Vector3(points[(i + 1) % points.Length].x, points[(i + 1) % points.Length].y, 0));

                Gizmos.DrawLine(worldPoint1, worldPoint2);
            }
        }

        // 투영된 모든 점들 표시 (디버그용) - 제거됨
        // if (showProjectedPoints && lastProjectedPoints != null && lastProjectedPoints.Count > 0)
        // {
        //     Gizmos.color = Color.yellow;
        //     foreach (Vector2 point in lastProjectedPoints)
        //     {
        //         Vector3 worldPoint = new Vector3(point.x, point.y, projectionPlaneZ);
        //         Gizmos.DrawSphere(worldPoint, 0.1f);
        //     }
        // }
    }

    // 외부 제어 메서드
    public void SetProjectionPlaneZ(float z)
    {
        projectionPlaneZ     = z;
        autoDetectPlayerZ    = false;
    }

    public void SetTrigger(bool trigger)
    {
        isTrigger = trigger;
        if (boxCollider2D != null)
            boxCollider2D.isTrigger = trigger;
        if (polygonCollider2D != null)
            polygonCollider2D.isTrigger = trigger;
    }
}
