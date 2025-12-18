using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ★ 오디오 옵접어

namespace DesignPatterns.Observer
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioObserver : MonoBehaviour
    {
        // 관찰할 의존성
        [SerializeField] ButtonSubject subjectToObserve;
        
        [SerializeField] float delay = 0f;
        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();

            if (subjectToObserve != null)
            {
                subjectToObserve.Clicked += OnThingHappened;
            }
        }

        public void OnThingHappened()
        {
            StartCoroutine(PlayWithDelay());
        }

        IEnumerator PlayWithDelay()
        {
            yield return new WaitForSeconds(delay);
            source.Stop();
            source.Play();
        }

        private void OnDestroy()
        {
            // 오브젝트가 파괴될 때 이벤트에서 구독 해제/등록 취소
            if (subjectToObserve != null)
            {
                subjectToObserve.Clicked -= OnThingHappened;
            }
        }
    }
}
