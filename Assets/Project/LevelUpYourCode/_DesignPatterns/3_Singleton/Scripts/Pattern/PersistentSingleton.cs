using UnityEngine;

namespace DesignPatterns.Singleton
{
    // ★☆ 보통은 이 코드를 사용하는데, LevelUpYourCode 프로젝트에서
    // 씬을 왔다 갔다 하기 때문에, 씬 전환 시 파괴되는 'Singleton.cs'를 사용함.
    
    /// <summary>
    /// 싱글톤 디자인 패턴의 제네릭이고 영구적인 구현입니다.
    /// 씬 로드/언로드에서 살아남아야 하는 싱글톤에 사용하세요.
    /// </summary>
    /// <typeparam name="T">싱글톤이 되어야 하는 MonoBehaviour의 타입.</typeparam>
    public class PersistentSingleton<T> : MonoBehaviour where T : Component
    {
        private static T s_Instance;

        public static T Instance
        {
            get
            {
                // 싱글톤 인스턴스가 존재하지 않으면, 씬에서 찾기 시도
                if (s_Instance == null)
                {
                    s_Instance = FindFirstObjectByType<T>();

                    // 존재하지 않으면 타입 T를 가진 새 GameObject를 생성
                    if (s_Instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        s_Instance                 = singletonObject.AddComponent<T>();

                        // 타입에 대한 싱글톤 인스턴스 이름 지정
                        singletonObject.name = typeof(T).ToString();

                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return s_Instance;
            }
        }

        protected virtual void Awake()
        {
            if (s_Instance == null)
            {
                s_Instance = this as T;
                DontDestroyOnLoad(this.gameObject);
            }
            else if (s_Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}
