using UnityEngine;

namespace DesignPatterns.StatePattern
{
    public class IdleState : IState
    {
        private PlayerController player;

        // 플레이어 색상 변경 (대안 : 생성자에 색상 값을 전달)
        Color meshColor = Color.white;
        public Color MeshColor { get => meshColor; set => meshColor = value; }

        // 생성자에 필요한 매개변수를 전달
        public IdleState(PlayerController player)
        {
            this.player = player;
        }

        public void Enter()
        {
            // 상태에 처음 진입할 때 실행되는 코드
            //Debug.Log("대기 상태 진입");
        }

        // 매 프레임 실행되는 로직, 새로운 상태로 전환하는 조건 포함
        public void Execute()
        {
            // 바닥에 닿지 않으면 점프 상태로 전환
            if (!player.IsGrounded)
            {
                player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.jumpState);
            }

            // 최소 임계값 이상으로 이동하면 걷기 상태로 전환
            if (Mathf.Abs(player.CharController.velocity.x) > 0.1f || Mathf.Abs(player.CharController.velocity.z) > 0.1f)
            {
                player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.walkState);
            }
        }

        public void Exit()
        {
            // 상태에서 빠져나갈 때 실행되는 코드
            //Debug.Log("대기 상태 종료");
        }
    }
}
