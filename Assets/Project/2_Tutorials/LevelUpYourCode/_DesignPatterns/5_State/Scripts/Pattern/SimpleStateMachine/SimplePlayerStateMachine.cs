using System;

namespace DesignPatterns.StatePattern
{
    // 상태 머신 처리
    [Serializable]
    public class SimplePlayerStateMachine
    {
        public IState CurrentState { get; private set; }

        // 상태 객체에 대한 참조
        public WalkState walkState;
        public JumpState jumpState;
        public IdleState idleState;

        // 상태 변경을 다른 객체에 알리기 위한 이벤트
        public event Action<IState> stateChanged;

        // 생성자에 필요한 매개변수를 전달
        public SimplePlayerStateMachine(PlayerController player)
        {
            // 각 상태의 인스턴스를 생성하고 PlayerController를 전달
            this.walkState = new WalkState(player);
            this.jumpState = new JumpState(player);
            this.idleState = new IdleState(player);
        }

        // 시작 상태를 설정
        public void Initialize(IState state)
        {
            CurrentState = state;
            state.Enter();

            // 다른 객체에 상태가 변경되었음을 알림(PlayerStateView구독 중)
            stateChanged?.Invoke(state);
        }

        // 현재 상태를 종료하고 다른 상태로 진입
        public void TransitionTo(IState nextState)
        {
            CurrentState.Exit();
            CurrentState = nextState;
            nextState.Enter();

            // 다른 객체에 상태가 변경되었음을 알림(PlayerStateView구독 중)
            stateChanged?.Invoke(nextState);
        }

        // StateMachine이 현재 상태를 업데이트할 수 있도록 허용
        public void Execute()
        {
            if (CurrentState != null)
            {
                CurrentState.Execute();
            }
        }
    }
}
