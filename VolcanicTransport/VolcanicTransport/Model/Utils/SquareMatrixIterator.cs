namespace VolcanicTransport.Model.Utils
{
    public class SquareMatrixIterator<T>(int size)
    {
        #region Fields
        private readonly T[,] _matrix = new T[size, size];
        #endregion

        #region Constructors
        public T this[Coordinate index]
        {
            get => index.IsInside(size) ? _matrix[index.Y, index.X] : throw new IndexOutOfRangeException();
            set
            {
                if (index.IsInside(size)) throw new IndexOutOfRangeException();
                _matrix[index.Y, index.X] = value;
            }
        }
        #endregion

        #region Methods
        public void ReadEach(Action<int, int, T> action)
        {
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    action(x, y, _matrix[y, x]);
        }

        public void ModifyEach(Func<int, int, T, T> func)
        {
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    _matrix[y, x] = func(x, y, _matrix[y, x]);
        }

        public void SetEach(Func<int, int, T> func)
        {
            for (var y = 0; y < size; y++)
                for (var x = 0; x < size; x++)
                    _matrix[y, x] = func(x, y);
        }
        #endregion
    }
}
