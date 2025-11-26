using UnityEngine;
using DesignPatterns.Utilities;

namespace DesignPatterns.SRP
{
    /// <summary>
    /// 단일 책임 원칙(SRP)을 준수하는 클래스입니다. 모놀리식 클래스 대신,
    /// 이 구현은 전문화된 컴포넌트 간에 책임을 분할합니다. 각 컴포넌트는
    /// 플레이어 동작의 특정 측면(입력 처리, 이동, 오디오, 시각 효과)에 집중합니다.
    /// </summary>
    [RequireComponent(typeof(PlayerInput), typeof(PlayerAudio), typeof(PlayerMovement))]
    public class Player : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("게임 환경에서 장애물을 식별하기 위한 LayerMask")]
        private LayerMask m_ObstacleLayer;

        // 플레이어 기능의 다양한 측면을 처리하기 위한 컴포넌트
        private PlayerInput    m_PlayerInput;
        private PlayerMovement m_PlayerMovement;
        private PlayerAudio    m_PlayerAudio;
        private PlayerFX       m_PlayerFX;

        private void Awake()
        {
            Initialize();
        }

        // 컴포넌트 참조 설정
        private void Initialize()
        {
            m_PlayerInput    = GetComponent<PlayerInput>();
            m_PlayerMovement = GetComponent<PlayerMovement>();
            m_PlayerAudio    = GetComponent<PlayerAudio>();
            m_PlayerFX       = GetComponent<PlayerFX>();
        }

        // 컨트롤러가 다른 콜라이더와 충돌할 때 호출되는 메서드
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 충돌한 오브젝트가 장애물 레이어에 있는지 확인
            if (m_ObstacleLayer.ContainsLayer(hit.gameObject))
            {
                // 충돌 시 무작위 오디오 클립 재생
                m_PlayerAudio.PlayRandomClip();

                // 시각 효과가 정의된 경우 트리거
                if (m_PlayerFX != null)
                    m_PlayerFX.PlayEffect();
            }
        }

        private void LateUpdate()
        {
            // PlayerInput 컴포넌트에서 입력 벡터 가져오기
            Vector3 inputVector = m_PlayerInput.InputVector;

            // 입력 벡터를 기반으로 플레이어 이동
            m_PlayerMovement.Move(inputVector);
        }
    }
}
