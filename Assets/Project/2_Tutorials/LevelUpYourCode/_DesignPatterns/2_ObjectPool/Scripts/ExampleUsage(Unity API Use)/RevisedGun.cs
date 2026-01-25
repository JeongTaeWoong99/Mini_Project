using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Events;

namespace DesignPatterns.ObjectPool
{
    /// <summary>
    /// Unity 2021+ 의 UnityEngine.Pool.ObjectPool을 사용하는 총기 클래스
    /// 콜백 기반의 풀링 시스템으로 더 유연하고 안전한 오브젝트 관리가 가능하다.
    /// </summary>
    public class RevisedGun : MonoBehaviour
    {
        [Header("Gun Settings - 총기 설정")]
        [Tooltip("발사할 프리팹")]
        [SerializeField] private RevisedProjectile projectilePrefab;
        [Tooltip("발사체에 가해지는 힘")]
        [SerializeField] private float             muzzleVelocity = 1500f;
        [Tooltip("발사체가 생성되는 총구 위치")]
        [SerializeField] private Transform         muzzlePosition;
        [Tooltip("발사 간격 (작을수록 연사 속도 증가)")]
        [SerializeField] private float             cooldownWindow = 0.1f;

        [Header("Events - 이벤트")]
        [SerializeField] private UnityEvent m_GunFired; // 발사 시 호출되는 이벤트

        [Header("Pool Settings - 풀 설정")]
        [Tooltip("이미 풀에 있는 오브젝트를 다시 반환하면 예외 발생")]
        [SerializeField] private bool collectionCheck  = true;
        [Tooltip("풀의 초기 용량")]
        [SerializeField] private int  defaultCapacity  = 20;
        [Tooltip("풀의 최대 크기 (초과 시 오브젝트 파괴)")]
        [SerializeField] private int  maxSize          = 100;

        // Unity 2021+ 에서 제공하는 스택 기반 ObjectPool
        private IObjectPool<RevisedProjectile> objectPool;

        // 다음 발사 가능 시간
        private float nextTimeToShoot;

        private void Awake()
        {
            // ObjectPool 생성 - 4개의 콜백 함수와 옵션을 전달
            objectPool = new ObjectPool<RevisedProjectile>
            (
                CreateProjectile,        // 생성 콜백
                OnGetFromPool,           // 풀에서 꺼낼 때 콜백
                OnReleaseToPool,         // 풀에 반환할 때 콜백
                OnDestroyPooledObject,   // 오브젝트 파괴 콜백
                collectionCheck,         // 중복 반환 체크
                defaultCapacity,         // 초기 용량
                maxSize                  // 최대 크기
            );
        }

        /// <summary>
        /// 오브젝트 생성 콜백
        /// 풀에 오브젝트가 부족할 때 호출되어 새 오브젝트를 생성한다.
        /// </summary>
        private RevisedProjectile CreateProjectile()
        {
            RevisedProjectile projectileInstance = Instantiate(projectilePrefab);
            projectileInstance.ObjectPool        = objectPool; // 풀 참조 설정
            return projectileInstance;
        }

        /// <summary>
        /// 풀에서 오브젝트를 꺼낼 때 호출되는 콜백
        /// 오브젝트를 활성화한다.
        /// </summary>
        /// <param name="pooledObject">풀에서 꺼낸 오브젝트</param>
        private void OnGetFromPool(RevisedProjectile pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        /// <summary>
        /// 풀에 오브젝트를 반환할 때 호출되는 콜백
        /// 오브젝트를 비활성화한다.
        /// </summary>
        /// <param name="pooledObject">반환할 오브젝트</param>
        private void OnReleaseToPool(RevisedProjectile pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }

        /// <summary>
        /// 풀 최대 크기 초과 시 오브젝트 파괴 콜백
        /// maxSize를 초과하면 오브젝트를 파괴한다.
        /// </summary>
        /// <param name="pooledObject">파괴할 오브젝트</param>
        private void OnDestroyPooledObject(RevisedProjectile pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }

        private void FixedUpdate()
        {
            // 쿨다운이 지났고 Fire1 버튼을 누르면 발사
            if (Input.GetButton("Fire1") && Time.time > nextTimeToShoot && objectPool != null)
            {
                Shoot();
            }
        }

        /// <summary>
        /// 발사체를 발사한다.
        /// 풀에서 오브젝트를 가져와 위치 설정 후 힘을 가한다.
        /// </summary>
        private void Shoot()
        {
            // Instantiate 대신 풀에서 오브젝트를 가져옴
            RevisedProjectile bulletObject = objectPool.Get();

            if (bulletObject == null)
                return;

            // 총구 위치와 회전에 맞춤
            bulletObject.transform.SetPositionAndRotation(muzzlePosition.position, muzzlePosition.rotation);

            // 발사체를 앞으로 밀어냄
            bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.forward * muzzleVelocity, ForceMode.Acceleration);

            // 일정 시간 후 풀에 반환되도록 Deactivate 호출
            bulletObject.Deactivate();

            // 다음 발사 시간 설정 (쿨다운)
            nextTimeToShoot = Time.time + cooldownWindow;

            // 발사 이벤트 호출
            m_GunFired.Invoke();
        }
    }
}
