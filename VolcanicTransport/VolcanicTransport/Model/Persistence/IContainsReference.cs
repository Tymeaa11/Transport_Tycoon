using VolcanicTransport.Model.Utils;

namespace VolcanicTransport.Model.Persistence
{
    internal interface IContainsReference
    {
        void RestoreReference(Coordinate coordinate);
    }
}
