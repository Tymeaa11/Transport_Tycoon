using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model
{
    internal interface IContainsReference
    {
        void RestoreReference(Coordinate coordinate);
    }
}
