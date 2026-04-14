using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolcanicTransport.Model.Utils
{
    public class FieldsEventArgs(List<Coordinate> coordinates) : EventArgs
    {
        public List<Coordinate> ChangedCoordinates { get; } = coordinates;
    }
}
