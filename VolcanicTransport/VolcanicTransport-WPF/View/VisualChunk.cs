using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VolcanicTransport.Model;
using VolcanicTransport.Model.World;
using VolcanicTransport.Model.World.Economy;

namespace VolcanicTransport_WPF.View
{
    public class VisualChunk : FrameworkElement
    {

        private static readonly int Dpi = 96;
        private static readonly int FieldSize = GameSettings.FieldSize;
        private static readonly int HalfFieldSize = FieldSize / 2;
        private static readonly int ChunkSizeInFields = GameSettings.ChunkSize * FieldSize;
        private static readonly Rect ChunkBoundries = new(0, 0, ChunkSizeInFields, ChunkSizeInFields);

        private readonly DrawingVisual _visual;

        public VisualChunk()
        {
            _visual = new DrawingVisual();
            AddVisualChild(_visual);

            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            DataContextChanged += (s, e) =>
            {
                if (e.OldValue is Chunk oldChunk)
                {
                    oldChunk.Changed -= OnChunkDataChanged;
                }

                if (e.NewValue is Chunk newChunk)
                {
                    newChunk.Changed += OnChunkDataChanged;
                    Dispatcher.InvokeAsync(() => PreRender(newChunk));
                }
            };
        }

        private void OnChunkDataChanged(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                if (DataContext is Chunk chunkData)
                {

                    PreRender(chunkData);
                }
            });
        }

        public void PreRender(Chunk chunkData)
        {

            RenderTargetBitmap bakedMap = new(
                ChunkSizeInFields, ChunkSizeInFields, Dpi, Dpi, PixelFormats.Pbgra32
            );

            DrawingVisual dv = new();
            using (DrawingContext dc = dv.RenderOpen())
            {
                chunkData.FieldMatrix.ReadEach((x, y, f) =>
                {
                    // Draw the tile based on FieldType
                    Brush brush = FieldBrushProvider.GetBrush(f.Type);
                    double fieldX = x * FieldSize;
                    double fieldY = y * FieldSize;

                    Rect rectangle = new(fieldX, fieldY, FieldSize, FieldSize);

                    dc.DrawRectangle(brush, null, rectangle);

                    // Draw Surface
                    if (f.Surface != null)
                    {
                        ImageWithRotation imageWithRotation = f.Surface switch
                        {
                            Mushroom m => RenderMushroom(m),
                            Road r => RenderRoad(r),
                            Station s => RenderStation(),
                            FactoryBuilding => RenderFactoryBuilding(),
                            CityBuilding => RenderCityBuilding(),
                            _ => RenderInvalid()
                        };

                        double centerX = fieldX + HalfFieldSize;
                        double centerY = fieldY + HalfFieldSize;

                        dc.PushTransform(new RotateTransform(imageWithRotation.AngleDegrees, centerX, centerY));
                        dc.DrawImage(imageWithRotation.ImageSource, rectangle);
                        dc.Pop();
                    }
                });
            }

            bakedMap.Render(dv);
            bakedMap.Freeze();

            // Draw the baked chunk to _visual
            using DrawingContext dc2 = _visual.RenderOpen();
            dc2.DrawImage(bakedMap, ChunkBoundries);
        }

        private ImageWithRotation RenderInvalid()
            => new(TextureAtlas.InvalidTexture, 0);
        private ImageWithRotation RenderMushroom(Mushroom m)
            => new(TextureAtlas.MushroomTextures[(int)m.GrowthStage], 0);
        private ImageWithRotation RenderRoad(Road r)
            => TextureAtlas.RoadTextures[r.RoadType];
        private ImageWithRotation RenderCityBuilding()
            => new(TextureAtlas.CityBuildingTexture, 0);
        private ImageWithRotation RenderFactoryBuilding()
            => new(TextureAtlas.FactoryBuildingTexture, 0);
        private ImageWithRotation RenderStation()
            => new(TextureAtlas.StationTexture, 0);

        protected override int VisualChildrenCount => 1;
        protected override Visual GetVisualChild(int index) => _visual;
    }
}
