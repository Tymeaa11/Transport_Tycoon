using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface ISeedable
    {
        public void SetSeed(int seed, Random nextRandom);
    }
}
