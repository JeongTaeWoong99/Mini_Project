using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 제품(Product) 간의 공통 인터페이스
    /// 모든 제품은 이 인터페이스를 구현해야 함
    /// </summary>
    public interface IProduct
    {
        // 공통 속성 및 메서드를 여기에 추가
        public string ProductName { get; set; }

        // 각 구체적인 제품(Concrete Product)에서 커스터마이징
        public void Initialize();
    }
}
