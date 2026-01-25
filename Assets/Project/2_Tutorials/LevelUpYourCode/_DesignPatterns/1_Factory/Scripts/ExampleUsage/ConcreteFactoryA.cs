using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 구체적인 팩토리 A (Concrete Factory A)
    /// ProductA를 생성하는 역할을 담당
    /// </summary>
    public class ConcreteFactoryA : Factory
    {
        // 프리팹 생성에 사용
        [SerializeField] private ProductA productPrefab;

        public override IProduct GetProduct(Vector3 position)
        {
            // 프리팹 인스턴스를 생성하고 제품 컴포넌트를 가져옴
            GameObject instance   = Instantiate(productPrefab.gameObject, position, Quaternion.identity);
            ProductA   newProduct = instance.GetComponent<ProductA>();

            // 각 제품은 고유한 로직을 포함
            newProduct.Initialize();

            return newProduct;
        }
    }
}
