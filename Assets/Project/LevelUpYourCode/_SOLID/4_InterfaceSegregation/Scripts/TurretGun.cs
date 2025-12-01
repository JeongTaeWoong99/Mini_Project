using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Events;
using DesignPatterns.Utilities;

namespace DesignPatterns.ISP
{
    /// <summary>
    /// ObjectPool에서 발사체를 발사하는 총을 나타냅니다.
    /// </summary>
    public class TurretGun : MonoBehaviour
    {
        [Tooltip("발사할 프리팹")]
        [SerializeField]
        private Projectile m_ProjectilePrefab;

        [Tooltip("발사체 속도")]
        [SerializeField]
        private float m_MuzzleVelocity = 700f;

        [Tooltip("발사가 나타나는 총의 끝점")]
        [SerializeField]
        private Transform m_MuzzlePosition;

        [Tooltip("발사 간격 / 작을수록 연사 속도가 높아짐")]
        [SerializeField]
        private float m_CooldownWindow = 0.1f;

        [Tooltip("이미 풀에 있는 아이템을 해제하려고 할 때 오류 발생")]
        [SerializeField] private bool m_CollectionCheck = true;

        [Tooltip("기본 풀 크기")]
        [SerializeField] private int m_DefaultCapacity = 20;

        [Tooltip("풀이 확장될 수 있는 최대 크기")]
        [SerializeField] private int m_MaxSize = 100;

        [SerializeField] private UnityEvent m_GunFired;
        [SerializeField] ScreenDeadZone m_ScreenDeadZone;
        
        private IObjectPool<Projectile> objectPool;
        private float nextTimeToShoot;

        private void Awake()
        {
            // 오브젝트 풀 생성
            objectPool = new ObjectPool<Projectile>(CreateProjectile, OnGetFromPool, OnReleaseToPool,
                OnDestroyPooledObject, m_CollectionCheck, m_DefaultCapacity, m_MaxSize);
        }

        private Projectile CreateProjectile()
        {
            Projectile projectileInstance = Instantiate(m_ProjectilePrefab);
            projectileInstance.Initialize(objectPool, m_MuzzleVelocity);
            return projectileInstance;
        }

        private void OnReleaseToPool(Projectile pooledObject)
        {
            pooledObject.gameObject.SetActive(false);
        }

        private void OnGetFromPool(Projectile pooledObject)
        {
            pooledObject.gameObject.SetActive(true);
        }

        private void OnDestroyPooledObject(Projectile pooledObject)
        {
            Destroy(pooledObject.gameObject);
        }

        private void FixedUpdate()
        {
            if (m_ScreenDeadZone.IsMouseInDeadZone())
                return;

            // Fire1 버튼이 눌리고 쿨다운 시간이 지났을 때 발사
            if (Input.GetButton("Fire1") && Time.fixedTime >= nextTimeToShoot)
            {
                Shoot();
            }
        }

        private void Shoot()
        {
            // 오브젝트 풀에서 발사체 가져오기
            Projectile bulletObject = objectPool.Get();
            // 발사체 발사
            bulletObject.Launch(m_MuzzlePosition.position, m_MuzzlePosition.rotation);
            // 다음 발사 시간 설정
            nextTimeToShoot = Time.fixedTime + m_CooldownWindow;

            m_GunFired.Invoke();
        }
    }
}