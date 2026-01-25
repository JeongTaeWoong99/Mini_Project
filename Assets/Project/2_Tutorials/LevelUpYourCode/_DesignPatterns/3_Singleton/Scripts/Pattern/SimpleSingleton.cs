using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Singleton
{
    // ★★ 제일 간단한 버전
    
    /// <summary>
    /// 싱글톤 패턴의 간단한 버전을 구현합니다.
    /// SimpleSingleton의 인스턴스가 하나만 존재하도록 보장합니다.
    /// 전역 접근을 위해 정적 Instance 변수를 사용하세요.
    ///
    /// 두 개 이상의 인스턴스를 생성하려고 시도하면 새로운 인스턴스는 파괴됩니다.
    /// </summary>
    public class SimpleSingleton : MonoBehaviour
    {
        // 전역 접근
        public static SimpleSingleton Instance;

        private void Awake()
        {
            if (Instance != null)
            {
                // Instance가 이미 설정되어 있으면, 이 중복 인스턴스를 파괴
                Destroy(gameObject);
            }
            else
            {
                // Instance가 설정되지 않았으면, 이 인스턴스를 싱글톤으로 설정
                Instance = this;
            }
        }
    }
}  
