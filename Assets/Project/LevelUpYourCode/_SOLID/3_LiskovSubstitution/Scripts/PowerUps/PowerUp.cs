using DesignPatterns.SRP;
using UnityEngine;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// 파워업의 기본 클래스 (리팩토링 완료)
    ///
    /// ✅ 리스코프 치환 원칙 준수 ✅
    /// 기본 클래스에 m_Duration 필드를 추가하여,
    /// 모든 하위 클래스가 duration 개념을 공유할 수 있도록 설계되었습니다.
    /// 이제 모든 PowerUp 하위 클래스는 서로 교체 가능합니다.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class PowerUp : MonoBehaviour
    {
        [Tooltip("파워업 지속 시간 (일시적인 효과인 경우)")]
        [SerializeField] protected float m_Duration;

        protected const string k_PlayerTag = "Player";

        // 각 하위 클래스에서 오버라이드할 로직
        public abstract void ApplyEffect(GameObject player);

        // 모든 파워업에 공통적인 기능
        protected void OnTriggerEnter(Collider other)
        {
            // 플레이어가 아니면 무시
            if (!other.gameObject.CompareTag(k_PlayerTag))
                return;

            // 랜덤 비프음 재생
            PlaySound(other.gameObject);

            // 하위 클래스의 로직 적용
            ApplyEffect(other.gameObject);

            // 파워업 수집 또는 제거 처리
            CollectPowerUp();
        }

        /// <summary>
        /// 파워업 획득 시 사운드를 재생합니다.
        /// </summary>
        protected void PlaySound(GameObject player)
        {
            PlayerAudio m_PlayerAudio = player.GetComponent<PlayerAudio>();

            if (m_PlayerAudio != null)
            {
                m_PlayerAudio.PlayRandomClip();
            }
        }

        /// <summary>
        /// 파워업을 제거/소비합니다.
        /// </summary>
        protected void CollectPowerUp()
        {
            // 파워업 수집 또는 제거 처리
            Destroy(gameObject);
        }
    }
}
