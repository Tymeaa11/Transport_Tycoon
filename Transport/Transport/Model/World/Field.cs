using Transport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Field
    {
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

        public FieldType Type { get; private set; } = FieldType.DEEP_LAVA_OCEAN;

        public ISurface? Surface { get; set; }

        public void SetFieldHeight(float height)
        {
            byte i = 0;
            while (i < MaxFieldTypeHeights.Length && height > MaxFieldTypeHeights[i]) i++;

            Type = (FieldType) i;
        }
        
        public bool IsBuildable() => Type > FieldType.LAVA_OCEAN && Surface == null;

        public int GetHeightDifference(Field? field) =>Type - field?.Type ?? 0;
    }
}
