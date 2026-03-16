using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public class TerrainHeightGenerator : ITerrainGenerator
    {
        private readonly FinalizeNoise _finalNoise;

        private class Falloff : ILayer
        {
            public float Get(float x, float y)
            {
                var wWidthP2 = World.World.Instance.SizeInFields.X * 0.5f;
                var wHeightP2 = World.World.Instance.SizeInFields.Y * 0.5f;
                
                x = x - wWidthP2;
                y = y - wHeightP2;
                
                return (float) (-1500 * Math.Pow(1 - (Math.Min( wWidthP2 - Math.Abs(x),wHeightP2 - Math.Abs(y) ) ) / Math.Max( wWidthP2, wHeightP2), 3));
            }
        }

        private class FinalizeNoise(LayeredTerrain baseNoise, CompositeLayer warp, PerlinLayer filter)
            : ILayer
        {
            public float Get(float x, float y)
            {
                var h = baseNoise.Get(x, y);
                return h is < 100.0f and > -50f  ? h + warp.Get(x,y) * filter.Get(x,y) : h % 500;
            }

            public void SetSeed(int seed)
            {
                baseNoise.SetSeed(seed);
                warp.SetSeed(seed);
                filter.SetSeed(seed);
            }
        }
        
        
        public TerrainHeightGenerator()
        {
            var perlin = new Perlin();
            var fallOff = new Falloff();
            var baseNoise = new LayeredTerrain();
            
            
            var p1 = new PerlinLayer(perlin, 0.005f, 700, 0, 0);
            var p2 = new PerlinLayer(perlin, 0.01f, 400, 0, 0);
            var p3 = new PerlinLayer(perlin, 0.02f, 200, 0, 0);
            var p4 = new PerlinLayer(perlin, 0.05f, 50, 0, 0);


            var m1 = new PerlinLayer(perlin, 0.01f, -300, 0, 0);
            var m2 = new PerlinLayer(perlin, 0.005f, -250, 0, 0);


            baseNoise.AddLayer(fallOff);
            baseNoise.AddLayer([p1,p2,p3,p4,m1,m2]);


            var w1 = new PerlinLayer(perlin, 0.005f, 255, 0, 0);
            var w2 = new PerlinLayer(perlin, 0.01f, 255, 0, 0);
            var w3 = new PerlinLayer(perlin, 0.1f, 255, 0, 0);

            var warp = new CompositeLayer(w3, w2, w1);

            var c1 = new PerlinLayer(perlin, 0.01f, 1.5f, 300, 200);


            _finalNoise = new FinalizeNoise(baseNoise, warp, c1);
        }

        public void ModifyField(Field field, int x, int y)
        {
            var f = _finalNoise.Get(x, y);
            field.SetFieldHeight((int)  f);
        }
    }
}
