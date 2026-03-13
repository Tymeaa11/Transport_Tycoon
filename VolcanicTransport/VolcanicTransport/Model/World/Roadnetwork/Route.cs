using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport.Model.World.Roadnetwork
{
    public class Route
    {
        private List<Station> stops;
        private List<Road> roadToNextStation;
        private bool isLoop;

        public Route()
        {
            stops = new List<Station>();
            roadToNextStation = new List<Road>();
            isLoop = false;
        }

        public Station GetNextStop(Station current)
        {
            return current; //javítandó
        }

        public List<Road> GetRoadsToNextStation()
        {
            return roadToNextStation; //javítandó
        }
    }
}
