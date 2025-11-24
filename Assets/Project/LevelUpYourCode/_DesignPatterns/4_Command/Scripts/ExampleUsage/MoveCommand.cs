using UnityEngine;

namespace DesignPatterns.Command
{
    // ICommand 인터페이스를 구현한 구체적인 명령 클래스
    // 플레이어를 특정 방향으로 이동시키는 명령
    public class MoveCommand : ICommand
    {
        private PlayerMover m_PlayerMover;  // 이동시킬 플레이어
        private Vector3     m_MoveVector;   // 이동 방향 및 거리

        // 생성자 : 이동 명령에 필요한 정보를 저장
        public MoveCommand(PlayerMover player, Vector3 moveVector)
        {
            this.m_PlayerMover = player;
            this.m_MoveVector  = moveVector;
        }

        // 명령 실행 : 플레이어를 이동시키고 경로에 기록
        public void Execute()
        {
            // 이동할 위치를 경로 시각화에 추가
            m_PlayerMover.PlayerPath.AddToPath(m_PlayerMover.transform.position + m_MoveVector);

            // 플레이어를 실제로 이동
            m_PlayerMover.Move(m_MoveVector);
        }

        // 명령 되돌리기 : 반대 방향으로 이동하고 경로에서 제거
        public void Undo()
        {
            // 반대 방향으로 이동 (음수 벡터 사용)
            m_PlayerMover.Move(-m_MoveVector);

            // 경로 시각화에서 마지막 지점 제거
            m_PlayerMover.PlayerPath.RemoveFromPath();
        }
    }
}