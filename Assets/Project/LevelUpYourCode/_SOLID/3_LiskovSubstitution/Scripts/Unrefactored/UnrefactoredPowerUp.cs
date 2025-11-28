using UnityEngine;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 파워업의 기본 클래스 (리팩토링 전)
    ///
    /// ⚠️ 리스코프 치환 원칙 위반 예시 ⚠️
    /// 이 클래스는 duration(지속 시간) 개념이 없어서,
    /// 하위 클래스에서 duration을 사용하려고 하면 원칙에 위배됩니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class UnrefactoredPowerUp : MonoBehaviour
    {
        const string k_PlayerTag = "Player";

        // 각 하위 클래스에서 오버라이드할 로직
        public abstract void ApplyEffect(GameObject player);

        // 모든 파워업에 공통적인 기능
        protected void OnTriggerEnter(Collider other)
        {
            // 플레이어가 아니면 무시
            if (!other.gameObject.CompareTag(k_PlayerTag))
                return;

            // 하위 클래스의 로직 적용
            ApplyEffect(other.gameObject);

            // 파워업 수집 또는 제거 처리
            CollectPowerUp();
        }

        // 파워업 제거/소비
        protected void CollectPowerUp()
        {
            // 파워업 수집 또는 제거 처리
            Destroy(gameObject);
        }
    }
}