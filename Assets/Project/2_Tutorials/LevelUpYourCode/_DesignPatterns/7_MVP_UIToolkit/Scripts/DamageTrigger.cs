using DesignPatterns.Utilities;
using UnityEngine;

namespace DesignPatterns.MVP_UIToolkit
{
    // 클릭으로 데미지를 전달하는 트리거 컴포넌트
    [RequireComponent(typeof(Collider))]
    public class DamageTrigger : MonoBehaviour
    {
        [SerializeField] private LayerMask       m_LayerMask;
        [SerializeField] private int             m_DamageValue = 10;
        [SerializeField] private HealthPresenter m_HealthPresenter;

        private Collider m_Collider;

        private void Start()
        {
            NullRefChecker.Validate(this);
            m_Collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (m_HealthPresenter is null)
                return;

            // 마우스 좌클릭으로 콜라이더 위에서 클릭했는지 확인
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, Mathf.Infinity, m_LayerMask))
                {
                    m_HealthPresenter.ApplyDamage(m_DamageValue);
                }
            }
        }
    }
}
