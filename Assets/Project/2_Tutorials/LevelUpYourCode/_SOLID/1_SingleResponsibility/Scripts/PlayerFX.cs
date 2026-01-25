using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.SRP
{
    public class PlayerFX : MonoBehaviour
    {
        [SerializeField] private ParticleSystem m_ParticleSystem;

        // 파티클 시스템 재생 간 쿨다운 시간
        const float k_Cooldown = 1f;

        private float m_TimeToNextPlay = -1f;

        public void PlayEffect()
        {
            // 쿨다운 시간이 지났는지 확인
            if (Time.time < m_TimeToNextPlay)
                return;

            // 파티클 시스템이 null이 아닌 경우 이펙트 재생
            if (m_ParticleSystem != null)
            {
                // 중복 효과를 방지하기 위해 재생 전 파티클 시스템 정지
                m_ParticleSystem.Stop();
                m_ParticleSystem.Play();

                m_TimeToNextPlay = Time.time + k_Cooldown;
            }
        }

    }
}
