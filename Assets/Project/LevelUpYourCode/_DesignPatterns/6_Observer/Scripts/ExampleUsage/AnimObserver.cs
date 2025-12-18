using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ★ 에임 옵접어

namespace DesignPatterns.Observer
{
    public class AnimObserver : MonoBehaviour
    {
        [SerializeField] ButtonSubject subjectToObserve;
        
        [SerializeField] Animation animClip;
        
        void Start()
        {
            if (subjectToObserve != null)
            {
                subjectToObserve.Clicked += OnThingHappened;
            }
        }

        private void OnThingHappened()
        {
            if (animClip != null)
            {
                animClip.Stop();
                animClip.Play();
            }
        }
    }
}
