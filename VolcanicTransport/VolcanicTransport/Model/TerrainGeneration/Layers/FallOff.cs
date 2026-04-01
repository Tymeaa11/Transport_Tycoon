namespace VolcanicTransport.Model.TerrainGeneration.Layers
{
    internal class Falloff() : ILayer
    {
        public float Get(float x, float y)
        {
            var wWidthP2 = World.World.Instance.SizeInFields.X * 0.5f;
            var wHeightP2 = World.World.Instance.SizeInFields.Y * 0.5f;

            x -= wWidthP2;
            y -= wHeightP2;

            return (float)(
                    Math.Pow(
                        1 - Math.Min(wWidthP2 - Math.Abs(x), wHeightP2 - Math.Abs(y)) / Math.Max(wWidthP2, wHeightP2), 
                        10));
        }

        public void SetSeed(int seed, Random nextRandom) {}
    }
}
