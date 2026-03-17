using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World
{
    public class Field
    {
        public const int FieldSize = 32;

        private static readonly float[] MaxFieldTypeHeights =
        [
            20,
            80,
            100,
            190,
            200,
            290,
            300,
            400,
            450,
            50000
        ];

        public Coordinate Coordinate { get; set; }

        public float Height { get; private set; }

        public static readonly int FieldSize = 64;

        public FieldType Type { get; private set; } = FieldType.DEEP_LAVA_OCEAN;

        public ISurface? Surface { get; set; }

        public bool HasStation { get; set; } = false;

        public List<Vehicle> VehiclesOnField { get; } = new List<Vehicle>();

        public Vehicle? ReservedBy { get; set; } = null;


        public void SetFieldHeight(float height)
        {
            this.Height = height;
            byte i = 0;
            while (i < MaxFieldTypeHeights.Length && height > MaxFieldTypeHeights[i]) i++;

            Type = (FieldType) i;
        }
        
        public bool IsBuildable() => Type > FieldType.LAVA_OCEAN && Surface == null;

        public int GetHeightDifference(Field? field) =>Type - field?.Type ?? 0;
    }
}
