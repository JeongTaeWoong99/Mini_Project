using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 모든 팩토리 타입의 기본 클래스
    /// 팩토리는 제품(Product)의 인스턴스를 생성하는 역할을 담당
    /// </summary>
    public abstract class Factory : MonoBehaviour
    {
        // 제품 인스턴스를 가져오는 추상 메서드
        public abstract IProduct GetProduct(Vector3 position);

        // 모든 팩토리에서 공유하는 메서드
        public string GetLog(IProduct product)
        {
            string logMessage = "Factory : created product " + product.ProductName;
            return logMessage;
        }
    }
}
