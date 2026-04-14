using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model
{
    public class ChunkUpdatedEventArgs(Coordinate coordinate) : EventArgs
    {
        public Coordinate ChunkCoordinate { get; set; } = coordinate;
    }
}
