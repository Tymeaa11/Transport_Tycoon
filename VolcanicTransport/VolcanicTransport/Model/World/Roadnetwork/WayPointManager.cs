using System.Numerics;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public static class WaypointManager
    {
        public static Dictionary<string, List<Vector2>> Paths { get; private set; }

        static WaypointManager()
        {
            Paths = new Dictionary<string, List<Vector2>>
            {
                // ==========================================
                // 1. EGYENESEK (Kereszteződésen is áthaladva)
                // ==========================================

                // Fentről lefelé (Jobb sáv bal oldalon)
                ["North_To_South"] = [new Vector2(24, 0), new Vector2(24, 64)],
                // Lentről felfelé (Jobb sáv jobb oldalon)
                ["South_To_North"] = [new Vector2(40, 64), new Vector2(40, 0)],
                // Balról jobbra (Jobb sáv alul)
                ["West_To_East"] = [new Vector2(0, 40), new Vector2(64, 40)],
                // Jobbról balra (Jobb sáv felül)
                ["East_To_West"] = [new Vector2(64, 24), new Vector2(0, 24)],


                // ==========================================
                // 2. JOBBRA KANYAROK (Kis (belső) ív, 3 pontból)
                // ==========================================

                // Fentről jön, Nyugatra (balra a képernyőn) megy ki
                ["North_To_West"] = [new Vector2(24, 0), new Vector2(24, 24), new Vector2(0, 24)],
                // Lentről jön, Keletre (jobbra a képernyőn) megy ki
                ["South_To_East"] = [new Vector2(40, 64), new Vector2(40, 40), new Vector2(64, 40)],
                // Balról jön, Délre (lefelé) megy ki
                ["West_To_South"] = [new Vector2(0, 40), new Vector2(24, 40), new Vector2(24, 64)],
                // Jobbról jön, Északra (felfelé) megy ki
                ["East_To_North"] = [new Vector2(64, 24), new Vector2(40, 24), new Vector2(40, 0)],


                // ==========================================
                // 3. BALRA KANYAROK (Nagy (külső) ív, 3 pontból)
                // ==========================================

                // Fentről jön, Keletre (jobbra) kanyarodik
                ["North_To_East"] = [new Vector2(24, 0), new Vector2(24, 40), new Vector2(64, 40)],
                // Lentről jön, Nyugatra (balra) kanyarodik
                ["South_To_West"] = [new Vector2(40, 64), new Vector2(40, 24), new Vector2(0, 24)],
                // Balról jön, Északra (felfelé) kanyarodik
                ["West_To_North"] = [new Vector2(0, 40), new Vector2(40, 40), new Vector2(40, 0)],
                // Jobbról jön, Délre (lefelé) kanyarodik
                ["East_To_South"] = [new Vector2(64, 24), new Vector2(24, 24), new Vector2(24, 64)]
            };
        }
    }
}