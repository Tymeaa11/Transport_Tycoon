namespace VolcanicTransport.Model.TerrainGeneration
{
    public class LayeredTerrain : ILayer
    {
        #region Fields
        private readonly List<ILayer> _layers = [];
        #endregion
        
        #region Constructors
        public LayeredTerrain(List<ILayer> layers) => _layers = layers;
        public LayeredTerrain() {}
        #endregion
        
        #region Methods
        public void AddLayer(ILayer layer) => _layers.Add(layer);
        public void AddLayer(ILayer[] layers) => _layers.AddRange(layers);
        public float Get(float x, float y) => _layers.Sum(layer => layer.Get(x, y));
        public void SetSeed(int seed) => _layers.ForEach(layer => layer.SetSeed(seed));
        #endregion
    }
}
