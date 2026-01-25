using System.Collections.Generic;
using UnityEngine;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// 상어 스포너
    /// - 게임 시작 시 플레이어 주변 원형 범위에 상어 생성
    /// - 항상 지정된 수만큼 상어 유지 (죽으면 새로 생성)
    /// </summary>
    public class SharkSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject sharkPrefab       = null;  // 상어 프리팹
        [SerializeField] private int        maxSharkCount     = 3;     // 유지할 상어 수
        [SerializeField] private Transform  spawnCenter       = null;  // 생성 중심점 (null이면 이 오브젝트)

        [Header("Spawn Range Settings")]
        [SerializeField] private float spawnRadius       = 15f;  // 생성 반경 (3D 원형 범위)
        [SerializeField] private float spawnMinDistance  = 5f;   // 중심점으로부터 최소 거리
        [SerializeField] private float spawnHeight       = 0f;   // 생성 높이 (Y 오프셋)
        [SerializeField] private float spawnZRange       = 0.5f; // Z축 범위 (-spawnZRange ~ +spawnZRange)

        [Header("Auto Spawn Settings")]
        [SerializeField] private bool  spawnOnStart      = true;  // 게임 시작 시 자동 생성
        [SerializeField] private float spawnCheckInterval = 1f;   // 상어 수 체크 간격 (초)

        [Header("Debug Settings")]
        [SerializeField] private bool  showDebug         = false; // 디버그 로그 표시
        [SerializeField] private bool  showGizmos        = true;  // 기즈모 표시
        [SerializeField] private Color gizmoColor        = Color.yellow; // 생성 범위 기즈모 색상

        // Private variables
        private List<GameObject> spawnedSharks = new List<GameObject>(); // 생성된 상어 리스트
        private float            nextCheckTime = 0f;                     // 다음 체크 시간

        private void Start()
        {
            // 생성 중심점이 없으면 RocketController.instance 사용
            if (spawnCenter == null)
            {
                if (RocketController.instance != null)
                {
                    spawnCenter = RocketController.instance.transform;
                }
                else
                {
                    Debug.LogWarning("SharkSpawner : RocketController.instance가 null입니다. 이 오브젝트를 중심점으로 사용합니다.");
                    spawnCenter = transform;
                }
            }

            // 게임 시작 시 자동 생성
            if (spawnOnStart)
            {
                SpawnInitialSharks();
            }
        }

        private void Update()
        {
            // 주기적으로 상어 수 체크 및 보충
            if (Time.time >= nextCheckTime)
            {
                CheckAndSpawnSharks();
                nextCheckTime = Time.time + spawnCheckInterval;
            }
        }

        /// <summary>
        /// 초기 상어 생성 (maxSharkCount만큼)
        /// </summary>
        private void SpawnInitialSharks()
        {
            if (sharkPrefab == null)
            {
                Debug.LogError("SharkSpawner : 상어 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 기존 상어 리스트 정리
            spawnedSharks.Clear();

            // maxSharkCount만큼 생성
            for (int i = 0; i < maxSharkCount; i++)
            {
                SpawnShark();
            }

            if (showDebug)
            {
                Debug.Log($"SharkSpawner : 초기 상어 {maxSharkCount}마리 생성 완료");
            }
        }

        /// <summary>
        /// 상어 수 체크 및 부족하면 생성
        /// </summary>
        private void CheckAndSpawnSharks()
        {
            // null 상어 제거 (파괴된 상어)
            spawnedSharks.RemoveAll(shark => shark == null);

            // 부족한 상어 수만큼 생성
            int needSpawn = maxSharkCount - spawnedSharks.Count;
            for (int i = 0; i < needSpawn; i++)
            {
                SpawnShark();
            }
        }

        /// <summary>
        /// 상어 1마리 생성
        /// </summary>
        private void SpawnShark()
        {
            if (sharkPrefab == null)
            {
                Debug.LogError("SharkSpawner : 상어 프리팹이 설정되지 않았습니다!");
                return;
            }

            // 생성 위치 계산 (원형 범위 내 랜덤)
            Vector3 spawnPos = GetRandomSpawnPosition();

            // 상어 생성
            GameObject shark = Instantiate(sharkPrefab, spawnPos, Quaternion.identity);
            spawnedSharks.Add(shark);

            // SharkManager에 등록
            if (SharkManager.Instance != null)
            {
                SharkManager.Instance.AddShark(shark.transform);
            }

            if (showDebug)
            {
                Debug.Log($"SharkSpawner : 상어 생성 완료! 위치 : {spawnPos}, 현재 상어 수 : {spawnedSharks.Count}/{maxSharkCount}");
            }
        }

        /// <summary>
        /// 랜덤 생성 위치 계산
        /// </summary>
        private Vector3 GetRandomSpawnPosition()
        {
            Vector3 centerPos = spawnCenter.position;

            // 원형 범위 내 랜덤 위치 (spawnMinDistance ~ spawnRadius)
            float   angle     = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float   distance  = Random.Range(spawnMinDistance, spawnRadius);
            Vector3 offset    = new Vector3(
                Mathf.Cos(angle) * distance,
                spawnHeight,
                Mathf.Sin(angle) * distance
            );

            // Z축 랜덤 오프셋 추가
            offset.z += Random.Range(-spawnZRange, spawnZRange);

            return centerPos + offset;
        }

        /// <summary>
        /// 수동으로 상어 생성 (외부 호출용)
        /// </summary>
        public void ManualSpawnShark()
        {
            SpawnShark();
        }

        /// <summary>
        /// 모든 생성된 상어 제거
        /// </summary>
        public void ClearAllSharks()
        {
            foreach (GameObject shark in spawnedSharks)
            {
                if (shark != null)
                {
                    // SharkManager에서 제거
                    if (SharkManager.Instance != null)
                    {
                        SharkManager.Instance.RemoveShark(shark.transform);
                    }

                    Destroy(shark);
                }
            }

            spawnedSharks.Clear();

            if (showDebug)
            {
                Debug.Log("SharkSpawner : 모든 상어 제거 완료");
            }
        }

        /// <summary>
        /// 현재 생성된 상어 수 반환
        /// </summary>
        public int GetCurrentSharkCount()
        {
            spawnedSharks.RemoveAll(shark => shark == null);
            return spawnedSharks.Count;
        }

        // 기즈모 그리기
        private void OnDrawGizmos()
        {
            if (!showGizmos) return;

            // 중심점 결정 : spawnCenter > RocketController.instance > transform
            Vector3 center;
            if (spawnCenter != null)
            {
                center = spawnCenter.position;
            }
            else if (RocketController.instance != null)
            {
                center = RocketController.instance.transform.position;
            }
            else
            {
                center = transform.position;
            }

            // 최소 거리 원
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            DrawCircle(center, spawnMinDistance, 32);

            // 최대 거리 원
            Gizmos.color = gizmoColor;
            DrawCircle(center, spawnRadius, 64);

            // 중심점 표시
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(center, 0.5f);
        }

        /// <summary>
        /// 원형 기즈모 그리기 헬퍼 메서드
        /// </summary>
        private void DrawCircle(Vector3 center, float radius, int segments)
        {
            float angleStep = 360f / segments;
            Vector3 prevPoint = center + new Vector3(radius, 0, 0);

            for (int i = 1; i <= segments; i++)
            {
                float   angle     = i * angleStep * Mathf.Deg2Rad;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }
}
