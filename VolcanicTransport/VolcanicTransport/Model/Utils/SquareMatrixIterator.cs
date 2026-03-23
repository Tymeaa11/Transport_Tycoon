namespace VolcanicTransport.Model.Utils
{
    public class SquareMatrixIterator<T>(int size)
    {

        private readonly T[,] _matrix = new T[size, size];
        public int Size => size;

        public T this[Coordinate index]
        {
            get => index.IsInside(Size) ? _matrix[index.Y, index.X] : throw new IndexOutOfRangeException();
            set
            {
                if (index.IsInside(Size)) throw new IndexOutOfRangeException();
                _matrix[index.Y, index.X] = value;
            }
        }

        public void ReadEach(Action<int, int, T> action)
        {
            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    action(x, y, _matrix[y, x]);
        }

        public void ModifyEach(Func<int, int, T, T> func)
        {
            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    _matrix[y, x] = func(x, y, _matrix[y, x]);
        }

        public void SetEach(Func<int, int, T> func)
        {
            for (var y = 0; y < Size; y++)
                for (var x = 0; x < Size; x++)
                    _matrix[y, x] = func(x, y);
        }
    }
}
