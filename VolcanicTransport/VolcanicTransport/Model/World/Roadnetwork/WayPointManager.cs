using System.Numerics;

namespace VolcanicTransport.Model.World.Roadnetwork
{

    public enum PathDirection
    {
        North,
        South,
        East,
        West,
        Start,
        End
    }
    public static class WaypointManager
    {
        public const int TILE_SIZE = 32;

        private const float END = (float)TILE_SIZE;
        private const float MID = TILE_SIZE / 2f;

        private const float OFFSET = TILE_SIZE / 8f;
        private const float L1 = MID - OFFSET; // Belső sáv (bal/fent) -> 12
        private const float L2 = MID + OFFSET; // Külső sáv (jobb/lent) -> 20

        public static Dictionary<(PathDirection, PathDirection), List<Vector2>> Paths { get; private set; }

        static WaypointManager()
        {
            Paths = new Dictionary<(PathDirection, PathDirection), List<Vector2>>
            {
                // ==========================================
                // VÉGPONTOK (Beérkezés az állomásra)
                // ==========================================
                [(PathDirection.North, PathDirection.End)] = [new Vector2(L1, 0), new Vector2(L1, MID)],
                [(PathDirection.South, PathDirection.End)] = [new Vector2(L2, END), new Vector2(L2, MID)],
                [(PathDirection.West, PathDirection.End)] = [new Vector2(0, L2), new Vector2(MID, L2)],
                [(PathDirection.East, PathDirection.End)] = [new Vector2(END, L1), new Vector2(MID, L1)],

                [(PathDirection.North, PathDirection.Start)] = [new Vector2(L1, 0), new Vector2(L1, MID)],
                [(PathDirection.South, PathDirection.Start)] = [new Vector2(L2, END), new Vector2(L2, MID)],
                [(PathDirection.West, PathDirection.Start)] = [new Vector2(0, L2), new Vector2(MID, L2)],
                [(PathDirection.East, PathDirection.Start)] = [new Vector2(END, L1), new Vector2(MID, L1)],

                // ==========================================
                // STARTPONTOK (Indulás az állomásról)
                // ==========================================
                [(PathDirection.Start, PathDirection.North)] = [new Vector2(L2, MID), new Vector2(L2, 0)],
                [(PathDirection.Start, PathDirection.South)] = [new Vector2(L1, MID), new Vector2(L1, END)],
                [(PathDirection.Start, PathDirection.East)] = [new Vector2(MID, L2), new Vector2(END, L2)],
                [(PathDirection.Start, PathDirection.West)] = [new Vector2(MID, L1), new Vector2(0, L1)],

                [(PathDirection.End, PathDirection.North)] = [new Vector2(L2, MID), new Vector2(L2, 0)],
                [(PathDirection.End, PathDirection.South)] = [new Vector2(L1, MID), new Vector2(L1, END)],
                [(PathDirection.End, PathDirection.East)] = [new Vector2(MID, L2), new Vector2(END, L2)],
                [(PathDirection.End, PathDirection.West)] = [new Vector2(MID, L1), new Vector2(0, L1)],
                // ==========================================
                // 1. EGYENESEK
                // ==========================================
                [(PathDirection.North, PathDirection.South)] = [new Vector2(L1, 0), new Vector2(L1, END)],
                [(PathDirection.South, PathDirection.North)] = [new Vector2(L2, END), new Vector2(L2, 0)],
                [(PathDirection.West, PathDirection.East)] = [new Vector2(0, L2), new Vector2(END, L2)],
                [(PathDirection.East, PathDirection.West)] = [new Vector2(END, L1), new Vector2(0, L1)],

                [(PathDirection.North, PathDirection.North)] = [new Vector2(L1, 0), new Vector2(L1, MID), new Vector2(L1, END)],
                [(PathDirection.South, PathDirection.South)] = [new Vector2(L2, END), new Vector2(L2, MID), new Vector2(L2, 0)],
                [(PathDirection.West, PathDirection.West)] = [new Vector2(0, L2), new Vector2(MID, L2), new Vector2(END, L2)],
                [(PathDirection.East, PathDirection.East)] = [new Vector2(END, L1), new Vector2(MID, L1), new Vector2(0, L1)],
                // ==========================================
                // 2. JOBBRA KANYAROK
                // ==========================================
                [(PathDirection.North, PathDirection.West)] = [new Vector2(L1, 0), new Vector2(L1, L1), new Vector2(0, L1)],
                [(PathDirection.South, PathDirection.East)] = [new Vector2(L2, END), new Vector2(L2, L2), new Vector2(END, L2)],
                [(PathDirection.West, PathDirection.South)] = [new Vector2(0, L2), new Vector2(L1, L2), new Vector2(L1, END)],
                [(PathDirection.East, PathDirection.North)] = [new Vector2(END, L1), new Vector2(L2, L1), new Vector2(L2, 0)],

                // ==========================================
                // 3. BALRA KANYAROK
                // ==========================================
                [(PathDirection.North, PathDirection.East)] = [new Vector2(L1, 0), new Vector2(L1, L2), new Vector2(END, L2)],
                [(PathDirection.South, PathDirection.West)] = [new Vector2(L2, END), new Vector2(L2, L1), new Vector2(0, L1)],
                [(PathDirection.West, PathDirection.North)] = [new Vector2(0, L2), new Vector2(L2, L2), new Vector2(L2, 0)],
                [(PathDirection.East, PathDirection.South)] = [new Vector2(END, L1), new Vector2(L1, L1), new Vector2(L1, END)]
            };
        }
    }
}