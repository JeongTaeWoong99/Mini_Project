using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

namespace DesignPatterns.ISP
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private int m_DamageValue = 5;

        // 지연 후 비활성화
        [SerializeField] private float m_Lifetime = 3f;
        private IObjectPool<Projectile> m_ObjectPool;
        private Rigidbody m_Rigidbody;
        private float m_MuzzleVelocity;

        // 발사체에게 ObjectPool에 대한 참조를 제공하는 공용 프로퍼티
        public IObjectPool<Projectile> ObjectPool
        {
            set => m_ObjectPool = value;
        }

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            CheckCollisionInterfaces(collision);
            DeactivateProjectile();
        }

        private void DeactivateProjectile()
        {
            m_Rigidbody.linearVelocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            m_ObjectPool.Release(this);
        }

        private void CheckCollisionInterfaces(Collision collision)
        {
            // 첫 번째 접촉점 가져오기
            ContactPoint contactPoint = collision.GetContact(0);

            // 표면 밖으로 이동하기 위한 약간의 오프셋
            float pushDistance = 0.1f;
            Vector3 offsetPosition = contactPoint.point + contactPoint.normal * pushDistance;

            var monoBehaviours = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (var monoBehaviour in monoBehaviours)
            {
                HandleDamageableInterface(monoBehaviour);
                HandleEffectTriggerInterface(monoBehaviour, offsetPosition);
            }
        }

        private void HandleDamageableInterface(MonoBehaviour monoBehaviour)
        {
            if (monoBehaviour is IDamageable damageable)
            {
                damageable.TakeDamage(m_DamageValue);
            }
        }

        private void HandleEffectTriggerInterface(MonoBehaviour monoBehaviour, Vector3 position)
        {
            if (monoBehaviour is IEffectTrigger effectTrigger)
            {
                effectTrigger.TriggerEffect(position);
            }
        }

        public void Initialize(IObjectPool<Projectile> pool, float velocity)
        {
            ObjectPool = pool;

            // Rigidbody 또는 기타 필요한 컴포넌트를 여기에 캐시
            m_MuzzleVelocity = velocity;
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        public void Launch(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);

            // 캐시된 Rigidbody를 사용하여 힘 적용
            m_Rigidbody.AddForce(transform.forward * m_MuzzleVelocity, ForceMode.Acceleration);

            StartCoroutine(LifetimeCoroutine());
        }
        
        private IEnumerator LifetimeCoroutine()
        {
            yield return new WaitForSeconds(m_Lifetime);
            DeactivateProjectile();
        }
    }
}