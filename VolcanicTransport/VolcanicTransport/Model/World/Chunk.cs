using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Chunk(Coordinate coordinate)
    {
        #region Fields
        public Coordinate Coordinate { get; init; } = coordinate;
        public SquareMatrixIterator<Field> FieldMatrix { get; } = new(GameSettings.ChunkSize);
        #endregion
    }
}
