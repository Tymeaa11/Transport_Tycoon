namespace VolcanicTransport.Model.Persistance
{
    public readonly record struct GameData(
        World.World World,
        bool IsPaused,
        double Time,
        double PlayerMoney
    )
    {
        public GameData(GameModel gm) : this(
            GameModel.WorldInstance,
            gm.IsPaused,
            gm.Time,
            gm.PlayerMoney)
        { }
    }
}
