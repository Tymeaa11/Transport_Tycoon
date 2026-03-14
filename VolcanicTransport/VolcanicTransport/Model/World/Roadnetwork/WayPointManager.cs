using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public static class WaypointManager
    {
        public static Dictionary<string, List<Vector2>> Paths { get; private set; }

        static WaypointManager()
        {
            Paths = new Dictionary<string, List<Vector2>>();

            // ==========================================
            // 1. EGYENESEK (Kereszteződésen is áthaladva)
            // ==========================================

            // Fentről lefelé (Jobb sáv bal oldalon)
            Paths["North_To_South"] = new List<Vector2> { new Vector2(24, 0), new Vector2(24, 64) };
            // Lentről felfelé (Jobb sáv jobb oldalon)
            Paths["South_To_North"] = new List<Vector2> { new Vector2(40, 64), new Vector2(40, 0) };
            // Balról jobbra (Jobb sáv alul)
            Paths["West_To_East"] = new List<Vector2> { new Vector2(0, 40), new Vector2(64, 40) };
            // Jobbról balra (Jobb sáv felül)
            Paths["East_To_West"] = new List<Vector2> { new Vector2(64, 24), new Vector2(0, 24) };


            // ==========================================
            // 2. JOBBRA KANYAROK (Kis (belső) ív, 3 pontból)
            // ==========================================

            // Fentről jön, Nyugatra (balra a képernyőn) megy ki
            Paths["North_To_West"] = new List<Vector2> { new Vector2(24, 0), new Vector2(24, 24), new Vector2(0, 24) };
            // Lentről jön, Keletre (jobbra a képernyőn) megy ki
            Paths["South_To_East"] = new List<Vector2> { new Vector2(40, 64), new Vector2(40, 40), new Vector2(64, 40) };
            // Balról jön, Délre (lefelé) megy ki
            Paths["West_To_South"] = new List<Vector2> { new Vector2(0, 40), new Vector2(24, 40), new Vector2(24, 64) };
            // Jobbról jön, Északra (felfelé) megy ki
            Paths["East_To_North"] = new List<Vector2> { new Vector2(64, 24), new Vector2(40, 24), new Vector2(40, 0) };


            // ==========================================
            // 3. BALRA KANYAROK (Nagy (külső) ív, 3 pontból)
            // ==========================================

            // Fentről jön, Keletre (jobbra) kanyarodik
            Paths["North_To_East"] = new List<Vector2> { new Vector2(24, 0), new Vector2(24, 40), new Vector2(64, 40) };
            // Lentről jön, Nyugatra (balra) kanyarodik
            Paths["South_To_West"] = new List<Vector2> { new Vector2(40, 64), new Vector2(40, 24), new Vector2(0, 24) };
            // Balról jön, Északra (felfelé) kanyarodik
            Paths["West_To_North"] = new List<Vector2> { new Vector2(0, 40), new Vector2(40, 40), new Vector2(40, 0) };
            // Jobbról jön, Délre (lefelé) kanyarodik
            Paths["East_To_South"] = new List<Vector2> { new Vector2(64, 24), new Vector2(24, 24), new Vector2(24, 64) };
        }
    }
}