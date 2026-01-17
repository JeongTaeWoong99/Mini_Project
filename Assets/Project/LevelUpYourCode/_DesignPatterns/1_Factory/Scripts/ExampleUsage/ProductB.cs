using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 구체적인 제품 B (Concrete Product B)
    /// IProduct 인터페이스를 구현하여 팩토리에서 생성 가능
    /// </summary>
    public class ProductB : MonoBehaviour, IProduct
    {
        [SerializeField] private string productName = "ProductB";

        public string ProductName { get => productName; set => productName = value; }

        // Private 변수
        private AudioSource audioSource;

        public void Initialize()
        {
            gameObject.name = productName;

            // 고유한 초기화 로직 수행
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
                return;

            audioSource.Stop();
            audioSource.Play();
        }
    }
}
