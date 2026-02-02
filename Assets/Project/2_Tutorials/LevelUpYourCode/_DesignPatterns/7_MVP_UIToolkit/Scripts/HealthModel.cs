using UnityEngine;
using System;

namespace DesignPatterns.MVP_UIToolkit
{
    /// <summary>
    /// MVP 디자인 패턴(UI Toolkit)을 따르는 체력 데이터 컨테이너.
    /// </summary>
    [CreateAssetMenu(fileName = "HealthData", menuName = "DesignPatterns/MVP_UIToolkit/HealthModel")]
    public class HealthModel : ScriptableObject
    {
        // ViewModel과 통신하기 위한 이벤트
        public event Action HealthChanged;

        // 최소/최대값은 설정용 상수
        private const int k_MinHealth = 0;
        private const int k_MaxHealth = 100;

        [Tooltip("체력 오브젝트의 ID")]
        [SerializeField] private string m_LabelName;

        [Space]
        [Tooltip("현재 체력 값")]
        public int CurrentHealth;

        public int    MinHealth => k_MinHealth;
        public int    MaxHealth => k_MaxHealth;
        public string LabelName => m_LabelName;

        // 런타임 인스턴스를 반환 (각 오브젝트가 독립적인 데이터로 동작)
        public static HealthModel CreateInstance(HealthModel original)
        {
            var newInstance = CreateInstance<HealthModel>();

            // 필요한 필드 복사
            newInstance.CurrentHealth = original.CurrentHealth;
            newInstance.m_LabelName   = original.m_LabelName;
            return newInstance;
        }

        // Inspector에서 입력 시 유효 범위로 클램핑
        private void OnValidate()
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, k_MinHealth, k_MaxHealth);
        }

        private void OnEnable()
        {
            // 씬 재시작 시 오브젝트가 활성화되면 체력을 최대값으로 초기화
            Restore();
        }

        // 체력 증가
        public void Increment(int amount)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, k_MinHealth, k_MaxHealth);
            HealthChanged?.Invoke();
        }

        // 체력 감소
        public void Decrement(int amount)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth - amount, k_MinHealth, k_MaxHealth);
            HealthChanged?.Invoke();
        }

        // 체력을 최대값으로 복원
        public void Restore()
        {
            CurrentHealth = k_MaxHealth;
            HealthChanged?.Invoke();
        }
    }
}
