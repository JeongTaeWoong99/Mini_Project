namespace DesignPatterns.StatePattern
{
    public interface IState : IColorable
    {
        public void Enter()
        {
            // 상태에 처음 진입할 때 실행되는 코드
        }

        public void Execute()
        {
            // 매 프레임 실행되는 로직, 새로운 상태로 전환하는 조건 포함
        }

        public void Exit()
        {
            // 상태에서 빠져나갈 때 실행되는 코드
        }
    }
}
