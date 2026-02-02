using DesignPatterns.Utilities;
using UnityEngine;

namespace DesignPatterns.MVVM
{
    // 클릭으로 데미지를 전달하는 트리거 컴포넌트
    [RequireComponent(typeof(Collider))]
    public class DamageTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask       m_LayerMask;
        [SerializeField] private int             m_DamageValue = 10;
        [SerializeField] private HealthViewModel m_HealthViewModel;

        private Collider m_Collider;

        private void Awake()
        {
            m_Collider = GetComponent<Collider>();
        }

        private void Start()
        {
            // 유틸리티 클래스로 필수 필드 검증
            NullRefChecker.Validate(this);
        }

        private void Update()
        {
            if (m_HealthViewModel is null)
                return;

            // 마우스 좌클릭 감지
            if (Input.GetMouseButtonDown(0))
            {
                // 카메라에서 마우스 위치로 레이 생성
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // 레이캐스트로 콜라이더 충돌 확인
                if (Physics.Raycast(ray, Mathf.Infinity, m_LayerMask))
                {
                    m_HealthViewModel.ApplyDamage(m_DamageValue);
                }
            }
        }
    }
}
