using System.Collections;
using UnityEngine;

namespace DesignPatterns.ObjectPool
{
    /// <summary>
    /// 오브젝트 풀에서 사용되는 발사체 클래스
    /// PooledObject 컴포넌트를 필요로 하며, 일정 시간 후 자동으로 풀에 반환된다.
    /// </summary>
    [RequireComponent(typeof(PooledObject))]
    public class ExampleProjectile : MonoBehaviour
    {
        [Header("Projectile Settings - 발사체 설정")]
        [SerializeField] private float timeoutDelay = 3f; // 비활성화까지의 딜레이 (초)

        // 풀에 반환하기 위한 PooledObject 컴포넌트 참조
        private PooledObject pooledObject;

        private void Awake()
        {
            pooledObject = GetComponent<PooledObject>();
        }

        /// <summary>
        /// 발사체 비활성화를 시작한다.
        /// 외부에서 발사 직후 호출하여 일정 시간 후 풀에 반환되도록 한다.
        /// </summary>
        public void Deactivate()
        {
            StartCoroutine(DeactivateRoutine(timeoutDelay));
        }

        /// <summary>
        /// 비활성화 코루틴
        /// 지정된 시간만큼 대기 후 Rigidbody를 초기화하고 풀에 반환한다.
        /// </summary>
        /// <param name="delay">대기 시간 (초)</param>
        private IEnumerator DeactivateRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Rigidbody 속도 초기화
            Rigidbody rBody        = GetComponent<Rigidbody>();
            rBody.linearVelocity   = Vector3.zero;
            rBody.angularVelocity  = Vector3.zero;

            // 풀에 반환
            pooledObject.Release();
            gameObject.SetActive(false);
        }
    }
}
