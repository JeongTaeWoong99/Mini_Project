using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace DesignPatterns.Singleton
{
    // ★☆ LevelUpYourCode 프로젝트에서
    // 씬을 왔다 갔다 하기 때문에, 씬 전환 시 파괴되는 'Singleton.cs'를 사용함.
    // 보통은 'PersistentSingleton.cs' 사용
    
    /// <summary>
    /// MonoBehaviour 타입을 위한 싱글톤 디자인 패턴의 제네릭 구현을 제공합니다.
    /// 애플리케이션 내에서 언제든지 하나의 싱글톤 인스턴스만 존재하도록 보장합니다.
    /// 접근 시 인스턴스를 찾지 못하면, 이 스크립트가 인스턴스를 생성합니다.
    /// </summary>
    /// <typeparam name="T">싱글톤이 되어야 하는 MonoBehaviour의 타입.</typeparam>
    public class Singleton<T> : MonoBehaviour where T : Component
    {
        [Tooltip("중복 인스턴스 제거를 명시적으로 호출할 때까지 지연시킵니다 (데모 용도 전용).")]
        [SerializeField] private bool m_DelayDuplicateRemoval;

        private static T s_Instance;

        public static T Instance
        {
            get
            {
                if (s_Instance == null)
                {
                    s_Instance = (T)FindFirstObjectByType(typeof(T));

                    if (s_Instance == null)
                    {
                        SetupInstance();
                    }
                    else
                    {
                        string typeName = typeof(T).Name;

                        Debug.Log("[Singleton] " + typeName + " instance already created: " +
                                  s_Instance.gameObject.name);
                    }
                }

                return s_Instance;
            }
        }

        public virtual void Awake()
        {
            // 데모 목적으로, 이 플래그는 중복 제거를 지연시킬 수 있습니다
            if (!m_DelayDuplicateRemoval)
                RemoveDuplicates();
        }

        private void OnEnable()
        {
            // 현재 씬을 언로드할 때 싱글 인스턴스를 정리
            SceneManager.sceneUnloaded += SceneManager_SceneUnloaded;
        }

        private void OnDisable()
        {
            if (s_Instance == this as T)
            {
                SceneManager.sceneUnloaded -= SceneManager_SceneUnloaded;
            }
        }

        private static void SetupInstance()
        {
            // 지연 인스턴스화
            s_Instance = (T)FindFirstObjectByType(typeof(T));

            if (s_Instance == null)
            {
                GameObject gameObj = new GameObject();
                gameObj.name = typeof(T).Name;

                s_Instance = gameObj.AddComponent<T>();
                DontDestroyOnLoad(gameObj);
            }
        }

        public void RemoveDuplicates()
        {
            if (s_Instance == null)
            {
                s_Instance = this as T;

                // 지속성을 위해 DontDestroyOnLoad를 사용하되 수동으로 정리/폐기
                //DontDestroyOnLoad(gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }

        // 이벤트 처리 메서드

        // 씬 언로드 시 싱글톤 파괴 (데모 용도 전용)
        private void SceneManager_SceneUnloaded(Scene scene)
        {
            if (s_Instance != null)
                Destroy(s_Instance.gameObject);

            s_Instance = null;
        }
    }
}