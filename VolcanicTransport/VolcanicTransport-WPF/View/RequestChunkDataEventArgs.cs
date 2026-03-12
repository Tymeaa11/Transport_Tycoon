using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.ViewModel
{
    public class RequestChunkDataEventArgs(Coordinate coordinate) : EventArgs
    {
        public Coordinate Coordinate { get; private init; } = coordinate;
    }
}
