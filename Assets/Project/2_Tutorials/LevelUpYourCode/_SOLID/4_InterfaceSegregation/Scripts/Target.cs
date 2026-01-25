using UnityEngine;
using DesignPatterns.ISP;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 게임 내 타겟의 기본 클래스로, 체력과 데미지 시스템을 포함합니다.
    /// </summary>
    public class Target : Health, IDamageable
    {
        [Tooltip("이 타겟의 데미지 배율 커스터마이징")]
        [SerializeField] float m_DamageMultiplier = 1f;
        public override void TakeDamage(float amount)
        {

            base.TakeDamage(amount * m_DamageMultiplier);

            // 추가적인 클래스별 로직을 여기에 커스터마이징
            // Debug.Log($"Target custom TakeDamage: {amount}");
        }

    }
}