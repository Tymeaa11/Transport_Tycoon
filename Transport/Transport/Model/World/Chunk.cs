using Transport.Model.Utils;

namespace Transport.Model.World
{
    public class Chunk(Coordinate coordinate)
    {
        public static readonly int ChunkSize = 32;
        
        public Coordinate Coordinate { get; init; } = coordinate;

        public SquareMatrixIterator<Field> FieldMatrix { get; init; } = new(ChunkSize);
    }
}
