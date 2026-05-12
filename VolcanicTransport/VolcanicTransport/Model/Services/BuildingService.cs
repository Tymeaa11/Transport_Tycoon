using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;
using VolcanicTransport.Model.World.Roadnetwork;

namespace VolcanicTransport.Model.Services
{
    public class BuildingService
    {
        private readonly Func<World.World> _getWorld;
        private readonly EconomyService _economy;

        private World.World WorldInstance => _getWorld();

        public event EventHandler? OnPlacementFailed;
        public event EventHandler? RoadBought;

        public BuildingService(Func<World.World> getWorld, EconomyService economy)
        {
            _getWorld = getWorld;
            _economy = economy;
        }

        public bool IsBuildable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsBuildable() ?? false;
        public bool IsHeightenable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsHeightenable() ?? false;
        public bool IsLowerable(Coordinate coordinate) => WorldInstance.GetField(coordinate)?.IsLowerable() ?? false;



        private void CheckAndRegisterJunctions(Coordinate centerCoord)
        {
            Coordinate[] coordsToCheck = [
                centerCoord,
                new(centerCoord.X, centerCoord.Y - 1),
                new(centerCoord.X, centerCoord.Y + 1),
                new(centerCoord.X + 1, centerCoord.Y),
                new(centerCoord.X - 1, centerCoord.Y)
            ];

            foreach (var c in coordsToCheck)
            {
                var surface = WorldInstance.GetField(c)?.Surface;
                if (surface is Road r && r.RoadType.HasFlag(RoadType.JUNCTION) || surface is Station)
                    WorldInstance.Roadnetwork.RegisterNodeIfNeeded(c);
            }
        }

        private Road? CanPlaceRoadHere(Coordinate coord, Field? field)
        {
            if (field == null || !field.IsBuildable()) return null;

            foreach (var dir in Coordinate.Directions)
            {
                if (WorldInstance.GetField(coord + dir)?.Surface is Bridge)
                {
                    System.Diagnostics.Debug.WriteLine("Építés megtagadva: Híd mellé nem kerülhet út!");
                    return null;
                }
            }

            Road tempRoad = new(coord);
            field.Surface = tempRoad;
            tempRoad.Update();

            if (tempRoad.RoadType != RoadType.INVALID) return tempRoad;

            field.Surface = null;
            OnPlacementFailed?.Invoke(this, EventArgs.Empty);
            return null;
        }

        public void PlaceRoad(Coordinate coord)
        {
            var roadPrice = GameSettings.BaseRoadPrice;
            var field = WorldInstance.GetField(coord);
            if (field == null) return;

            roadPrice += EconomyService.GetMushroomCosts(field);

            if (!_economy.TryPurchase(roadPrice)) return;

            var road = CanPlaceRoadHere(coord, field);
            if (road == null)
            {
                _economy.AddMoney(roadPrice);
                return;
            }

            if (!road.TryUpdateNeighbours())
            {
                field.Surface = null;
                road.UpdateNeighbours();
                _economy.AddMoney(roadPrice);
                OnPlacementFailed?.Invoke(this, EventArgs.Empty);
                return;
            }

            road.Update();
            WorldInstance.UpdateRoadNetworkAround(coord);
            CheckAndRegisterJunctions(coord);
            WorldInstance.Roadnetwork.RebuildEdges();
            RoadBought?.Invoke(this, EventArgs.Empty);
        }

        public bool PlaceBridge(Coordinate start, Coordinate end, GameSettings.BridgeData bridgeType)
        {
            if (start.X != end.X && start.Y != end.Y) return false;

            int dx = Math.Abs(start.X - end.X);
            int dy = Math.Abs(start.Y - end.Y);
            int length = Math.Max(dx, dy) + 1;

            if (length < 3 || length > bridgeType.Length) return false;

            Field? startField = WorldInstance.GetField(start);
            Field? endField = WorldInstance.GetField(end);

            if (startField == null || endField == null) return false;
            if (startField.Type != endField.Type) return false;

            var tempStartRoad = new Road(start);
            var tempEndRoad = new Road(end);
            var originalStartSurface = startField.Surface;
            var originalEndSurface = endField.Surface;

            startField.Surface = tempStartRoad;
            endField.Surface = tempEndRoad;
            tempStartRoad.Update();
            tempEndRoad.Update();

            bool bridgeHeadsValid = tempStartRoad.RoadType != RoadType.INVALID &&
                                    tempEndRoad.RoadType != RoadType.INVALID &&
                                    tempStartRoad.TryUpdateNeighbours() &&
                                    tempEndRoad.TryUpdateNeighbours();

            if (!bridgeHeadsValid)
            {
                startField.Surface = originalStartSurface;
                endField.Surface = originalEndSurface;
                tempStartRoad.UpdateNeighbours();
                tempEndRoad.UpdateNeighbours();
                return false;
            }

            int stepX = start.X == end.X ? 0 : (end.X > start.X ? 1 : -1);
            int stepY = start.Y == end.Y ? 0 : (end.Y > start.Y ? 1 : -1);
            RoadType bridgeDir = stepX == 0 ? RoadType.STRAIGHT_NS : RoadType.STRAIGHT_EW;

            List<Coordinate> bridgeCoords = [];
            for (int i = 0; i < length; i++)
            {
                var c = new Coordinate(start.X + i * stepX, start.Y + i * stepY);
                bridgeCoords.Add(c);
                Field? f = WorldInstance.GetField(c);

                if (i > 0 && i < length - 1)
                {
                    if (f == null || f.Type >= startField.Type || f.Surface is Road)
                    {
                        startField.Surface = originalStartSurface;
                        endField.Surface = originalEndSurface;
                        tempStartRoad.UpdateNeighbours();
                        tempEndRoad.UpdateNeighbours();
                        return false;
                    }
                }
            }

            for (int i = 1; i < length - 1; i++)
            {
                Coordinate c = bridgeCoords[i];
                foreach (var dir in Coordinate.Directions)
                {
                    Coordinate adjCoord = c + dir;
                    if (bridgeCoords.Contains(adjCoord)) continue;
                    if (WorldInstance.GetField(adjCoord)?.Surface is Road)
                    {
                        startField.Surface = originalStartSurface;
                        endField.Surface = originalEndSurface;
                        tempStartRoad.UpdateNeighbours();
                        tempEndRoad.UpdateNeighbours();
                        return false;
                    }
                }
            }

            double actualPrice = (bridgeType.Price / bridgeType.Length) * length;
            if (!_economy.TryPurchase(actualPrice))
            {
                startField.Surface = originalStartSurface;
                endField.Surface = originalEndSurface;
                tempStartRoad.UpdateNeighbours();
                tempEndRoad.UpdateNeighbours();
                return false;
            }

            for (int i = 0; i < length; i++)
            {
                Coordinate c = bridgeCoords[i];
                Field field = WorldInstance.GetField(c)!;

                if (i == 0 || i == length - 1)
                    PlaceRoad(c);
                else
                    field.Surface = bridgeType.Tier switch
                    {
                        0 => new BoneBridge(c, bridgeDir, startField.Type),
                        1 => new StoneBridge(c, bridgeDir, startField.Type),
                        2 => new SteelBridge(c, bridgeDir, startField.Type),
                        _ => throw new NotImplementedException()
                    };
            }

            foreach (var c in bridgeCoords)
            {
                if (WorldInstance.GetField(c)?.Surface is Road r) r.Update();
                WorldInstance.UpdateRoadNetworkAround(c);
                CheckAndRegisterJunctions(c);
            }

            WorldInstance.Roadnetwork.RebuildEdges();
            RoadBought?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool PlaceStation(Coordinate coord)
        {
            var stationCost = GameSettings.BaseStationPrice;

            if (!IsBuildable(coord) || _economy.PlayerMoney < stationCost) return false;
            if (WorldInstance.Stations.Any(s => s.Coordinate.Distance(coord) <= 3)) return false;

            var field = WorldInstance.GetField(coord);
            if (field == null) return false;

            var hasValidNearRoad = Coordinate.Directions.Any(dir =>
            {
                var neighbor = WorldInstance.GetField(coord + dir);
                return neighbor?.Surface is Road && neighbor.Type == field.Type;
            });

            if (!hasValidNearRoad) return false;

            stationCost += EconomyService.GetMushroomCosts(field);

            var city = WorldInstance.Cities.FirstOrDefault(c => c.CenterCoordinate.Distance(coord) <= 4);
            var factory = WorldInstance.Factories.FirstOrDefault(f => f.OriginCoordinate.Distance(coord) <= 4);

            Station? newStation = null;
            if (city != null) newStation = new CityStation(city, coord, city.Name + " megálló");
            else if (factory != null) newStation = new FactoryStation(coord, factory.Name + " megálló", factory);

            if (newStation == null || !_economy.TryPurchase(stationCost)) return false;

            field.Surface = newStation;
            newStation.Update();

            if (!newStation.TryUpdateNeighbours())
            {
                field.Surface = null;
                newStation.UpdateNeighbours();
                _economy.AddMoney(stationCost);
                return false;
            }

            WorldInstance.Stations.Add(newStation);
            WorldInstance.UpdateRoadNetworkAround(coord);
            CheckAndRegisterJunctions(coord);
            WorldInstance.Roadnetwork.RebuildEdges();

            var chunkCoord = WorldInstance.GetChunkCoordinate(coord);
            WorldInstance.UpdateChunk(chunkCoord);

            return true;
        }

        public void HeightenField(Coordinate coord) => TerraformField(coord, +1);
        public void LowerField(Coordinate coord) => TerraformField(coord, -1);

        private void TerraformField(Coordinate coord, int deltaHeight)
        {
            var terraformationPrice = GameSettings.BaseTerraformationPrice;
            var field = WorldInstance.GetField(coord);
            if (field == null) return;

            if (!((deltaHeight == -1 && field.IsLowerable()) || (deltaHeight == 1 && field.IsHeightenable()))) return;

            terraformationPrice += EconomyService.GetMushroomCosts(field);
            var newFieldType = (FieldType)((int)field.Type + deltaHeight);

            if (newFieldType > FieldType.HIGH_MOUNTAINS) return;
            if (!_economy.TryPurchase(terraformationPrice)) return;

            field.SetFieldTypeTo(newFieldType);
            WorldInstance.UpdateChunk(WorldInstance.GetChunkCoordinate(coord));
        }
    }
}
