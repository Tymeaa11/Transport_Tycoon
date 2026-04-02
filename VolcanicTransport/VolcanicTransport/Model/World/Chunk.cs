using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.World
{
    public class Chunk(Coordinate coordinate)
    {
        #region Fields
        public Coordinate Coordinate { get; init; } = coordinate;
        public SquareMatrixIterator<Field> FieldMatrix { get; } = new(GameSettings.ChunkSize);
        #endregion
        
        #region UpdateEvent
        public event EventHandler? Changed;
        public void TriggerRerender() => Changed?.Invoke(this, EventArgs.Empty);
        public void RemoveAllUpdateTriggers() { Changed = null; }
        #endregion
    }
}
