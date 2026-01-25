using UnityEngine;
using UnityEngine.Events;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 오브젝트의 체력을 추적하는 기본 동작을 제공하는 클래스
    /// 데미지, 회복, 사망 처리 등의 기능을 포함합니다.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private float m_MaxHealth     = 100f;
        [SerializeField] private float m_CurrentHealth;
        [SerializeField] private bool  m_ResetOnStart;

        [Tooltip("이 오브젝트가 사망했을 때 리스너에게 알림")]
        public UnityEvent Died;

        [Tooltip("체력 퍼센트가 변경되었을 때 리스너에게 알림")]
        public UnityEvent<float> HealthChanged;

        protected bool m_IsInvulnerable;
        protected bool m_IsDead;

        // 프로퍼티
        public float MaxHealth      { get => m_MaxHealth;      set => m_MaxHealth = value; }
        public float CurrentHealth  => m_CurrentHealth;
        public bool  IsInvulnerable { get => m_IsInvulnerable; set => m_IsInvulnerable = value; }

        private void Awake()
        {
            if (m_ResetOnStart)
                m_CurrentHealth = MaxHealth;
        }

        private void Start()
        {
            HealthChanged.Invoke(CurrentHealth / MaxHealth);
        }

        /// <summary>
        /// GameObject에 데미지를 적용합니다.
        /// </summary>
        /// <param name="amount">데미지량</param>
        public virtual void TakeDamage(float amount)
        {
            // 이미 죽었거나 무적 상태면 아무것도 하지 않음
            if (m_IsDead || m_IsInvulnerable)
                return;

            m_CurrentHealth -= amount;

            // 사망 조건 확인
            if (m_CurrentHealth <= 0)
            {
                m_CurrentHealth = 0;
                Die();
            }

            // 현재 체력 퍼센트를 리스너에게 알림
            HealthChanged.Invoke(CurrentHealth / MaxHealth);
        }

        /// <summary>
        /// GameObject를 회복하고, 최대값까지만 회복하며 리스너에게 알립니다.
        /// </summary>
        /// <param name="amount">회복량</param>
        public void Heal(float amount)
        {
            // 이미 죽었으면 회복하지 않음
            if (m_IsDead)
                return;

            m_CurrentHealth += amount;

            if (m_CurrentHealth > MaxHealth)
                m_CurrentHealth = MaxHealth;

            HealthChanged.Invoke(CurrentHealth / MaxHealth);
        }

        /// <summary>
        /// 이 오브젝트가 사망했음을 리스너에게 알리고,
        /// 추가 상호작용을 방지하기 위해 GameObject를 비활성화합니다.
        /// </summary>
        protected virtual void Die()
        {
            // 한 번만 죽음
            if (m_IsDead)
                return;

            m_IsDead = true;
            Died.Invoke();
            gameObject.SetActive(false);
        }
    }
}