using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.TerrainGeneration.Generators;

namespace VolcanicTransport.Model.TerrainGeneration
{
    public interface IWorldGenerator : ISeedable, ITerrainGenerator
    {
        public void GenerateCitiesAndFactories();
    }
}
