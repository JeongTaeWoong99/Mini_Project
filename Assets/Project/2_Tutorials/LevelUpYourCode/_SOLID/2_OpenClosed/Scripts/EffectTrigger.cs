using System;
using UnityEngine;

namespace DesignPatterns.OCP
{
    public class EffectTrigger : MonoBehaviour
    {
        [Tooltip("이 컴포넌트와 충돌 시 트리거되는 AreaOfEffect")]
        [SerializeField] AreaOfEffect m_Effect;
        [Tooltip("트리거 간 최소 시간(초)")]
        [SerializeField] float m_Cooldown = 2f;

        float m_LastEffectTime = -1;
        // 플레이어 태그
        const string k_PlayerTag = "Player";

        private void OnTriggerEnter(Collider other)
        {
            PlayEffect(other);

            if (other.CompareTag(k_PlayerTag) && m_Effect != null)
                m_Effect.ShowAreaText();
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag(k_PlayerTag))
                PlayEffect(other);
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(k_PlayerTag) && m_Effect != null)
                m_Effect.ShowLabelText(string.Empty);
        }

        private void PlayEffect(Collider other)
        {
            float nextEffectTime = m_LastEffectTime + m_Cooldown;

            // 태그로 확인
            if (other.CompareTag(k_PlayerTag) && Time.time > nextEffectTime)
            {
                m_LastEffectTime = Time.time;

                // 플레이어용 효과 트리거
                m_Effect.PlayEffect();
            }
        }
    }
}