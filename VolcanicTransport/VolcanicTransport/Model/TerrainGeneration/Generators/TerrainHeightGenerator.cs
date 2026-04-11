using VolcanicTransport.Model.TerrainGeneration.Layers;
using VolcanicTransport.Model.World;

namespace VolcanicTransport.Model.TerrainGeneration.Generators
{
    public class TerrainHeightGenerator : ITerrainGenerator
    {

        private readonly Perlin _perlin;
        private readonly LayeredTerrain _positiveTerrain;
        private readonly LayeredTerrain _rivers;
        private readonly Falloff _falloff;

        public TerrainHeightGenerator()
        {
            _perlin = new Perlin();
            _falloff = new Falloff();
            _positiveTerrain = new LayeredTerrain();
            _rivers = new LayeredTerrain();

            Initialise();
        }

        private PerlinLayer PLayer(float frequency, float amplitude)
            => new(_perlin, frequency, amplitude, World.World.Instance.SharedRandom);

        private void Initialise()
        {
            var p1 = PLayer(0.03f, 1.0f);
            var p2 = PLayer(0.1f, .2f);
            var p3 = PLayer(0.2f, .1f);

            _positiveTerrain.AddLayer([p1, p2, p3]);

            var m1 = PLayer(0.01f, 0.6f);
            var m2 = PLayer(0.02f, 0.3f);
            var m3 = PLayer(0.05f, 0.2f);


            _rivers.AddLayer([m1, m2, m3]);


            //var w1 = new PerlinLayer(perlin, 0.005f, 255, GetOffset(), GetOffset());
            //var w2 = new PerlinLayer(perlin, 0.01f, 255, GetOffset(), GetOffset());
            //var w3 = new PerlinLayer(perlin, 0.1f, 255, GetOffset(), GetOffset());

            //var warp = new CompositeLayer(w3, w2, w1);
        }

        public void ModifyField(Field field, int x, int y)
        {
            // delete what was on this field
            field.Surface = null;

            double h = _positiveTerrain.Get(x, y);

            // apply falloff
            h += _falloff.Get(x, y);


            // make volcanos
            h = h < 0.78 ? Math.Pow(h, 1.4) : Math.Pow(-4 * h + 4, 3);

            h *= 900;

            // carve
            var neg = Math.Pow(_rivers.Get(x, y), 0.9);

            if (neg is > 0 and < 1)
                neg = Math.Pow(1 - 2 * Math.Abs(neg - 0.5), 10);
            else
                neg = 100000;

            h -= neg * 600;

            // finalize
            field.SetFieldHeight((int)h);
        }

        public void SetSeed(int seed, Random nextRandom)
        {
            _perlin.SetSeed(seed, nextRandom);
            _positiveTerrain.SetSeed(seed, nextRandom);
            _rivers.SetSeed(seed, nextRandom);
        }
    }
}
