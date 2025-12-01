using DesignPatterns.LSP;
using Unity.Mathematics;
using UnityEngine;


namespace DesignPatterns.ISP
{
    /// <summary>
    /// 폭발할 수 있고 죽을 때 이펙트를 생성하는 대체 타입의 타겟입니다. 여기서는
    /// 기본 Target을 상속하고 IExplodable 인터페이스를 추가합니다.
    /// </summary>
    public class ExplodableTarget : Target, IExplodable
    {
        [Tooltip("폭발 시 인스턴스화할 이펙트")]
        [SerializeField] GameObject m_ExplosionPrefab;

        protected override void Die()
        {
            base.Die();
            Explode();
        }

        public void Explode()
        {
            if (m_ExplosionPrefab)
            {
                GameObject instance = Instantiate(m_ExplosionPrefab, transform.position, quaternion.identity);
            }

            // 커스텀 폭발 로직을 여기에 추가
        }
    }
}