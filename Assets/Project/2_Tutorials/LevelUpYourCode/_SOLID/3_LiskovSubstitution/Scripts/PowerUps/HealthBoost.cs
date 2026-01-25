using UnityEngine;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 체력 회복 파워업
    ///
    /// ✅ LSP 준수 ✅
    /// PowerUp 기본 클래스를 상속받아 체력 회복 기능을 구현합니다.
    /// ApplyEffect()를 오버라이드하여 고유한 효과를 제공하며,
    /// 기본 클래스의 동작 방식을 변경하지 않습니다.
    /// </summary>
    public class HealthBoost : PowerUp
    {
        [Tooltip("회복할 체력량")]
        [SerializeField] private float m_HealValue = 50f;

        /// <summary>
        /// 플레이어에게 체력 회복 효과를 적용합니다.
        /// </summary>
        public override void ApplyEffect(GameObject player)
        {
            Health playerHealth = player.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.Heal(m_HealValue);
            }
        }
    }
}