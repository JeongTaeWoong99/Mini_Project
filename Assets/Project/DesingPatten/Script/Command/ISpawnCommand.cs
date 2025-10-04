namespace DesingPatten.Command
{
    public interface ISpawnCommand
    { 
        public abstract void Execute();      // 단순히 실행만 하는 스크립트(= void Execute();)
        public abstract void StateUpdate();
    }
}