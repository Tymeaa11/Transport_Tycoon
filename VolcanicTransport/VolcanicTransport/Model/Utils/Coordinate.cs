using VolcanicTransport.Model.World;
using static VolcanicTransport.Model.TerrainGeneration.FactoryAndCityGenerator;

namespace VolcanicTransport.Model.Utils
{
    public readonly struct Coordinate(int x, int y) : IEquatable<Coordinate>
    {
        #region  Fields
        public int X { get; } = x;
        public int Y { get; } = y;
        #endregion

        #region Constructors
        public Coordinate(Coordinate c) : this(c.X, c.Y) { }
        public Coordinate(int value) : this(value, value) { }
        public Coordinate() : this(0, 0) { }
        #endregion

        public double Magnitude => Math.Sqrt(X * X + Y * Y);

        public double Distance(Coordinate o) => (this-o).Magnitude;

        public bool IsInside(Coordinate topLeft, Coordinate bottomRight)
        => X >= topLeft.X && X < bottomRight.X && Y >= topLeft.Y && Y < bottomRight.Y;

        public bool IsInside(Coordinate bottomRight)
            => X >= 0 && X < bottomRight.X && Y >= 0 && Y < bottomRight.Y;

        public bool IsInside(int bottomRight)
            => X >= 0 && X < bottomRight && Y >= 0 && Y < bottomRight;

        public override string ToString() => $"({X},{Y})";


        public static List<Coordinate> GetArea(Coordinate topLeft, Coordinate topRight)
        {
            if (!topLeft.IsInside(topRight)) 
                return [];

            List<Coordinate> coords = [];
            for (int y = topLeft.Y; y <= topRight.Y; y++)
                for (int x = topLeft.X; x <= topRight.X; x++)
                    coords.Add(new(x, y));

            return coords;
        }


        #region Equals & HashCode
        public override bool Equals(object? obj)
            => obj is Coordinate coordinate && X == coordinate.X && Y == coordinate.Y;
        public bool Equals(Coordinate other) => X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);
        #endregion

        #region Operators
        public static Coordinate operator +(Coordinate c1, Coordinate c2)
            => new(c1.X + c2.X, c1.Y + c2.Y);

        public static Coordinate operator +(Coordinate c1, int c)
             => new(c1.X + c, c1.Y + c);

        public static Coordinate operator -(Coordinate c1, Coordinate c2)
            => new(c1.X - c2.X, c1.Y - c2.Y);

        public static Coordinate operator -(Coordinate c1, int c)
            => new(c1.X - c, c1.Y - c);

        public static Coordinate operator *(Coordinate c1, Coordinate c2)
            => new(c1.X * c2.X, c1.Y * c2.Y);

        public static Coordinate operator *(Coordinate c1, int c2)
            => new(c1.X * c2, c1.Y * c2);

        public static Coordinate operator /(Coordinate c1, Coordinate c2)
            => new(c1.X / c2.X, c1.Y / c2.Y);

        public static Coordinate operator /(Coordinate c1, int c2)
            => new(c1.X / c2, c1.Y / c2);

        public static Coordinate operator %(Coordinate c1, Coordinate c2)
            => new(c1.X % c2.X, c1.Y % c2.Y);

        public static Coordinate operator %(Coordinate c1, int c2)
            => new(c1.X % c2, c1.Y % c2);


        public static bool operator ==(Coordinate left, Coordinate right)
            => left.Equals(right);

        public static bool operator !=(Coordinate left, Coordinate right)
            => !(left == right);
        #endregion
    }
}
