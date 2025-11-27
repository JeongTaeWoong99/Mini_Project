
using System;
using UnityEngine;
using DesignPatterns.Utilities;
using UnityEngine.UI;

namespace DesignPatterns.OCP
{
    /// <summary>
    /// ParticleSystem과 AudioClip을 재생하는 클래스.
    ///
    /// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
    /// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
    /// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
    /// </summary>
    public abstract class AreaOfEffect: MonoBehaviour
    {
        [Header("Particle Effect")]
        [SerializeField]
        [Optional]
        ParticleSystem m_EffectParticleSystem;
        [Header("Audio Effect")]
        [Optional]
        [SerializeField]
        AudioSource m_EffectAudioSource;
        [Optional]
        [SerializeField]
        AudioClip m_EffectSoundFX;
        [Space]
        [SerializeField] float  m_CooldownTime = 1.0f;
        [SerializeField] string m_LabelString;
        [SerializeField] Text   m_LabelText;

        /// <summary> 이 AreaOfEffect의 파티클 시스템 </summary>
        public ParticleSystem EffectParticleSystem => m_EffectParticleSystem;
        /// <summary> 이 AreaOfEffect의 오디오 소스 </summary>
        public AudioSource EffectAudioSource => m_EffectAudioSource;
        /// <summary> 이 AreaOfEffect의 오디오 클립 </summary>
        public AudioClip EffectSoundFX => m_EffectSoundFX;
        // public float TotalArea => CalculateArea();
        private float cooldownTimer;
        
        /// <summary>
        /// 각 AreaOfEffect 서브클래스는 고유한 CalculateArea 정의를 구현합니다.
        /// </summary>
        /// <returns></returns>
        public abstract float CalculateArea();

        /// <summary>
        /// 사운드와 효과를 재생합니다.
        /// </summary>
        private void Start()
        {
            if (m_LabelText != null)
                m_LabelText.text = string.Empty;
        }

        public void PlayEffect()
        {
            // 쿨다운 시간이 경과했는지 확인
            if (Time.time >= cooldownTimer)
            {
                cooldownTimer = Time.time + m_CooldownTime;
                PlayParticleEffect();
                PlaySoundEffect();

                ShowAreaText();
            }
        }

        private void PlayParticleEffect()
        {
            if (m_EffectParticleSystem != null)
            {
                m_EffectParticleSystem.Play();
            }
        }

        private void PlaySoundEffect()
        {
            if (m_EffectAudioSource != null && m_EffectSoundFX != null)
            {
                m_EffectAudioSource.PlayOneShot(m_EffectSoundFX);
            }
        }

        public void ShowLabelText(string textToShow)
        {
            if (m_LabelText != null)
            {
                m_LabelText.text = textToShow;
            }
        }

        public void ShowAreaText()
        {
            ShowLabelText(m_LabelString + " " + CalculateArea());
        }
    }
}
