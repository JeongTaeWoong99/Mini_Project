using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 구체적인 제품 A (Concrete Product A)
    /// IProduct 인터페이스를 구현하여 팩토리에서 생성 가능
    /// </summary>
    public class ProductA : MonoBehaviour, IProduct
    {
        [SerializeField] private string productName = "ProductA";

        public string ProductName { get => productName; set => productName = value; }

        // Private 변수
        private ParticleSystem particleSystem;

        public void Initialize()
        {
            gameObject.name  = productName;
            
            // 고유한 초기화 로직을 여기에 추가
            particleSystem   = GetComponentInChildren<ParticleSystem>();

            if (particleSystem == null)
                return;

            particleSystem.Stop();
            particleSystem.Play();
        }
    }
}
