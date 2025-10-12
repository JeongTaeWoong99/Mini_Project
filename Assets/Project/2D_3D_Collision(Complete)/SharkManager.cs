using UnityEngine;
using System.Collections.Generic;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// 씬의 모든 상어를 관리하고, 활성 상어(Z값 -5 ~ 5 범위) 리스트를 제공하는 매니저
    /// </summary>
    public class SharkManager : MonoBehaviour
    {
        [Header("Shark Detection Settings")]
        [SerializeField] private float activeZMin = -5f;  // 활성 Z값 최소
        [SerializeField] private float activeZMax = 5f;   // 활성 Z값 최대

        [Header("Auto Find Settings")]
        [SerializeField] private bool   autoFindSharks = true;    // 시작 시 자동으로 상어 찾기
        [SerializeField] private string sharkTag       = "Shark"; // 상어 태그 (옵션)

        [Header("Debug Settings")]
        [SerializeField] private bool  showDebug     = false;            // 디버그 표시
        [SerializeField] private Color activeColor   = Color.green;   // 활성 상어 기즈모 색상
        [SerializeField] private Color inactiveColor = Color.red;     // 비활성 상어 기즈모 색상

        // Private variables
        private List<Transform> allSharks    = new List<Transform>();    // 모든 상어 리스트
        private List<Transform> activeSharks = new List<Transform>();    // 활성 상어 리스트 (Z값 -5 ~ 5)

        // Singleton pattern (옵션)
        private static SharkManager instance;
        public static SharkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<SharkManager>();
                }
                return instance;
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Unity Lifecycle Methods
        // ═══════════════════════════════════════════════════════════

        void Awake()
        {
            // Singleton 설정
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("SharkManager : 씬에 여러 개의 SharkManager가 존재합니다. 이 인스턴스를 파괴합니다.");
                Destroy(gameObject);
                return;
            }
        }

        void Start()
        {
            // 자동으로 상어 찾기
            if (autoFindSharks)
            {
                FindAllSharks();
            }
        }

        void Update()
        {
            // 매 프레임마다 활성 상어 리스트 갱신
            UpdateActiveSharks();
        }

        // ═══════════════════════════════════════════════════════════
        // Shark Management
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 씬의 모든 상어를 찾아서 리스트에 추가
        /// </summary>
        public void FindAllSharks()
        {
            allSharks.Clear();
            
            // 태그로 찾기 (옵션 - sharkTag가 설정된 경우)
            if (!string.IsNullOrEmpty(sharkTag))
            {
                GameObject[] sharkObjects = GameObject.FindGameObjectsWithTag(sharkTag);
                foreach (GameObject shark in sharkObjects)
                {
                    if (!allSharks.Contains(shark.transform))
                    {
                        allSharks.Add(shark.transform);
                    }
                }
            }

            if (showDebug)
            {
                Debug.Log($"[SharkManager] 총 {allSharks.Count}마리의 상어를 찾았습니다.");
            }
        }

        /// <summary>
        /// 활성 상어 리스트 갱신 (Z값 -5 ~ 5 범위)
        /// </summary>
        void UpdateActiveSharks()
        {
            activeSharks.Clear();

            foreach (Transform shark in allSharks)
            {
                if (shark == null) continue;

                // Z값이 activeZMin ~ activeZMax 범위에 있는지 확인
                float zPos = shark.position.z;
                if (zPos >= activeZMin && zPos <= activeZMax)
                {
                    activeSharks.Add(shark);
                }
            }
        }

        /// <summary>
        /// 수동으로 상어 추가
        /// </summary>
        public void AddShark(Transform shark)
        {
            if (shark != null && !allSharks.Contains(shark))
            {
                allSharks.Add(shark);

                if (showDebug)
                {
                    Debug.Log($"[SharkManager] 상어 추가 : {shark.name}");
                }
            }
        }

        /// <summary>
        /// 수동으로 상어 제거
        /// </summary>
        public void RemoveShark(Transform shark)
        {
            if (allSharks.Contains(shark))
            {
                allSharks.Remove(shark);

                if (showDebug)
                {
                    Debug.Log($"[SharkManager] 상어 제거 : {shark.name}");
                }
            }
        }

        // ═══════════════════════════════════════════════════════════
        // Public Getters
        // ═══════════════════════════════════════════════════════════

        /// <summary>
        /// 모든 상어 리스트 반환 (읽기 전용)
        /// </summary>
        public List<Transform> GetAllSharks()
        {
            return new List<Transform>(allSharks);
        }

        /// <summary>
        /// 활성 상어 리스트 반환 (Z값 -5 ~ 5 범위, 읽기 전용)
        /// </summary>
        public List<Transform> GetActiveSharks()
        {
            return new List<Transform>(activeSharks);
        }

        /// <summary>
        /// 특정 위치에서 가장 가까운 활성 상어 반환
        /// </summary>
        public Transform GetClosestActiveShark(Vector3 position)
        {
            if (activeSharks.Count == 0) return null;

            Transform closest       = null;
            float     minDistance = Mathf.Infinity;

            foreach (Transform shark in activeSharks)
            {
                if (shark == null) continue;

                float distance = Vector3.Distance(position, shark.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest     = shark;
                }
            }

            return closest;
        }

        /// <summary>
        /// 활성 상어 수 반환
        /// </summary>
        public int GetActiveSharkCount()
        {
            return activeSharks.Count;
        }

        // ═══════════════════════════════════════════════════════════
        // Debug Visualization
        // ═══════════════════════════════════════════════════════════

        void OnDrawGizmos()
        {
            if (!showDebug) return;

            // 활성 Z 범위 시각화 (평면 2개)
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);

            // Z = activeZMin 평면
            Vector3 minPlaneCenter = new Vector3(0, 0, activeZMin);
            Gizmos.DrawCube(minPlaneCenter, new Vector3(100f, 100f, 0.1f));

            // Z = activeZMax 평면
            Vector3 maxPlaneCenter = new Vector3(0, 0, activeZMax);
            Gizmos.DrawCube(maxPlaneCenter, new Vector3(100f, 100f, 0.1f));

            // 각 상어의 상태 표시
            foreach (Transform shark in allSharks)
            {
                if (shark == null) continue;

                // 활성 여부 확인
                bool isActive = activeSharks.Contains(shark);
                Gizmos.color = isActive ? activeColor : inactiveColor;

                // 상어 위치에 구 그리기
                Gizmos.DrawWireSphere(shark.position, 1f);

                // 라벨 표시 (에디터 전용)
                #if UNITY_EDITOR
                string status = isActive ? "[ACTIVE]" : "[INACTIVE]";
                UnityEditor.Handles.Label(
                    shark.position + Vector3.up * 2f,
                    $"{shark.name}\n{status}\nZ : {shark.position.z:F2}"
                );
                #endif
            }
        }
    }
} // namespace MiniGame2D3DCollision
