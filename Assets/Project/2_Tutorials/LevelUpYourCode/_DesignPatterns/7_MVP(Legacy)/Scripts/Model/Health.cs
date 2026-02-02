using System;
using UnityEngine;

namespace DesignPatterns.MVP
{
    // Model 클래스. MVP 패턴의 데이터를 보유한다.
    // System.Object 또는 ScriptableObject로도 구현 가능하다.
    public class Health : MonoBehaviour
    {
        // Presenter에게 체력 변경을 알리는 이벤트.
        // 값을 저장하는 데 시간이 걸리는 경우(예 : 디스크 저장, DB 저장)에 유용하다.
        public event Action HealthChanged;

        private const int k_MinHealth = 0;
        private const int k_MaxHealth = 100;
        
        private int m_CurrentHealth;

        // 프로퍼티
        public int CurrentHealth
        {
            get => m_CurrentHealth;
            set
            {
                m_CurrentHealth = Mathf.Clamp(value, k_MinHealth, k_MaxHealth);
                HealthChanged?.Invoke();
            }
        }
        public int MinHealth => k_MinHealth;
        public int MaxHealth => k_MaxHealth;

        // 체력 증가
        public void Increment(int amount)
        {
            CurrentHealth += amount;
        }

        // 체력 감소
        public void Decrement(int amount)
        {
            CurrentHealth -= amount;
        }

        // 체력을 최대값으로 복원
        public void Restore()
        {
            CurrentHealth = k_MaxHealth;
        }
    }
}
