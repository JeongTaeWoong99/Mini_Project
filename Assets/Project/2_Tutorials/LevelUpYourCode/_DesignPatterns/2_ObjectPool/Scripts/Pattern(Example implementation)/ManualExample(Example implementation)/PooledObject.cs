using UnityEngine;

namespace DesignPatterns.ObjectPool
{
    /// <summary>
    /// 오브젝트 풀에서 관리되는 오브젝트의 기본 클래스
    /// 풀에 대한 참조를 가지고 있어 스스로를 풀에 반환할 수 있다.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        // 이 오브젝트가 속한 오브젝트 풀 참조
        private ObjectPool pool;

        /// <summary>
        /// 오브젝트 풀 참조 프로퍼티
        /// </summary>
        public ObjectPool Pool { get => pool; set => pool = value; }

        /// <summary>
        /// 이 오브젝트를 풀에 반환한다.
        /// 외부에서 호출하여 오브젝트를 비활성화하고 풀로 돌려보낸다.
        /// </summary>
        public void Release()
        {
            pool.ReturnToPool(this);
        }
    }
}
