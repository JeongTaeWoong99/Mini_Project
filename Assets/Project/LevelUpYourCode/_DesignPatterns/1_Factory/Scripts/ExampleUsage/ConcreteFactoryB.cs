using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 구체적인 팩토리 B (Concrete Factory B)
    /// ProductB를 생성하는 역할을 담당
    /// </summary>
    public class ConcreteFactoryB : Factory
    {
        // 프리팹 생성에 사용
        [SerializeField] private ProductB productPrefab;

        public override IProduct GetProduct(Vector3 position)
        {
            // 프리팹 인스턴스를 생성하고 제품 컴포넌트를 가져옴
            GameObject instance   = Instantiate(productPrefab.gameObject, position, Quaternion.identity);
            ProductB   newProduct = instance.GetComponent<ProductB>();

            // 각 제품은 고유한 로직을 포함
            newProduct.Initialize();

            // 이 팩토리에 고유한 동작 추가
            instance.name = newProduct.ProductName;
            Debug.Log(GetLog(newProduct));

            return newProduct;
        }
    }
}
