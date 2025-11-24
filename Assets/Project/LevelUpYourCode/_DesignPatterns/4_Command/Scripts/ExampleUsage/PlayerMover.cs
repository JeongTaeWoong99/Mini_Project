using UnityEngine;

namespace DesignPatterns.Command
{
    // 플레이어 이동을 실제로 수행하는 클래스 (Receiver 역할)
    // Command Pattern에서 명령을 받아 실제 작업을 처리하는 객체
    public class PlayerMover : MonoBehaviour
    {
        [SerializeField] private LayerMask m_ObstacleLayer;  // 장애물 레이어

        private const float k_BoardSpacing = 1f;  // 이동 거리 (레이캐스트용)

        // 경로 시각화 컴포넌트
        private PlayerPath m_PlayerPath;
        public PlayerPath PlayerPath => m_PlayerPath;


        private void Start()
        {
            m_PlayerPath = gameObject.GetComponent<PlayerPath>();
        }

        // XZ 평면에서 플레이어를 이동시킴
        // 실제 이동을 수행하는 메서드
        public void Move(Vector3 movement)
        {
            Vector3 destination = transform.position + movement;
            transform.position  = destination;
        }

        // 이동이 가능한지 검사 (장애물 체크)
        // Physics.Raycast로 이동 방향에 장애물이 있는지 확인
        public bool IsValidMove(Vector3 movement)
        {
            return !Physics.Raycast(transform.position, movement, k_BoardSpacing, m_ObstacleLayer);
        }
    }
}