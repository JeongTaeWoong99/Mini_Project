using UnityEngine;
using DesignPatterns.Singleton;

namespace DesignPatterns.Strategy
{
    public abstract class Ability : ScriptableObject
    {
        [SerializeField] protected string         m_AbilityName;

        [Tooltip("UI 버튼 이미지 텍스처")]
        [SerializeField] protected Sprite         m_ButtonIcon;

        [Header("Visuals")]
        [SerializeField] protected ParticleSystem m_ParticleSystem;
        [SerializeField] protected AudioClip      m_AudioClip;

        // 각 전략은 커스텀 로직을 사용할 수 있다. 서브클래스에서 Use 메서드를 구현한다
        public virtual void Use(GameObject gameObject)
        {
            // Use 메서드는 이름을 로그로 출력하고, 사운드와 파티클 이펙트를 재생한다
            Debug.Log($"능력 사용 : {m_AbilityName}");
            PlaySound();
            PlayParticleFX();
        }

        public Sprite ButtonIcon => m_ButtonIcon;

        private void PlayParticleFX()
        {
            if (m_ParticleSystem != null)
            {
                ParticleSystem instance = Instantiate(m_ParticleSystem, Vector3.zero, Quaternion.identity);
                // 파티클 시스템이 먼저 정지된 후 재생되도록 한다
                instance.Stop();
                instance.Play();
            }
        }

        private void PlaySound()
        {
            if (m_AudioClip)
                AudioManager.Instance.PlaySoundEffect(m_AudioClip);
        }
    }
}
