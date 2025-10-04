using System.Collections.Generic;
using UnityEngine;

public class HamiltonTSPVisualizer : MonoBehaviour
{
    private Camera cam;
    
    public int        nodeCount = 10;
    public GameObject nodePrefab;
    public Material   lineMaterial;

    private List<Transform>    nodes       = new List<Transform>();
    private List<GameObject>   nodeObjects = new List<GameObject>();
    private List<LineRenderer> lines       = new List<LineRenderer>();
    
    void Start()
    {
        cam = Camera.main;
    }

    // 화면에 랜덤 노드를 생성하고 최단 비겹침 경로를 라인으로 시각화
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ClearScene();
            GenerateNodes();
            int startIndex = FindCornerClosestNode();
            List<int> path = FindValidPath(startIndex);
            DrawPath(path);
        }
    }
    
    // 이전에 생성된 오브젝트 및 라인을 모두 제거
    void ClearScene()
    {
        foreach (var obj in nodeObjects)
            Destroy(obj);
        nodeObjects.Clear();
        nodes.Clear();
    
        foreach (var line in lines)
            Destroy(line.gameObject);
        lines.Clear();
    }
    
    // 카메라 뷰포트 내 랜덤 위치에 nodeCount 개수만큼 노드를 생성
    void GenerateNodes()
    {
        for (int i = 0; i < nodeCount; i++)
        {
            Vector3 randomPos = new Vector3(Random.Range(0.1f, 0.9f), Random.Range(0.1f, 0.9f), 10f);
            Vector3 worldPos  = cam.ViewportToWorldPoint(randomPos);
            GameObject node   = Instantiate(nodePrefab, worldPos, Quaternion.identity);
            nodes.Add(node.transform);
            nodeObjects.Add(node);
        }
    }
    
    // 화면의 4개 모서리 중 가장 가까운 노드를 시작 노드로 선택
    int FindCornerClosestNode()
    {
        Vector3[] corners =
        {
            new Vector3(0, 0, 10f),
            new Vector3(1, 0, 10f),
            new Vector3(0, 1, 10f),
            new Vector3(1, 1, 10f)
        };
    
        float minDist = float.MaxValue;
        int closestIndex = 0;
    
        foreach (Vector3 corner in corners)
        {
            Vector3 worldCorner = cam.ViewportToWorldPoint(corner);
            for (int i = 0; i < nodes.Count; i++)
            {
                float dist = Vector3.Distance(worldCorner, nodes[i].position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestIndex = i;
                }
            }
        }
    
        return closestIndex;
    }
    
    // 교차 없이 모든 노드를 순회하는 최단 경로를 탐색
    List<int> FindValidPath(int startIndex)
    {
        List<int> bestPath     = new List<int>();
        float     bestDistance = float.MaxValue;
    
        List<int> currentPath = new List<int> { startIndex };
        bool[] visited        = new bool[nodeCount];
        visited[startIndex]   = true;
    
        TryPaths(currentPath, visited, 0f, ref bestPath, ref bestDistance);
    
        return bestPath;
    }
    
    // 백트래킹을 이용해 가능한 모든 경로 중 조건을 만족하는 최단 경로를 찾음
    void TryPaths(List<int> path, bool[] visited, float currentDist, ref List<int> bestPath, ref float bestDist)
    {
        if (path.Count == nodeCount)
        {
            if (currentDist < bestDist)
            {
                bestDist = currentDist;
                bestPath = new List<int>(path);
            }
            return;
        }
    
        for (int i = 0; i < nodeCount; i++)
        {
            if (visited[i]) continue;
    
            int last = path[path.Count - 1];
            float segmentDist = Vector3.Distance(nodes[last].position, nodes[i].position);
    
            if (WouldCrossExistingLines(path, i)) continue;
    
            visited[i] = true;
            path.Add(i);
            TryPaths(path, visited, currentDist + segmentDist, ref bestPath, ref bestDist);
            path.RemoveAt(path.Count - 1);
            visited[i] = false;
        }
    }
    
    // 현재 경로에 새로 연결할 선분이 기존 선분들과 교차하는지 검사
    bool WouldCrossExistingLines(List<int> path, int newIndex)
    {
        if (path.Count < 2) return false;
    
        Vector2 newA = nodes[path[path.Count - 1]].position;
        Vector2 newB = nodes[newIndex].position;
    
        for (int i = 0; i < path.Count - 2; i++)
        {
            Vector2 a = nodes[path[i]].position;
            Vector2 b = nodes[path[i + 1]].position;
    
            if (LinesIntersect(a, b, newA, newB))
                return true;
        }
        return false;
    }
    
    // 두 선분이 겹치는지 기하학적으로 판정
    bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        float d = (a2.x - a1.x) * (b2.y - b1.y) - (a2.y - a1.y) * (b2.x - b1.x);
        if (Mathf.Approximately(d, 0)) return false;
    
        float u = ((b1.x - a1.x) * (b2.y - b1.y) - (b1.y - a1.y) * (b2.x - b1.x)) / d;
        float v = ((b1.x - a1.x) * (a2.y - a1.y) - (b1.y - a1.y) * (a2.x - a1.x)) / d;
    
        return (u > 0 && u < 1 && v > 0 && v < 1);
    }
    
    // 노드에 색상을 입히고 라인 렌더러를 사용해 경로를 시각화
    void DrawPath(List<int> path)
    {
        for (int i = 0; i < path.Count; i++)
        {
            int index = path[i];
            SpriteRenderer sr = nodeObjects[index].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (i == 0)
                    sr.color = Color.green;   // 시작 노드
                else if (i == path.Count - 1)
                    sr.color = Color.red;     // 끝 노드
                else
                    sr.color = Color.yellow;  // 중간 노드
            }
        }
    
        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 start = nodes[path[i]].position;
            Vector3 end   = nodes[path[i + 1]].position;
    
            GameObject lineObj = new GameObject("Line");
            LineRenderer lr    = lineObj.AddComponent<LineRenderer>();
            
            lr.material      = lineMaterial;
            lr.positionCount = 2;
            lr.startWidth    = 0.05f;
            lr.endWidth      = 0.05f;
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            
            lines.Add(lr);
        }
    }
}