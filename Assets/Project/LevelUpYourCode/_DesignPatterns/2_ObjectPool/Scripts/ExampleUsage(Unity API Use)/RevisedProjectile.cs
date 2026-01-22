using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace DesignPatterns.ObjectPool
{
    /// <summary>
    /// Unity 2021+ 의 UnityEngine.Pool을 사용하는 발사체 클래스
    /// IObjectPool 인터페이스를 통해 풀에 반환된다.
    /// </summary>
    public class RevisedProjectile : MonoBehaviour
    {
        [Header("Projectile Settings - 발사체 설정")]
        [SerializeField] private float timeoutDelay = 3f; // 비활성화까지의 딜레이 (초)

        // Unity의 IObjectPool 인터페이스 참조
        private IObjectPool<RevisedProjectile> objectPool;

        /// <summary>
        /// 발사체가 자신이 속한 오브젝트 풀을 참조할 수 있도록 설정
        /// RevisedGun에서 발사체 생성 시 설정한다.
        /// </summary>
        public IObjectPool<RevisedProjectile> ObjectPool { set => objectPool = value; }

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
            Rigidbody rBody       = GetComponent<Rigidbody>();
            rBody.linearVelocity  = Vector3.zero;
            rBody.angularVelocity = Vector3.zero;

            // Unity Pool API의 Release 메서드로 풀에 반환
            objectPool.Release(this);
        }
    }
}
