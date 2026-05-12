// ReSharper disable InconsistentNaming
namespace VolcanicTransport.Model.World
{
    [Flags]
    public enum RoadType : byte
    {
        TYPE_MASK = 0b1111_0000,
        DIRECTION_MASK = 0b0000_1111,
        INVALID = 0b1111_1111,
        // scjr_NSEW
        // s = straight, c = curved, j = junction, r = slope 
        STRAIGHT = 0b1000_0000,
        SLOPE = 0b1001_0000,
        CURVED = 0b0100_0000,
        JUNCTION = 0b0010_0000,

        // 0 sides
        LONELY = 0,

        // 1 side
        NORTH = 0b0000_1000,
        SOUTH = 0b0000_0100,
        EAST = 0b0000_0010,
        WEST = 0b0000_0001,

        // 2 sides
        STRAIGHT_NS = STRAIGHT | NORTH | SOUTH,
        STRAIGHT_EW = STRAIGHT | EAST | WEST,

        CURVED_NE = CURVED | NORTH | EAST,
        CURVED_ES = CURVED | EAST | SOUTH,
        CURVED_SW = CURVED | SOUTH | WEST,
        CURVED_WN = CURVED | NORTH | WEST,

        // 3 sides
        JUNCTION_T_WNE = JUNCTION | WEST | NORTH | EAST,
        JUNCTION_T_NES = JUNCTION | NORTH | EAST | SOUTH,
        JUNCTION_T_ESW = JUNCTION | EAST | SOUTH | WEST,
        JUNCTION_T_SWN = JUNCTION | SOUTH | WEST | NORTH,

        // 4 sides
        JUNCTION_NESW = JUNCTION | NORTH | SOUTH | EAST | WEST,

        // slope

        SLOPE_N = SLOPE | NORTH,
        SLOPE_E = SLOPE | EAST,
        SLOPE_W = SLOPE | WEST,
        SLOPE_S = SLOPE | SOUTH,

        SLOPE_NS = SLOPE | NORTH | SOUTH,
        SLOPE_EW = SLOPE | EAST | WEST
    }
}
