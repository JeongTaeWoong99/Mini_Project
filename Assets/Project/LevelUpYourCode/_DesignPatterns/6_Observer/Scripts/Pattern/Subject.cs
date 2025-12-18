using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// ★ 옵저버 패턴의 기본 구조 (템플릿/틀) 
// ★ 기본 옵접어 구조

namespace DesignPatterns.Observer
{
    public class Subject: MonoBehaviour
    {
        // 자신만의 델리게이트로 이벤트 정의하기
        //public delegate void ExampleDelegate();
        //public static event ExampleDelegate ExampleEvent;

        //... 또는 그냥 System.Action 사용하기
        public event Action ThingHappened;

        // 이벤트를 호출하여 모든 리스너/옵저버에게 브로드캐스트
        public void DoThing()
        {
            ThingHappened?.Invoke();
        }
    }
}

