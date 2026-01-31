using UnityEngine;

namespace DesignPatterns.StatePattern
{
    public class JumpState : IState
    {
        private PlayerController player;

        // 플레이어 색상 변경 (대안 : 생성자에 색상 값을 전달)
        private Color meshColor = Color.red;
        public  Color MeshColor { get => meshColor; set => meshColor = value; }

        // 생성자에 필요한 매개변수를 전달
        public JumpState(PlayerController player)
        {
            this.player = player;
        }

        public void Enter()
        {
            // 상태에 처음 진입할 때 실행되는 코드
            //Debug.Log("점프 상태 진입");
        }

        // 매 프레임 실행되는 로직, 새로운 상태로 전환하는 조건 포함
        public void Execute()
        {
            //Debug.Log("점프 상태 업데이트");

            if (player.IsGrounded)
            {
                if (Mathf.Abs(player.CharController.velocity.x) > 0.1f || Mathf.Abs(player.CharController.velocity.z) > 0.1f)
                {
                    player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.idleState);
                }
                else
                {
                    player.PlayerStateMachine.TransitionTo(player.PlayerStateMachine.walkState);
                }
            }
        }

        public void Exit()
        {
            // 상태에서 빠져나갈 때 실행되는 코드
            //Debug.Log("점프 상태 종료");
        }
    }
}
