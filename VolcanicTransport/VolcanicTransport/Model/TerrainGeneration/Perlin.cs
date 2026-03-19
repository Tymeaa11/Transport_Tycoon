namespace VolcanicTransport.Model.TerrainGeneration
{
    public class Perlin
    {
        // Large part of this Perlin noise generation was taken from Processing 4 then converted to C# 

        #region Static

        private const int TwoPi = 720;
        private const int Pi = 360;
        private static readonly float[] CosTable = new float[720];

        static Perlin()
        {
            for (var i = 0; i < 720; ++i)
                CosTable[i] = (float)Math.Cos(i * 0.017453292F * 0.5F);
        }

        private static float Noise_fsc(float i)
            => 0.5F * (1.0F - CosTable[(int)(i * Pi) % TwoPi]);

        #endregion

        #region Fields

        private readonly float[] _perlin;
        private Random _perlinRandom;

        private int _octaves;
        private float _fallOff;
        #endregion

        #region Constructors
        public Perlin()
        {
            _perlin = new float[4096];
            _octaves = 4;
            _fallOff = 0.5f;
            _perlinRandom = new Random(World.World.Instance.WorldSeed);

            for (var i = 0; i < 4096; ++i)
            {
                _perlin[i] = _perlinRandom.NextSingle();
            }
        }

        public Perlin(int lod, float fallOff) : this()
        {
            NoiseDetail(lod, fallOff);
        }
        #endregion

        #region Methods
        public float Noise(float x, float y = 0.0F, float z = 0.0F)
        {

            if (x < 0.0F) x = -x;
            if (y < 0.0F) y = -y;
            if (z < 0.0F) z = -z;

            var xi = (int)x;
            var yi = (int)y;
            var zi = (int)z;
            var xf = x - xi;
            var yf = y - yi;
            var zf = z - zi;
            var r = 0.0F;
            var ampl = 0.5F;

            for (var i = 0; i < _octaves; ++i)
            {
                var of = xi + (yi << 4) + (zi << 8);
                var rxf = Noise_fsc(xf);
                var ryf = Noise_fsc(yf);
                var n1 = _perlin[of & 4095];
                n1 += rxf * (_perlin[of + 1 & 4095] - n1);
                var n2 = _perlin[of + 16 & 4095];
                n2 += rxf * (_perlin[of + 16 + 1 & 4095] - n2);
                n1 += ryf * (n2 - n1);
                of += 256;
                n2 = _perlin[of & 4095];
                n2 += rxf * (_perlin[of + 1 & 4095] - n2);
                var n3 = _perlin[of + 16 & 4095];
                n3 += rxf * (_perlin[of + 16 + 1 & 4095] - n3);
                n2 += ryf * (n3 - n2);
                n1 += Noise_fsc(zf) * (n2 - n1);
                r += n1 * ampl;
                ampl *= _fallOff;
                xi <<= 1;
                xf *= 2.0F;
                yi <<= 1;
                yf *= 2.0F;
                zi <<= 1;
                zf *= 2.0F;

                if (xf >= 1.0F)
                {
                    ++xi;
                    --xf;
                }

                if (yf >= 1.0F)
                {
                    ++yi;
                    --yf;
                }

                if (zf >= 1.0F)
                {
                    ++zi;
                    --zf;
                }
            }

            return r;
        }

        public void NoiseDetail(int lod)
        {
            if (lod > 0)
            {
                _octaves = lod;
            }
        }

        public void NoiseDetail(int lod, float falloff)
        {
            if (lod > 0)
            {
                _octaves = lod;
            }

            if (falloff > 0.0F)
            {
                _fallOff = falloff;
            }

        }

        public void NoiseSeed(int seed)
        {
            _perlinRandom = new Random(seed);

            for (var i = 0; i < 4096; ++i)
            {
                _perlin[i] = _perlinRandom.NextSingle();
            }
        }
        #endregion
    }
}
