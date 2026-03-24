namespace VolcanicTransport.Model.TerrainGeneration.Layers
{
    public interface ILayer : ISeedable
    {
        public float Get(float x, float y);
    }
}
