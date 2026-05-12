using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.Persistance
{
    internal interface IContainsReference
    {
        void RestoreReference(Coordinate coordinate);
    }
}
