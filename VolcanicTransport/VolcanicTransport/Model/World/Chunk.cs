using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Chunk(Coordinate coordinate)
    {
        public static readonly int ChunkSize = 32;

        public Coordinate Coordinate { get; init; } = coordinate;

        public SquareMatrixIterator<Field> FieldMatrix { get; init; } = new(ChunkSize);

        public event EventHandler? Changed;
        public void TriggerRerender() => Changed?.Invoke(this, EventArgs.Empty);
        public void RemoveAllUpdateTriggers() { Changed = null; }
    }
}
