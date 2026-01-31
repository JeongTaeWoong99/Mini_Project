using UnityEngine;

namespace DesignPatterns.StatePattern
{
    public enum PlayerControllerState
    {
        Idle,
        Walk,
        Jump
    }

    public class UnrefactoredPlayerController : MonoBehaviour
    {
        // 이 방식은 동작하지만 확장성이 없음; 새로운 내부 상태를 생성할 때마다
        // case를 추가해야 함. 대신 스테이트 패턴을 사용할 것
        private PlayerControllerState state;

        private void Update()
        {
            GetInput();

            switch (state)
            {
                case PlayerControllerState.Idle:
                    Idle();
                    break;
                case PlayerControllerState.Walk:
                    Walk();
                    break;
                case PlayerControllerState.Jump:
                    Jump();
                    break;
            }
        }

        private void GetInput()
        {
            // 걷기 및 점프 입력 처리
        }

        private void Walk()
        {
            // 걷기 로직
        }

        private void Idle()
        {
            // 대기 로직
        }

        private void Jump()
        {
            // 점프 로직
        }
    }
}
