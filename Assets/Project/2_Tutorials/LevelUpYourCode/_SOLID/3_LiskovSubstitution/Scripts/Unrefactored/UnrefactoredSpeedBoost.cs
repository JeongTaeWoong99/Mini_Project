using DesignPatterns.SRP;
using UnityEngine;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 리스코프 치환 원칙 위반 예시 클래스
    ///
    /// ⚠️ LSP 위반 ⚠️
    /// 이 하위 클래스는 시간 기반 duration을 추가하지만, 기본 클래스에는 이 개념이 없습니다.
    /// 로직은 작동하지만, "duration"은 기본 클래스의 개념이 아닙니다.
    /// 따라서 UnrefactoredSpeedBoost는 duration을 지원하지 않는 다른 PowerUp과 대체될 수 없습니다.
    ///
    /// 📝 문제점 :
    /// - 기본 클래스에서 정의하지 않은 duration 개념을 하위 클래스에서 임의로 추가
    /// - 기본 클래스의 ApplyEffect()는 duration 파라미터가 없음
    /// - 다른 PowerUp과 일관성 없는 동작 방식
    /// </summary>
    public class UnrefactoredSpeedBoost : UnrefactoredPowerUp
    {
        public float m_SpeedMultiplier = 2f;
        public float m_Duration        = 5f; // 기본 클래스에서 지원하지 않는 duration

        public override void ApplyEffect(GameObject player)
        {
            if (m_Duration > 0)
            {
                SpeedModifier playerMovement = player.GetComponent<SpeedModifier>();
                if (playerMovement != null)
                {
                    playerMovement.ModifySpeed(m_SpeedMultiplier, m_Duration);
                }
            }
            else
            {
                // 이 분기는 duration 없이 "ApplyEffect"만 기대하는 사용자에게 혼란을 줄 수 있습니다.
                // 이 로직을 사용하면 모든 PowerUp이 서로 교체 가능하지 않게 됩니다.
            }
        }
    }
}
