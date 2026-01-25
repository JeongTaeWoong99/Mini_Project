using UnityEngine;
using UnityEngine.SceneManagement;

namespace DesignPatterns
{
    /// <summary>
    /// 씬 리셋 기능을 제공하는 클래스
    /// 특정 키를 누르면 프리팹을 다시 생성하여 씬을 초기화합니다.
    /// </summary>
    public class SceneReset : MonoBehaviour
    {
        [Tooltip("씬 리셋 / 프리팹 리로드 키")]
        [SerializeField] private KeyCode     m_KeyCode       = KeyCode.R;
        [SerializeField] private GameObject  m_PrefabToLoad;

        private GameObject m_PrefabInstance;

        void Start()
        {
            InstantiatePrefab();
        }

        /// <summary>
        /// 프리팹을 생성하거나 재생성합니다.
        /// 기존 인스턴스가 있으면 파괴하고 새로 생성합니다.
        /// </summary>
        private void InstantiatePrefab()
        {
            // 기존 인스턴스가 있으면 파괴
            if (m_PrefabInstance != null)
                Destroy((m_PrefabInstance));

            // 프리팹이 할당되어 있으면 생성
            if (m_PrefabToLoad != null)
            {
                m_PrefabInstance = Instantiate(m_PrefabToLoad, Vector3.zero, Quaternion.identity);

                // 생성된 프리팹을 현재 GameObject의 씬으로 이동
                SceneManager.MoveGameObjectToScene(m_PrefabInstance, gameObject.scene);
            }
        }

        void Update()
        {
            // 지정된 키를 누르면 프리팹 재생성
            if (Input.GetKey(m_KeyCode))
            {
                InstantiatePrefab();
            }
        }
    }
}
