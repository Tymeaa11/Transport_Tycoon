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

                ["North_To_End"] = [new Vector2(24, 0), new Vector2(24, 32)], // Fentről jön, középen megáll
                ["South_To_End"] = [new Vector2(40, 64), new Vector2(40, 32)], // Lentről jön, középen megáll
                ["West_To_End"] = [new Vector2(0, 40), new Vector2(32, 40)], // Balról jön, középen megáll
                ["East_To_End"] = [new Vector2(64, 24), new Vector2(32, 24)], // Jobbról jön, középen megáll


                ["Start_To_North"] = [new Vector2(40, 32), new Vector2(40, 0)],
                ["Start_To_South"] = [new Vector2(24, 32), new Vector2(24, 64)],
                ["Start_To_East"] = [new Vector2(32, 40), new Vector2(64, 40)],
                ["Start_To_West"] = [new Vector2(32, 24), new Vector2(0, 24)],

                [(PathDirection.North, PathDirection.Start)] = [new Vector2(L1, 0), new Vector2(L1, MID)],
                [(PathDirection.South, PathDirection.Start)] = [new Vector2(L2, END), new Vector2(L2, MID)],
                [(PathDirection.West, PathDirection.Start)] = [new Vector2(0, L2), new Vector2(MID, L2)],
                [(PathDirection.East, PathDirection.Start)] = [new Vector2(END, L1), new Vector2(MID, L1)],

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


                [(PathDirection.End, PathDirection.North)] = [new Vector2(L2, MID), new Vector2(L2, 0)],
                [(PathDirection.End, PathDirection.South)] = [new Vector2(L1, MID), new Vector2(L1, END)],
                [(PathDirection.End, PathDirection.East)] = [new Vector2(MID, L2), new Vector2(END, L2)],
                [(PathDirection.End, PathDirection.West)] = [new Vector2(MID, L1), new Vector2(0, L1)],
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


                [(PathDirection.North, PathDirection.North)] = [new Vector2(L1, 0), new Vector2(L1, MID), new Vector2(L1, END)],
                [(PathDirection.South, PathDirection.South)] = [new Vector2(L2, END), new Vector2(L2, MID), new Vector2(L2, 0)],
                [(PathDirection.West, PathDirection.West)] = [new Vector2(0, L2), new Vector2(MID, L2), new Vector2(END, L2)],
                [(PathDirection.East, PathDirection.East)] = [new Vector2(END, L1), new Vector2(MID, L1), new Vector2(0, L1)],
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