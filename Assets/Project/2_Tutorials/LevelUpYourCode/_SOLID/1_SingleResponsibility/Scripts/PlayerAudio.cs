using DesignPatterns.Utilities;
using System;
using System.Linq;
using UnityEngine;

namespace DesignPatterns.SRP
{
    /// <summary>
    /// 벽이나 장애물과 충돌할 때 효과음을 재생합니다.
    /// </summary>
    public class PlayerAudio : MonoBehaviour
    {
        [SerializeField] private float       m_CooldownTime  = 2f;
        [SerializeField] private AudioClip[] m_BounceClips;

        private float       m_LastTimePlayed;
        private AudioSource m_AudioSource;

        void Awake()
        {
            m_AudioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            m_LastTimePlayed = -m_CooldownTime;
        }

        public void PlayRandomClip()
        {
            // 다음 클립을 재생할 시간 계산
            float timeToNextPlay = m_CooldownTime + m_LastTimePlayed;

            // 쿨다운 시간이 지났는지 확인
            if (Time.time > timeToNextPlay)
            {
                m_LastTimePlayed   = Time.time;
                m_AudioSource.clip = GetRandomClip();
                m_AudioSource.Play();
            }
        }

        private AudioClip GetRandomClip()
        {
            // 배열의 클립 수를 기반으로 무작위 클립 가져오기
            int randomIndex = UnityEngine.Random.Range(0, m_BounceClips.Length);
            return m_BounceClips[randomIndex];
        }
    }
}
