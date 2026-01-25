using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

// ★ 파티클 옵접어

namespace DesignPatterns.Observer
{

    public class ParticleSystemObserver : MonoBehaviour
    {
        [SerializeField] ButtonSubject m_SubjectToObserve;
        
        [SerializeField] ParticleSystem m_ParticleSystem;

        private void Awake()
        {
            if (m_SubjectToObserve != null)
            {
                m_SubjectToObserve.Clicked += OnThingHappened;
            }
        }

        private void OnThingHappened()
        {
            if (m_ParticleSystem != null)
            {
                m_ParticleSystem.Stop();
                m_ParticleSystem.Play();
            }
        }

        private void OnDestroy()
        {
            // 오브젝트가 파괴될 때 이벤트에서 구독 해제/등록 취소
            if (m_SubjectToObserve != null)
            {
                m_SubjectToObserve.Clicked -= OnThingHappened;
            }
        }

    }
}
