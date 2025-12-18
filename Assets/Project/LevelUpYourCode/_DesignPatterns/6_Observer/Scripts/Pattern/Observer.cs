using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ★ 옵저버 패턴의 기본 구조 (템플릿/틀) 
// ★ 기본 옵접어 구조

namespace DesignPatterns.Observer
{
    public class ExampleObserver : MonoBehaviour
    {
        // 관찰/리스닝할 Subject에 대한 참조
        [SerializeField] Subject subjectToObserve;

        // 이벤트 처리 메서드 : 함수 시그니처는 Subject의 이벤트와 일치해야 함
        private void OnThingHappened()
        {
            // 이벤트에 반응하는 로직을 여기에 작성
        }

        private void Awake()
        {
            // Subject의 이벤트에 구독/등록
            if (subjectToObserve != null)
            {
                subjectToObserve.ThingHappened += OnThingHappened;
            }
        }

        private void OnDestroy()
        {
            // 오브젝트가 파괴될 때 구독 해제/등록 취소
            if (subjectToObserve != null)
            {
                subjectToObserve.ThingHappened -= OnThingHappened;
            }
        }
    }
}
