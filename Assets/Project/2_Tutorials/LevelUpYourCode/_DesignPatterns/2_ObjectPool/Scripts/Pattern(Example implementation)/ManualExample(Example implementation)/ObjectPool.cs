using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.ObjectPool
{
    /// <summary>
    /// 직접 구현한 오브젝트 풀 클래스
    /// Stack 자료구조를 사용하여 오브젝트를 관리한다.
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [Header("Pool Settings - 풀 설정")]
        [SerializeField] private uint         initPoolSize; // 초기 풀 크기 (미리 생성할 오브젝트 수)
        [SerializeField] private PooledObject objectToPool; // 풀링할 프리팹

        // Public Property
        public uint InitPoolSize => initPoolSize;

        // 풀링된 오브젝트를 저장하는 스택
        private Stack<PooledObject> stack;

        private void Start()
        {
            SetupPool();
        }

        /// <summary>
        /// 풀을 초기화한다.
        /// 렉이 눈에 띄지 않는 타이밍 (게임 시작, 로딩 등)에 호출한다.
        /// </summary>
        private void SetupPool()
        {
            // objectToPool 프리팹이 없으면 중단
            if (objectToPool == null)
            {
                return;
            }

            stack = new Stack<PooledObject>();

            // 초기 크기만큼 오브젝트를 미리 생성하여 풀에 추가
            PooledObject instance = null;

            for (int i = 0; i < initPoolSize; i++)
            {
                instance           = Instantiate(objectToPool);
                instance.Pool      = this;
                instance.gameObject.SetActive(false);
                stack.Push(instance);
            }
        }

        /// <summary>
        /// 풀에서 오브젝트를 꺼내온다.
        /// 풀이 비어있으면 새로 생성하고, 있으면 스택에서 꺼낸다.
        /// </summary>
        /// <returns>활성화된 PooledObject</returns>
        public PooledObject GetPooledObject()
        {
            // objectToPool 프리팹이 없으면 null 반환
            if (objectToPool == null)
            {
                return null;
            }

            // 풀이 비어있으면 새로운 오브젝트를 생성
            if (stack.Count == 0)
            {
                PooledObject newInstance = Instantiate(objectToPool);
                newInstance.Pool         = this;
                return newInstance;
            }

            // 풀에 오브젝트가 있으면 스택에서 꺼내서 활성화
            PooledObject nextInstance = stack.Pop();
            nextInstance.gameObject.SetActive(true);
            return nextInstance;
        }

        /// <summary>
        /// 오브젝트를 풀에 반환한다.
        /// 오브젝트를 비활성화하고 스택에 다시 넣는다.
        /// </summary>
        /// <param name="pooledObject">반환할 오브젝트</param>
        public void ReturnToPool(PooledObject pooledObject)
        {
            stack.Push(pooledObject);
            pooledObject.gameObject.SetActive(false);
        }
    }
}
