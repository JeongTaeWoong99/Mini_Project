using UnityEngine;

namespace DesignPatterns.ISP
{
    /// <summary>
    /// 발사체가 표면에 충돌할 때의 이펙트 트리거를 구현합니다. 인터페이스 분리 원칙은
    /// 더 작고 클라이언트별 인터페이스를 권장합니다.
    /// </summary>
    public class HitEffect : MonoBehaviour, IEffectTrigger
    {
        [SerializeField] private ParticleSystem m_ParticleSystem;

        public void TriggerEffect(Vector3 position)
        {
            // 파티클 시스템이 null이 아니면 이펙트 재생
            if (m_ParticleSystem != null)
            {
                m_ParticleSystem.transform.position = position;
                // 겹치는 이펙트를 방지하기 위해 다시 재생하기 전에 파티클 시스템 정지
                m_ParticleSystem.Stop();
                m_ParticleSystem.Play();
            }
        }
    }
}
