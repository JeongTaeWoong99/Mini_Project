using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.DIP
{
    /// <summary>
    /// 전환 가능한 객체에 대한 계약을 정의합니다.
    /// 이 인터페이스는 객체의 활성화/비활성화 세부사항을 추상화하여
    /// 의존 역전 원칙(Dependency Inversion Principle, DIP)을 구현하는 데 도움을 줍니다.
    /// </summary>
    public interface ISwitchable 
    {
        public bool IsActive { get; }

        public void Activate();
        public void Deactivate();
    }
}
