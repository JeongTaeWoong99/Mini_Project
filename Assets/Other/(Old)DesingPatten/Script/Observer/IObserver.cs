namespace DesingPatten.Observer
{
    public enum GameEvent
    {
        GameOver,
        TimeUp,
        CastleDestroyed,
        CoinChanged,
        MonsterSpawned
    }

    public interface IObserver
    {
        void Notify(GameEvent gameEvent, object data = null);
    }
}