using System.Runtime.CompilerServices;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.World
{
    public class Field
    {
        public static readonly int FieldSize = 32;

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
            520,
            50000
        ];


        public FieldType Type { get; private set; } = FieldType.DEEP_LAVA_OCEAN;

        public ISurface? Surface { get; set; }


        public void SetFieldTypeTo(Field f) => Type = f.Type;
        public void SetFieldTypeTo(FieldType ftype) => Type = ftype;
        public void SetFieldHeight(float height)
        {
            int i = 0;
            while (i < MaxFieldTypeHeights.Length && height > MaxFieldTypeHeights[i]) i++;

            Type = (FieldType)i;
        }

        public bool IsBuildable() => Type > FieldType.LAVA_OCEAN && (Surface == null  || Surface is Mushroom);
        public bool IsLowerable() => Type > FieldType.DEEP_LAVA_OCEAN && (Surface == null || Surface is Mushroom);
        public bool IsHeightenable() => Type < FieldType.HIGH_MOUNTAINS && (Surface == null || Surface is Mushroom);
        public int GetHeightDifference(Field? field) => Type - field?.Type ?? 0;
    }
}
