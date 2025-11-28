using DesignPatterns.SRP;
using UnityEngine;
using UnityEngine.Serialization;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 속도 증가 파워업
    ///
    /// ✅ LSP 준수 ✅
    /// PowerUp 기본 클래스를 상속받아 속도 증가 효과를 구현합니다.
    /// 기본 클래스의 m_Duration을 사용하여 일시적인 속도 증가를 제공합니다.
    /// 각 PowerUp 하위 클래스는 고유한 동작을 가질 수 있지만,
    /// 기본 클래스의 계약(contract)을 준수합니다.
    /// </summary>
    public class SpeedBoost : PowerUp
    {
        [Header("속도 파라미터")]
        [Tooltip("속도 배율 (2.0이면 2배 빠르게)")]
        [SerializeField] private float m_SpeedMultiplier = 2f;

        /// <summary>
        /// 플레이어에게 속도 증가 효과를 적용합니다.
        /// </summary>
        public override void ApplyEffect(GameObject player)
        {
            // SpeedBoost 로직 추가
            SpeedModifier speedModifier = player.GetComponent<SpeedModifier>();

            if (speedModifier != null)
            {
                // 기본 클래스의 m_Duration을 사용하여 속도 수정
                speedModifier.ModifySpeed(m_SpeedMultiplier, m_Duration);
            }
        }
    }
}