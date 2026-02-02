using UnityEngine;

namespace DesignPatterns.MVP
{
    // 클릭으로 데미지를 주는 컴포넌트 (User Input 담당)
    [RequireComponent(typeof(Collider))]
    public class ClickDamage : MonoBehaviour
    {
        [SerializeField] private LayerMask       m_LayerMask;
        [SerializeField] private int             m_DamageValue = 10;
        [SerializeField] private HealthPresenter m_HealthPresenter;

        private Collider m_Collider;

        private void Awake()
        {
            m_Collider        = GetComponent<Collider>();
            m_HealthPresenter = GetComponent<HealthPresenter>();
        }

        private void Update()
        {
            // 마우스 좌클릭 감지
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                // 레이캐스트로 콜라이더 충돌 확인
                if (Physics.Raycast(ray, Mathf.Infinity, m_LayerMask))
                {
                    // ?. (Null 조건부 연산자)
                    // m_HealthPresenter?.Damage() : null이면 호출을 건너뛰고, 있으면 정상 실행
                    // m_HealthPresenter.Damage()  : null이면 NullReferenceException 에러 발생
                    m_HealthPresenter?.Damage(m_DamageValue);
                }
            }
        }
    }
}
