using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DesignPatterns.Factory
{
    /// <summary>
    /// 클릭으로 제품을 생성하는 예제 클래스
    /// 마우스 클릭 위치에 랜덤한 팩토리를 사용하여 제품을 생성
    /// </summary>
    public class ClickToCreate : MonoBehaviour
    {
        [Header("Click Settings")]
        [SerializeField] private LayerMask layerToClick;   // 클릭 가능한 레이어
        [SerializeField] private Vector3   offset;         // 생성 위치 오프셋

        [Header("Factory Settings")]
        [SerializeField] private Factory[] factories;      // 사용 가능한 팩토리 배열

        // 생성된 모든 제품을 추적하는 리스트
        private List<GameObject> createdProducts = new List<GameObject>();

        private void Update()
        {
            GetProductAtClick();
        }

        private void GetProductAtClick()
        {
            // 마우스 왼쪽 버튼 클릭 확인
            if (Input.GetMouseButtonDown(0))
            {
                // 리스트에서 랜덤한 팩토리 선택
                Factory    selectedFactory = factories[Random.Range(0, factories.Length)];
                Ray        ray             = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                // 레이캐스트가 지정된 레이어의 콜라이더에 충돌하는지 확인
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerToClick) && selectedFactory != null)
                {
                    IProduct product = selectedFactory.GetProduct(hitInfo.point + offset);

                    // 생성된 제품의 GameObject를 리스트에 추가
                    if (product is Component component)
                    {
                        createdProducts.Add(component.gameObject);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            // 이 오브젝트가 파괴될 때 생성된 모든 제품 정리
            foreach (GameObject product in createdProducts)
            {
                Destroy(product);
            }

            // 리스트 초기화
            createdProducts.Clear();
        }
    }
}
