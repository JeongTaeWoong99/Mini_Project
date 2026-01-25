using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DesignPatterns.SRP;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// PlayerMovement에 속도 증가 또는 감소 효과를 적용하는 클래스
    /// 일정 시간 동안 이동 속도를 배율로 조절합니다.
    /// </summary>
    [RequireComponent(typeof(PlayerMovement))]
    public class SpeedModifier : MonoBehaviour
    {
        [SerializeField] private PlayerMovement  m_PlayerMovement;

        [Header("시각 효과")]
        [Tooltip("수정자가 활성화되어 있을 때 재생할 파티클")]
        [SerializeField] private ParticleSystem  m_ParticleSystem;

        private Coroutine m_SpeedBoostCoroutine;
        private bool      m_IsModifierActive = false;
        private float     m_Duration         = 0f;

        private void Start()
        {
            if (m_PlayerMovement == null)
            {
                m_PlayerMovement = GetComponent<PlayerMovement>();
            }
        }

        /// <summary>
        /// 속도를 수정합니다.
        /// </summary>
        /// <param name="speedMultiplier">속도 배율 (2.0이면 2배 빠르게, 0.5면 절반 속도)</param>
        /// <param name="duration">지속 시간 (초)</param>
        public void ModifySpeed(float speedMultiplier, float duration)
        {
            // 수정자가 이미 활성화되어 있으면, 코루틴을 중지하고 새로 시작
            if (m_IsModifierActive)
            {
                StopCoroutine(m_SpeedBoostCoroutine);
                m_SpeedBoostCoroutine =
                    StartCoroutine(ApplySpeedModifier(speedMultiplier, m_Duration + duration - Time.time));
            }
            else
            {
                m_Duration            = Time.time + duration;
                m_SpeedBoostCoroutine = StartCoroutine(ApplySpeedModifier(speedMultiplier, duration));
            }
        }

        /// <summary>
        /// 속도 수정자를 적용하고 지정된 시간 후에 제거합니다.
        /// </summary>
        private IEnumerator ApplySpeedModifier(float speedMultiplier, float duration)
        {
            // 아직 활성화되지 않았다면 속도 수정자 적용
            if (!m_IsModifierActive)
            {
                m_PlayerMovement.SpeedMultiplier *= speedMultiplier;
                m_IsModifierActive                = true;

                if (m_ParticleSystem != null)
                {
                    m_ParticleSystem.Play();
                }
            }

            yield return new WaitForSeconds(duration);

            // 지속 시간이 지나면 속도 수정자 제거
            if (Time.time >= m_Duration)
            {
                m_PlayerMovement.SpeedMultiplier /= speedMultiplier;
                m_IsModifierActive                = false;

                if (m_ParticleSystem != null)
                {
                    m_ParticleSystem.Stop();
                }
            }
        }
    }
}